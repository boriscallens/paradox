// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.GeometricPrimitives;

namespace SiliconStudio.Paradox.Rendering.ProceduralModels
{
    /// <summary>
    /// A Cone descriptor
    /// </summary>
    [DataContract("ConeProceduralModel")]
    [Display("Cone")]
    public class ConeProceduralModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Initializes a new instance of the Cone descriptor class.
        /// </summary>
        public ConeProceduralModel()
        {
            Height = 1.0f;
            Radius = 0.5f;
            Tessellation = 16;
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        /// <userdoc>The height of the cone.</userdoc>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float Height { get; set; }

        /// <summary>
        /// Gets or sets the radius of the base of the Cone.
        /// </summary>
        /// <value>The radius.</value>
        /// <userdoc>The radius of the cone.</userdoc>
        [DataMember(20)]
        [DefaultValue(0.5f)]
        public float Radius { get; set; }

        /// <summary>
        /// Gets or sets the tessellation factor.
        /// </summary>
        /// <value>The tessellation.</value>
        /// <userdoc>The tessellation of the cone. That is the number of polygons composing it.</userdoc>
        [DataMember(30)]
        [DefaultValue(16)]
        public int Tessellation { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.Cone.New(Radius, Height, Tessellation, UvScale.X, UvScale.Y);
        }
    }
}