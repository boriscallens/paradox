﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interactivity;

using SiliconStudio.Presentation.Core;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// An abstract behavior that allows to perform actions when an event is raised. It supports both <see cref="RoutedEvent"/> and standard <c>event</c>,
    /// and allow to catch routed event triggered by any control.
    /// </summary>
    public abstract class OnEventBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// Identifies the <see cref="EventName"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EventNameProperty = DependencyProperty.Register("EventName", typeof(string), typeof(OnEventBehavior));

        /// <summary>
        /// Identifies the <see cref="EventOwnerType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EventOwnerTypeProperty = DependencyProperty.Register("EventOwnerType", typeof(Type), typeof(OnEventBehavior));

        /// <summary>
        /// Identifies the <see cref="HandleEvent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HandleEventProperty = DependencyProperty.Register("HandleEvent", typeof(bool), typeof(OnEventBehavior));

        private readonly RoutedEventHandler routedEventHandler;
        private AnonymousEventHandler eventHandler;
        private RoutedEvent routedEvent;

        protected OnEventBehavior()
        {
            routedEventHandler = RoutedEventHandler;
        }

        /// <summary>
        /// Gets or sets the name of the event to handle.
        /// </summary>
        public string EventName { get { return (string)GetValue(EventNameProperty); } set { SetValue(EventNameProperty, value); } }

        /// <summary>
        /// Gets or sets the type that owns the event when <see cref="EventName"/> describes a <see cref="RoutedEvent"/>.
        /// </summary>
        public Type EventOwnerType { get { return (Type)GetValue(EventOwnerTypeProperty); } set { SetValue(EventOwnerTypeProperty, value); } }

        /// <summary>
        /// Gets or sets whether to set the event as handled.
        /// </summary>
        public bool HandleEvent { get { return (bool)GetValue(HandleEventProperty); } set { SetValue(HandleEventProperty, value); } }

        /// <summary>
        /// Invoked when the monitored event is raised.
        /// </summary>
        protected abstract void OnEvent();

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            if (EventName == null)
                throw new ArgumentException(string.Format("The EventName property must be set on behavior '{0}'.", GetType().FullName));

            var eventOwnerType = EventOwnerType ?? AssociatedObject.GetType();

            RoutedEvent[] routedEvents = EventManager.GetRoutedEvents().Where(evt => evt.Name == EventName && evt.OwnerType.IsAssignableFrom(eventOwnerType)).ToArray();

            if (routedEvents.Length > 0)
            {
                if (routedEvents.Length > 1)
                    throw new NotImplementedException("TODO: several events found, find a way to decide the most relevant one.");

                routedEvent = routedEvents.First();
                AssociatedObject.AddHandler(routedEvent, routedEventHandler);
            }
            else
            {
                var eventInfo = AssociatedObject.GetType().GetEvent(EventName);

                if (eventInfo == null)
                    throw new InvalidOperationException(string.Format("Impossible to find a valid event named '{0}'.", EventName));

                eventHandler = AnonymousEventHandler.RegisterEventHandler(eventInfo, AssociatedObject, OnEvent);
            }
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            if (routedEvent != null)
            {
                AssociatedObject.RemoveHandler(routedEvent, routedEventHandler);
                routedEvent = null;
            }
            else if (eventHandler != null)
            {
                AnonymousEventHandler.UnregisterEventHandler(eventHandler);
                eventHandler = null;
            }
        }

        private void RoutedEventHandler(object sender, RoutedEventArgs e)
        {
            if (HandleEvent)
            {
                e.Handled = true;
            }
            OnEvent();
        }
    }
}