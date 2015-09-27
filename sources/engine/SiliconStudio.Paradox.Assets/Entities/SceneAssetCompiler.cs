// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Assets.Serializers;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Assets.Entities
{
    public class SceneAssetCompiler : AssetCompilerBase<SceneAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SceneAsset asset, AssetCompilerResult result)
        {
            foreach (var entityData in asset.Hierarchy.Entities)
            {
                // TODO: How to make this code pluggable?
                var modelComponent = entityData.Entity.Components.Get(ModelComponent.Key);
                var spriteComponent = entityData.Entity.Components.Get(SpriteComponent.Key);
                var scriptComponent = entityData.Entity.Components.Get(ScriptComponent.Key);

                // determine the underlying source asset exists
                if (modelComponent != null)
                {
                    if (modelComponent.Model == null)
                    {
                        result.Warning(string.Format("The entity [{0}:{1}] has a model component that does not reference any model.", urlInStorage, entityData.Entity.Name));
                        continue;
                    }

                    var modelAttachedReference = AttachedReferenceManager.GetAttachedReference(modelComponent.Model);
                    var modelId = modelAttachedReference.Id;

                    // compute the full path to the source asset.
                    var assetItem = AssetItem.Package.Session.FindAsset(modelId);
                    if (assetItem == null)
                    {
                        result.Error(string.Format("The entity [{0}:{1}] is referencing an unreachable model.", urlInStorage, entityData.Entity.Name));
                        continue;
                    }
                }
                if (spriteComponent != null && spriteComponent.SpriteProvider == null)
                {
                    result.Warning(string.Format("The entity [{0}:{1}] has a sprite component that does not reference any sprite group.", urlInStorage, entityData.Entity.Name));
                }
                if (scriptComponent != null)
                {
                    foreach (var script in scriptComponent.Scripts)
                    {
                        if (script is UnloadableScript)
                        {
                            result.Error(string.Format("The entity [{0}:{1}] reference an invalid script '{2}'.", urlInStorage, entityData.Entity.Name, script.GetType().Name));
                        }
                    }
                }
            }

            result.BuildSteps = new AssetBuildStep(AssetItem) { new EntityCombineCommand(urlInStorage, AssetItem.Package, context, asset) };
        }

        private class EntityCombineCommand : AssetCommand<SceneAsset>
        {
            private readonly Package package;
            private readonly AssetCompilerContext context;


            public EntityCombineCommand(string url, Package package, AssetCompilerContext context, SceneAsset assetParameters) : base(url, assetParameters)
            {
                this.package = package;
                this.context = context;
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new AssetManager();

                var scene = new Scene(AssetParameters.Hierarchy.SceneSettings);
                foreach (var rootEntity in AssetParameters.Hierarchy.RootEntities)
                {
                    scene.Entities.Add(AssetParameters.Hierarchy.Entities[rootEntity].Entity);
                }
                assetManager.Save(Url, scene);

                return Task.FromResult(ResultStatus.Successful);
            }

            public override string ToString()
            {
                return "Entity combine command for entity asset '{0}'.".ToFormat(Url);
            }
        }
    }
}