﻿// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Extensions;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.GeometricPrimitives;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Physics
{
    public class SphereColliderShape : ColliderShape
    {
        private static MeshDraw cachedDebugPrimitive;

        /// <summary>
        /// Initializes a new instance of the <see cref="SphereColliderShape"/> class.
        /// </summary>
        /// <param name="is2D">if set to <c>true</c> [is2 d].</param>
        /// <param name="radius">The radius.</param>
        public SphereColliderShape(bool is2D, float radius)
        {
            Type = ColliderShapeTypes.Sphere;
            Is2D = is2D;

            var shape = new BulletSharp.SphereShape(radius)
            {
                LocalScaling = Vector3.One
            };

            if (Is2D)
            {
                InternalShape = new BulletSharp.Convex2DShape(shape) { LocalScaling = new Vector3(1, 1, 0) };
            }
            else
            {
                InternalShape = shape;
            }

            DebugPrimitiveMatrix = Matrix.Scaling(2 * radius * 1.01f);
            if (Is2D)
            {
                DebugPrimitiveMatrix.M33 = 0f;
            }
        }

        public override MeshDraw CreateDebugPrimitive(GraphicsDevice device)
        {
            return cachedDebugPrimitive ?? (cachedDebugPrimitive = GeometricPrimitive.Sphere.New(device).ToMeshDraw());
        }
    }
}