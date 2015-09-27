﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Core.MicroThreading
{
    /// <summary>
    /// Scheduler that manage a group of cooperating <see cref="MicroThread"/>.
    /// </summary>
    /// <remarks>
    /// Microthreading provides a way to execute many small execution contexts who cooperatively yield to each others.
    /// </remarks>
    public class Scheduler
    {
        internal readonly static Logger Log = GlobalLogger.GetLogger("Scheduler");

        // An ever-increasing counter that will be used to have a "stable" microthread scheduling (first added is first scheduled)
        internal long SchedulerCounter;

        internal PriorityNodeQueue<SchedulerEntry> scheduledEntries = new PriorityNodeQueue<SchedulerEntry>();
        internal LinkedList<MicroThread> allMicroThreads = new LinkedList<MicroThread>();
        internal List<MicroThreadCallbackNode> callbackNodePool = new List<MicroThreadCallbackNode>();

        private ThreadLocal<MicroThread> runningMicroThread = new ThreadLocal<MicroThread>();

        public event EventHandler<SchedulerThreadEventArgs> MicroThreadStarted;
        public event EventHandler<SchedulerThreadEventArgs> MicroThreadEnded;

        public event EventHandler<SchedulerThreadEventArgs> MicroThreadCallbackStart;
        public event EventHandler<SchedulerThreadEventArgs> MicroThreadCallbackEnd;

        // This is part of temporary internal API, this should be improved before exposed
        internal event Action<Scheduler, SchedulerEntry, Exception> ActionException;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scheduler" /> class.
        /// </summary>
        public Scheduler()
        {
            PropagateExceptions = true;
            FrameChannel = new Channel<int> { Preference = ChannelPreference.PreferSender };
        }

        /// <summary>
        /// Gets or sets a value indicating whether microthread exceptions are propagated (and crashes the process). Default to true.
        /// This can be overridden to false per <see cref="MicroThread"/> by using <see cref="MicroThreadFlags.IgnoreExceptions"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [propagate exceptions]; otherwise, <c>false</c>.
        /// </value>
        internal bool PropagateExceptions { get; set; }

        /// <summary>
        /// Gets the current running micro thread in this scheduler through <see cref="Run"/>.
        /// </summary>
        /// <value>The current running micro thread in this scheduler.</value>
        public MicroThread RunningMicroThread
        {
            get { return runningMicroThread.Value; }
        }

        /// <summary>
        /// Gets the scheduler associated with current micro thread.
        /// </summary>
        /// <value>The scheduler associated with current micro thread.</value>
        public static Scheduler Current
        {
            get
            {
                var currentThread = CurrentMicroThread;
                return (currentThread != null) ? currentThread.Scheduler : null;
            }
        }

        /// <summary>
        /// Gets the list of every non-stopped micro threads.
        /// </summary>
        /// <value>The list of every non-stopped micro threads.</value>
        public ICollection<MicroThread> MicroThreads
        {
            get { return allMicroThreads; }
        }

        protected Channel<int> FrameChannel { get; private set; }

        /// <summary>
        /// Gets the current micro thread (self).
        /// </summary>
        /// <value>The current micro thread (self).</value>
        public static MicroThread CurrentMicroThread
        {
            get
            {
                var microThreadSyncContext = SynchronizationContext.Current as MicroThreadSynchronizationContext;
                return (microThreadSyncContext != null) ? microThreadSyncContext.MicroThread : null;
            }
        }


        /// <summary>
        /// Yields execution.
        /// If any other micro thread is pending, it will be run now and current micro thread will be scheduled as last.
        /// </summary>
        /// <returns></returns>
        public static MicroThreadYieldAwaiter Yield()
        {
            return new MicroThreadYieldAwaiter(CurrentMicroThread);
        }

        /// <summary>
        /// Yields execution until next frame.
        /// </summary>
        /// <returns>Task.</returns>
        public ChannelMicroThreadAwaiter<int> NextFrame()
        {
            if(MicroThread.Current == null)
                throw new Exception("NextFrame cannot be called out of the micro-thread context.");

            return FrameChannel.Receive();
        }

        /// <summary>
        /// Runs until no runnable tasklets left.
        /// This function is reentrant.
        /// </summary>
        public void Run()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
            int managedThreadId = 0;
#else
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
#endif

            MicroThreadCallbackList callbacks = default(MicroThreadCallbackList);

            while (true)
            {
                SchedulerEntry schedulerEntry;
                MicroThread microThread;
                lock (scheduledEntries)
                {
                    // Reclaim callbacks of previous microthread
                    MicroThreadCallbackNode callback;
                    while (callbacks.TakeFirst(out callback))
                    {
                        callback.Clear();
                        callbackNodePool.Add(callback);
                    }

                    if (scheduledEntries.Count == 0)
                        break;
                    schedulerEntry = scheduledEntries.Dequeue();
                    microThread = schedulerEntry.MicroThread;
                    if (microThread != null)
                    {
                        callbacks = microThread.Callbacks;
                        microThread.Callbacks = default(MicroThreadCallbackList);
                    }
                }

                // Since it can be reentrant, it should be restored after running the callback.
                var previousRunningMicrothread = runningMicroThread.Value;
                if (previousRunningMicrothread != null)
                {
                    if (MicroThreadCallbackEnd != null)
                        MicroThreadCallbackEnd(this, new SchedulerThreadEventArgs(previousRunningMicrothread, managedThreadId));
                }

                runningMicroThread.Value = microThread;

                if (microThread != null)
                {
                    var previousSyncContext = SynchronizationContext.Current;
                    SynchronizationContext.SetSynchronizationContext(microThread.SynchronizationContext);

                    // TODO: Do we still need to try/catch here? Everything should be caught in the continuation wrapper and put into MicroThread.Exception
                    try
                    {
                        if (microThread.State == MicroThreadState.Starting && MicroThreadStarted != null)
                            MicroThreadStarted(this, new SchedulerThreadEventArgs(microThread, managedThreadId));

                        if (MicroThreadCallbackStart != null)
                            MicroThreadCallbackStart(this, new SchedulerThreadEventArgs(microThread, managedThreadId));

                        using (Profiler.Begin(MicroThread.ProfilingKey, microThread.ScriptId))
                        {
                            var callback = callbacks.First;
                            while (callback != null)
                            {
                                callback.Invoke();
                                callback = callback.Next;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("Unexpected exception while executing a micro-thread", e);
                        microThread.SetException(e);
                    }
                    finally
                    {
                        if (MicroThreadCallbackEnd != null)
                            MicroThreadCallbackEnd(this, new SchedulerThreadEventArgs(microThread, managedThreadId));

                        SynchronizationContext.SetSynchronizationContext(previousSyncContext);
                        if (microThread.IsOver)
                        {
                            lock (microThread.AllLinkedListNode)
                            {
                                if (microThread.CompletionTask != null)
                                {
                                    if (microThread.State == MicroThreadState.Failed || microThread.State == MicroThreadState.Canceled)
                                        microThread.CompletionTask.TrySetException(microThread.Exception);
                                    else
                                        microThread.CompletionTask.TrySetResult(1);
                                }
                                else if (microThread.State == MicroThreadState.Failed && microThread.Exception != null)
                                {
                                    // Nothing was listening on the micro thread and it crashed
                                    // Let's treat it as unhandled exception and propagate it
                                    // Use ExceptionDispatchInfo.Capture to not overwrite callstack
                                    if (PropagateExceptions && (microThread.Flags & MicroThreadFlags.IgnoreExceptions) != MicroThreadFlags.IgnoreExceptions)
                                        ExceptionDispatchInfo.Capture(microThread.Exception).Throw();
                                }

                                if (MicroThreadEnded != null)
                                    MicroThreadEnded(this, new SchedulerThreadEventArgs(microThread, managedThreadId));
                            }
                        }

                        runningMicroThread.Value = previousRunningMicrothread;
                        if (previousRunningMicrothread != null)
                        {
                            if (MicroThreadCallbackStart != null)
                                MicroThreadCallbackStart(this, new SchedulerThreadEventArgs(previousRunningMicrothread, managedThreadId));
                        }
                    }
                }
                else
                {
                    try
                    {
                        schedulerEntry.Action();
                    }
                    catch (Exception e)
                    {
                        ActionException?.Invoke(this, schedulerEntry, e);
                    }
                }
            }

            while (FrameChannel.Balance < 0)
                FrameChannel.Send(0);
        }

        /// <summary>
        /// Creates a micro thread out of the specified function and schedules it as last micro thread to run in this scheduler.
        /// Note that in case of multithreaded scheduling, it might start before this function returns.
        /// </summary>
        /// <param name="microThreadFunction">The function to create a micro thread from.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>A micro thread.</returns>
        public MicroThread Add(Func<Task> microThreadFunction, MicroThreadFlags flags = MicroThreadFlags.None)
        {
            var microThread = new MicroThread(this, flags);
            microThread.Start(microThreadFunction);
            return microThread;
        }

        /// <summary>
        /// Creates a new empty micro thread, that could later be started with <see cref="MicroThread.Start"/>.
        /// </summary>
        /// <returns></returns>
        public MicroThread Create()
        {
            return new MicroThread(this);
        }

        /// <summary>
        /// Task that will completes when all MicroThread executions are completed.
        /// </summary>
        /// <param name="microThreads">The micro threads.</param>
        /// <returns></returns>
        public async Task WhenAll(params MicroThread[] microThreads)
        {
            var currentMicroThread = CurrentMicroThread;
            Task<int>[] continuationTasks;
            var tcs = new TaskCompletionSource<int>();

            // Need additional checks: Not sure if we should switch to return a Task and set it before returning it.
            // It should continue execution right away (no execution flow yielding).
            lock (microThreads)
            {
                if (microThreads.All(x => x.State == MicroThreadState.Completed))
                    return;

                if (microThreads.Any(x => x.State == MicroThreadState.Failed || x.State == MicroThreadState.Canceled))
                    throw new AggregateException(microThreads.Select(x => x.Exception).Where(x => x != null));

                var completionTasks = new List<Task<int>>();
                foreach (var thread in microThreads)
                {
                    if (!thread.IsOver)
                    {
                        lock (thread.AllLinkedListNode)
                        {
                            if (thread.CompletionTask == null)
                                thread.CompletionTask = new TaskCompletionSource<int>();
                        }
                        completionTasks.Add(thread.CompletionTask.Task);
                    }
                }

                continuationTasks = completionTasks.ToArray();
            }
            // Force tasks exception to be checked and propagated
            await Task.Factory.ContinueWhenAll(continuationTasks, tasks => Task.WaitAll(tasks));
        }

        // TODO: We will need a better API than exposing PriorityQueueNode<SchedulerEntry> before we can make this public.
        internal void Add(Action simpleAction, int priority = 0, object token = null)
        {
            var schedulerEntryNode = new PriorityQueueNode<SchedulerEntry>(new SchedulerEntry(simpleAction, priority) { Token = token });
            Schedule(schedulerEntryNode, ScheduleMode.Last);
        }

        internal PriorityQueueNode<SchedulerEntry> Create(Action simpleAction, int priority)
        {
            return new PriorityQueueNode<SchedulerEntry>(new SchedulerEntry(simpleAction, priority));
        }

        internal void Schedule(PriorityQueueNode<SchedulerEntry> schedulerEntry, ScheduleMode scheduleMode)
        {
            var nextCounter = SchedulerCounter++;
            if (scheduleMode == ScheduleMode.First)
                nextCounter = -nextCounter;

            schedulerEntry.Value.SchedulerCounter = nextCounter;

            scheduledEntries.Enqueue(schedulerEntry);
        }


        // TODO: Currently kept as a struct, but maybe a class would make more sense?
        // Ideally it should be merged with PriorityQueueNode so that we need to allocate only one object?
    }
}
