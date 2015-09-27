// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Rendering.Colors
{
    /// <summary>
    /// A light color described by a rgb color
    /// </summary>
    [DataContract("ColorRgbProvider")]
    [DataAlias("LightColorRgb")]
    [Display("RGB")]
    public class ColorRgbProvider : IColorProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorRgbProvider"/> class.
        /// </summary>
        public ColorRgbProvider()
        {
            Value = new Color3(1.0f);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorRgbProvider"/> class.
        /// </summary>
        /// <param name="color">The color.</param>
        public ColorRgbProvider(Color3 color)
        {
            Value = color;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorRgbProvider"/> class.
        /// </summary>
        /// <param name="color">The color.</param>
        public ColorRgbProvider(Color color)
        {
            Value = (Color3)color;
        }

        /// <summary>
        /// Gets or sets the light color in rgb.
        /// </summary>
        /// <value>The color.</value>
        [DataMember(10)]
        [DataAlias("Color")]
        [InlineProperty]
        public Color3 Value { get; set; }

        public Color3 ComputeColor()
        {
            return Value;
        }
    }
}