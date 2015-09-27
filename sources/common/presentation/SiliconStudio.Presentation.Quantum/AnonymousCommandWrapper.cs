﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.ViewModel;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public class AnonymousCommandWrapper : NodeCommandWrapperBase
    {
        private readonly IViewModelServiceProvider serviceProvider;
        private readonly string name;
        private readonly CombineMode combineMode;
        private readonly Func<object, bool, UndoToken> redo;
        private readonly Action<object, UndoToken> undo;
        private bool isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousCommandWrapper"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="ITransactionalActionStack"/> to use for this view model.</param>
        /// <param name="name">The name of this command.</param>
        /// <param name="combineMode">The combine mode to apply to this command.</param>
        /// <param name="redo">The do/redo function.</param>
        /// <param name="undo">The undo action, if the command can be undone.</param>
        /// <param name="dirtiables">The <see cref="IDirtiableViewModel"/> instances associated to this command.</param>
        /// <param name="discardTransactions">The transaction will be discarded if true, otherwise it is ended.</param>
        public AnonymousCommandWrapper(IViewModelServiceProvider serviceProvider, string name, CombineMode combineMode, Func<object, UndoToken> redo, Action<object, UndoToken> undo, IEnumerable<IDirtiableViewModel> dirtiables, bool discardTransactions = true)
            : base(serviceProvider, dirtiables)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (redo == null) throw new ArgumentNullException("redo");
            this.name = name;
            this.combineMode = combineMode;
            this.redo = (parameter, creatingActionItem) => redo(parameter);
            this.undo = undo;
            this.serviceProvider = serviceProvider;
            this.DiscardTransactions = discardTransactions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousCommandWrapper"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="ITransactionalActionStack"/> to use for this view model.</param>
        /// <param name="name">The name of this command.</param>
        /// <param name="combineMode">The combine mode to apply to this command.</param>
        /// <param name="redo">The do/redo function.</param>
        /// <param name="undo">The undo action, if the command can be undone.</param>
        /// <param name="dirtiables">The <see cref="IDirtiableViewModel"/> instances associated to this command.</param>
        /// <param name="discardTransactions">The transaction will be discarded if true, otherwise it is ended.</param>
        public AnonymousCommandWrapper(IViewModelServiceProvider serviceProvider, string name, CombineMode combineMode, Func<object, bool, UndoToken> redo, Action<object, UndoToken> undo, IEnumerable<IDirtiableViewModel> dirtiables, bool discardTransactions = true)
            : base(serviceProvider, dirtiables)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (redo == null) throw new ArgumentNullException("redo");
            this.name = name;
            this.combineMode = combineMode;
            this.redo = redo;
            this.undo = undo;
            this.serviceProvider = serviceProvider;
            this.DiscardTransactions = discardTransactions;
            this.AllowReentrancy = false;
        }

        /// <inheritdoc/>
        public override string Name { get { return name; } }

        /// <inheritdoc/>
        public override CombineMode CombineMode { get { return combineMode; } }

        /// <inheritdoc/>
        protected override UndoToken Redo(object parameter, bool creatingActionItem)
        {
            return redo(parameter, creatingActionItem);
        }

        /// <inheritdoc/>
        protected override void Undo(object parameter, UndoToken token)
        {
            if (undo == null)
                throw new InvalidOperationException("This command cannot be cancelled.");

            undo(parameter, token);
        }
    }
}