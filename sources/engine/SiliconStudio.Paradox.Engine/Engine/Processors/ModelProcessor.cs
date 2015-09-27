﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Engine.Processors
{
    /// <summary>
    /// The processor for <see cref="ModelComponent"/>.
    /// </summary>
    public class ModelProcessor : EntityProcessor<ModelProcessor.RenderModelItem>
    {
        // TODO: ModelProcessor should be decoupled from RenderModel
        private readonly RenderModelCollection[] allModelGroups;

        /// <summary>
        /// The link transformation to update.
        /// </summary>
        /// <remarks>The collection is declared globally only to avoid allocation at each frames</remarks>
        private readonly FastCollection<TransformComponent> linkTransformationToUpdate = new FastCollection<TransformComponent>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelProcessor"/> class.
        /// </summary>
        public ModelProcessor()
            : base(new PropertyKey[] { ModelComponent.Key, TransformComponent.Key })
        {
            ModelGroups = new List<RenderModelCollection>();
            allModelGroups = new RenderModelCollection[32];
            for (int i = 0; i < allModelGroups.Length; i++)
            {
                allModelGroups[i] = new RenderModelCollection((EntityGroup)i);
            }
        }

        protected override RenderModelItem GenerateAssociatedData(Entity entity)
        {
            return new RenderModelItem(new RenderModel(entity.Get<ModelComponent>()), entity.Transform);
        }

        protected override void OnEntityAdding(Entity entity, RenderModelItem data)
        {
            // Register model view hierarchy update
            entity.Transform.PostOperations.Add(data.TransformOperation = new ModelViewHierarchyTransformOperation(data.ModelComponent));
        }

        protected override void OnEntityRemoved(Entity entity, RenderModelItem data)
        {
            // Unregister model view hierarchy update
            entity.Transform.PostOperations.Remove(data.TransformOperation);
        }

        public Dictionary<Entity, RenderModelItem> EntityToRenderModel
        {
            get
            {
                return enabledEntities;
            }
        }

        /// <summary>
        /// Gets the current models to render per group.
        /// </summary>
        /// <value>The current models to render.</value>
        public List<RenderModelCollection> ModelGroups { get; private set; }

        /// <summary>
        /// Queries the list of <see cref="RenderModelCollection"/> for the specified mask
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <param name="outputCollection">The output collection.</param>
        public void QueryModelGroupsByMask(EntityGroupMask mask, List<RenderModelCollection> outputCollection)
        {
            // Get all meshes from the render model processor
            foreach (var renderModelGroup in ModelGroups)
            {
                // Perform culling on group and accept
                if (!mask.Contains(renderModelGroup.Group))
                {
                    continue;
                }
                outputCollection.Add(renderModelGroup);
            }
        }

        public override void Draw(RenderContext context)
        {
            // Clear previous model groups
            foreach (var modelGroup in ModelGroups)
            {
                modelGroup.Clear();
            }
            ModelGroups.Clear();

            var groupMaskUsed = EntityGroupMask.None;

            // Collect models for this frame, and dispatch them to list of group
            foreach (var matchingEntity in enabledEntities)
            {
                var item = matchingEntity.Value;
                var renderModel = item.RenderModel;

                // Skip disabled model components, or model components without a proper model set
                if (!renderModel.Update())
                {
                    continue;
                }

                // Add the render model to the specified collection group
                var groupIndex = (int)matchingEntity.Key.Group;
                groupMaskUsed |= (EntityGroupMask)(1 << groupIndex);
                var modelCollection = allModelGroups[groupIndex];
                modelCollection.Add(renderModel);
            }

            // Collect model groups
            for (int groupIndex = 0, groupMask = (int)groupMaskUsed; groupMask != 0; groupMask = groupMask >> 1, groupIndex++)
            {
                if ((groupMask & 1) == 0)
                {
                    continue;
                }

                var modelGroup = allModelGroups[groupIndex];
                ModelGroups.Add(modelGroup);
            }
        }

        public class RenderModelItem
        {
            public RenderModelItem(RenderModel renderModel, TransformComponent transformComponent)
            {
                RenderModel = renderModel;
                ModelComponent = renderModel.ModelComponent;
                TransformComponent = transformComponent;
            }

            public readonly RenderModel RenderModel;

            public readonly ModelComponent ModelComponent;

            public readonly TransformComponent TransformComponent;

            public ModelViewHierarchyTransformOperation TransformOperation;
        }

        public struct EntityLink
        {
            public int NodeIndex;
            public Entity Entity;
            public ModelComponent ModelComponent;
        }
    }
}