﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Performs hierarchical updates for a given <see cref="Model"/>.
    /// </summary>
    public class ModelViewHierarchyUpdater
    {
        private ModelNodeDefinition[] nodes;
        private ModelNodeTransformation[] nodeTransformations;

        private int matrixCounter;

        public ModelNodeDefinition[] Nodes
        {
            get { return nodes; }
        }

        public ModelNodeTransformation[] NodeTransformations
        {
            get { return nodeTransformations; }
        }

        private static ModelNodeDefinition[] GetDefaultNodeDefinitions()
        {
            return new[] { new ModelNodeDefinition { Name = "Root", ParentIndex = -1, Transform = { Scaling = Vector3.One }, Flags = ModelNodeFlags.Default } };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelViewHierarchyUpdater"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public ModelViewHierarchyUpdater(Model model)
        {
            if (model == null) throw new ArgumentNullException("model");
            Initialize(model);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelViewHierarchyUpdater" /> class.
        /// </summary>
        /// <param name="newNodes">The new nodes.</param>
        public ModelViewHierarchyUpdater(ModelNodeDefinition[] newNodes)
        {
            Initialize(newNodes);
        }

        public void Initialize(Model model)
        {
            Initialize(model.Hierarchy?.Nodes);
            nodeTransformations[0].Flags &= ~ModelNodeFlags.EnableTransform;
        }

        public void Initialize(ModelNodeDefinition[] newNodes)
        {
            if (this.nodes == newNodes && this.nodes != null)
            {
                return;
            }

            this.nodes = newNodes ?? GetDefaultNodeDefinitions();

            if (nodeTransformations == null || nodeTransformations.Length < this.nodes.Length)
                nodeTransformations = new ModelNodeTransformation[this.nodes.Length];

            for (int index = 0; index < nodes.Length; index++)
            {
                nodeTransformations[index].ParentIndex = nodes[index].ParentIndex;
                nodeTransformations[index].Transform = nodes[index].Transform;
                nodeTransformations[index].Flags = nodes[index].Flags;
                nodeTransformations[index].RenderingEnabledRecursive = true;
            }
        }

        /// <summary>
        /// Resets initial values.
        /// </summary>
        public void ResetInitialValues()
        {
            var nodesLocal = nodes;
            for (int index = 0; index < nodesLocal.Length; index++)
            {
                nodeTransformations[index].Transform = nodesLocal[index].Transform;
            }
        }

        /// <summary>
        /// For each node, updates the world matrices from local matrices.
        /// </summary>
        public void UpdateMatrices()
        {
            // Compute transformations
            var nodesLength = nodes.Length;
            for (int index = 0; index < nodesLength; index++)
            {
                UpdateNode(ref nodeTransformations[index]);
            }
            matrixCounter++;
        }

        /// <summary>
        /// Updates previously computed world matrices to TransformationKeys.World for each <see cref="RenderMesh" />.
        /// </summary>
        /// <param name="renderMesh">The render mesh.</param>
        internal void UpdateRenderMesh(RenderMesh renderMesh)
        {
            var nodeIndex = renderMesh.Mesh.NodeIndex;
            var enabled = nodeTransformations[nodeIndex].RenderingEnabledRecursive;
            renderMesh.Enabled = enabled;
            if (enabled && renderMesh.MatrixCounter != matrixCounter)
            {
                renderMesh.WorldMatrix = nodeTransformations[nodeIndex].WorldMatrix;
                renderMesh.IsGeometryInverted = nodeTransformations[nodeIndex].IsScalingNegative;
                renderMesh.MatrixCounter = matrixCounter;
            }
        }

        public void GetWorldMatrix(int index, out Matrix matrix)
        {
            matrix = nodeTransformations[index].WorldMatrix;
        }

        public void GetLocalMatrix(int index, out Matrix matrix)
        {
            matrix = nodeTransformations[index].LocalMatrix;
        }

        private void UpdateNode(ref ModelNodeTransformation node)
        {
            // Compute LocalMatrix
            if ((node.Flags & ModelNodeFlags.EnableTransform) == ModelNodeFlags.EnableTransform)
            {
                var scaling = node.Transform.Scaling;
                TransformComponent.CreateMatrixTRS(ref node.Transform.Translation, ref node.Transform.Rotation, ref scaling, out node.LocalMatrix);
                node.IsScalingNegative = scaling.X * scaling.Y * scaling.Z < 0.0f;
            }

            var nodeTransformationsLocal = this.nodeTransformations;

            var parentIndex = node.ParentIndex;

            // Update Enabled
            bool renderingEnabledRecursive = (node.Flags & ModelNodeFlags.EnableRender) == ModelNodeFlags.EnableRender;
            if (parentIndex != -1)
                renderingEnabledRecursive &= nodeTransformationsLocal[parentIndex].RenderingEnabledRecursive;

            node.RenderingEnabledRecursive = renderingEnabledRecursive;

            if (renderingEnabledRecursive)
            {
                // Compute WorldMatrix
                if (parentIndex != -1)
                {
                    Matrix.Multiply(ref node.LocalMatrix, ref nodeTransformationsLocal[parentIndex].WorldMatrix, out node.WorldMatrix);
                    if (nodeTransformationsLocal[parentIndex].IsScalingNegative)
                        node.IsScalingNegative = !node.IsScalingNegative;
                }
                else
                    node.WorldMatrix = node.LocalMatrix;
            }
        }
    }
}