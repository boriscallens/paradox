﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using System.ComponentModel;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<SphereColliderShapeDesc>))]
    [DataContract("SphereColliderShapeDesc")]
    [Display(50, "Sphere")]
    public class SphereColliderShapeDesc : IInlineColliderShapeDesc
    {
        /// <userdoc>
        /// Select this if this shape will represent a Circle 2D shape
        /// </userdoc>
        [DataMember(10)]
        public bool Is2D;

        /// <userdoc>
        /// The radius of the sphere/circle.
        /// </userdoc>
        [DataMember(20)]
        [DefaultValue(0.5f)]
        public float Radius = 0.5f;

        /// <userdoc>
        /// The offset with the real graphic mesh.
        /// </userdoc>
        [DataMember(30)]
        public Vector3 LocalOffset;

        public int CompareTo(object obj)
        {
            var other = obj as SphereColliderShapeDesc;
            if (other == null) return -1;
            if (other.Is2D == Is2D && Math.Abs(other.Radius - Radius) < float.Epsilon && other.LocalOffset == LocalOffset) return 0;
            return 1;
        }
    }
}