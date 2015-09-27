// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Engine.Processors;

namespace SiliconStudio.Paradox.Engine
{
    [DataContract("ModelNodeLinkComponent")]
    [Display(15, "Model Node Link", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(ModelNodeLinkProcessor))]
    public sealed class ModelNodeLinkComponent : EntityComponent
    {
        public static PropertyKey<ModelNodeLinkComponent> Key = new PropertyKey<ModelNodeLinkComponent>("Key", typeof(ModelNodeLinkComponent));

        /// <summary>
        /// Gets or sets the model which contains the hierarchy to use.
        /// </summary>
        /// <value>
        /// The model which contains the hierarchy to use.
        /// </value>
        /// <userdoc>The reference to the target entity to which attach the current entity. If null, parent will be used.</userdoc>
        [Display("Target (Parent if not set)")]
        public ModelComponent Target { get; set; }

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        /// <userdoc>The name of node of the model of the target entity to which attach the current entity.</userdoc>
        public string NodeName { get; set; }

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}