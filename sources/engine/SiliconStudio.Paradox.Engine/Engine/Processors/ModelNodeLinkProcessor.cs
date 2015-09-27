// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Engine.Processors
{
    public class ModelNodeLinkProcessor : EntityProcessor<ModelNodeLinkComponent>
    {
        internal ModelProcessor meshProcessor;

        public ModelNodeLinkProcessor()
            : base(new PropertyKey[] { TransformComponent.Key, ModelNodeLinkComponent.Key })
        {
        }

        protected override ModelNodeLinkComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get(ModelNodeLinkComponent.Key);
        }

        protected override void OnEntityRemoved(Entity entity, ModelNodeLinkComponent data)
        {
            // Reset TransformLink
            if (entity.Transform.TransformLink is ModelNodeTransformLink)
                entity.Transform.TransformLink = null;
        }

        public override void Draw(RenderContext context)
        {
            foreach (var item in enabledEntities)
            {
                var modelNodeLink = item.Value;
                var transformComponent = item.Key.Transform;
                var transformLink = transformComponent.TransformLink as ModelNodeTransformLink;

                // Try to use Target, otherwise Parent
                var modelComponent = modelNodeLink.Target;
                var modelEntity = modelComponent?.Entity ?? transformComponent.Parent?.Entity;

                // Check against Entity instead of ModelComponent to avoid having to get ModelComponent when nothing changed)
                if (transformLink == null || transformLink.NeedsRecreate(modelEntity, modelNodeLink.NodeName))
                {
                    // In case we use parent, modelComponent still needs to be resolved
                    if (modelComponent == null)
                        modelComponent = modelEntity?.Get(ModelComponent.Key);

                    // If model component is not parent, we want to use forceRecursive because we might want to update this link before the modelComponent.Entity is updated (depending on order of transformation update)
                    transformComponent.TransformLink = modelComponent != null ? new ModelNodeTransformLink(modelComponent, modelNodeLink.NodeName, modelEntity != transformComponent.Parent?.Entity) : null;
                }
            }
        }
    }
}