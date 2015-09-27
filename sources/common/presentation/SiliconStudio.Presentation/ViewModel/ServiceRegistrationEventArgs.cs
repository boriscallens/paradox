using System;

namespace SiliconStudio.Presentation.ViewModel
{
    /// <summary>
    /// Arguments of the events raised by <see cref="IViewModelServiceProvider"/> implementations.
    /// </summary>
    public class ServiceRegistrationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationEventArgs"/> class.
        /// </summary>
        /// <param name="service">The service related to this event.</param>
        internal ServiceRegistrationEventArgs(object service)
        {
            Service = service;
        }

        /// <summary>
        /// Gets the service related to this event.
        /// </summary>
        public object Service { get; }
    }
}