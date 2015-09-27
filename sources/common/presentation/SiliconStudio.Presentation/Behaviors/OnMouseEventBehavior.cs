﻿using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Behaviors
{
    public enum MouseEventType
    {
        None,
        MouseDown,
        MouseUp,
        MouseMove,
        PreviewMouseDown,
        PreviewMouseUp,
        PreviewMouseMove
    }

    public class OnMouseEventBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty EventTypeProperty = DependencyProperty.Register("EventType", typeof(MouseEventType), typeof(OnMouseEventBehavior), new FrameworkPropertyMetadata(MouseEventType.None, EventTypeChanged));

        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(OnMouseEventBehavior));

        /// <summary>
        /// Identifies the <see cref="HandleEvent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HandleEventProperty = DependencyProperty.Register("HandleEvent", typeof(bool), typeof(OnMouseEventBehavior));

        public MouseEventType EventType { get { return (MouseEventType)GetValue(EventTypeProperty); } set { SetValue(EventTypeProperty, value); } }

        /// <summary>
        /// Gets or sets the command to invoke when the event is raised.
        /// </summary>
        public ICommand Command { get { return (ICommand)GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }

        /// <summary>
        /// Gets or sets whether to set the event as handled.
        /// </summary>
        public bool HandleEvent { get { return (bool)GetValue(HandleEventProperty); } set { SetValue(HandleEventProperty, value); } }

        protected override void OnAttached()
        {
            base.OnAttached();
            RegisterHandler(EventType);
        }

        protected override void OnDetaching()
        {
            UnregisterHandler(EventType);
            base.OnAttached();
        }

        private static void EventTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (OnMouseEventBehavior)d;
            if (behavior.AssociatedObject == null)
                return;

            var oldValue = (MouseEventType)e.OldValue;
            behavior.UnregisterHandler(oldValue);
            var newValue = (MouseEventType)e.NewValue;
            behavior.RegisterHandler(newValue);
        }

        private void RegisterHandler(MouseEventType type)
        {
            switch (type)
            {
                case MouseEventType.MouseDown:
                    AssociatedObject.MouseDown += MouseMoveHandler;
                    break;
                case MouseEventType.MouseUp:
                    AssociatedObject.MouseUp += MouseMoveHandler;
                    break;
                case MouseEventType.MouseMove:
                    AssociatedObject.MouseMove += MouseMoveHandler;
                    break;
                case MouseEventType.PreviewMouseDown:
                    AssociatedObject.PreviewMouseDown += MouseMoveHandler;
                    break;
                case MouseEventType.PreviewMouseUp:
                    AssociatedObject.PreviewMouseUp += MouseMoveHandler;
                    break;
                case MouseEventType.PreviewMouseMove:
                    AssociatedObject.PreviewMouseMove += MouseMoveHandler;
                    break;
            }
        }

        private void UnregisterHandler(MouseEventType type)
        {
            switch (type)
            {
                case MouseEventType.MouseDown:
                    AssociatedObject.MouseDown -= MouseButtonHandler;
                    break;
                case MouseEventType.MouseUp:
                    AssociatedObject.MouseUp -= MouseButtonHandler;
                    break;
                case MouseEventType.MouseMove:
                    AssociatedObject.MouseMove -= MouseMoveHandler;
                    break;
                case MouseEventType.PreviewMouseDown:
                    AssociatedObject.PreviewMouseDown -= MouseButtonHandler;
                    break;
                case MouseEventType.PreviewMouseUp:
                    AssociatedObject.PreviewMouseUp -= MouseButtonHandler;
                    break;
                case MouseEventType.PreviewMouseMove:
                    AssociatedObject.PreviewMouseMove -= MouseMoveHandler;
                    break;
            }
        }

        private void MouseButtonHandler(object sender, MouseButtonEventArgs e)
        {
            MouseMoveHandler(sender, e);
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (HandleEvent)
            {
                e.Handled = true;
            }
            var cmd = Command;
            var position = e.GetPosition(AssociatedObject);
            var vectorPosition = new Vector2((float)position.X, (float)position.Y);
            if (cmd != null && cmd.CanExecute(vectorPosition))
                cmd.Execute(vectorPosition);
        }
    }
}
