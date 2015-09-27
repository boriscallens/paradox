// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Input
{
    /// <summary>
    /// This class represents a sensor of type Accelerometer. It measures the acceleration forces (including gravity) applying on the device.
    /// </summary>
    public class AccelerometerSensor : SensorBase
    {
        /// <summary>
        /// Gets the current acceleration applied on the device (in meters/seconds^2).
        /// </summary>
        public Vector3 Acceleration { get; internal set; }

        internal override void ResetData()
        {
            Acceleration = Vector3.Zero;
        }
    }
}