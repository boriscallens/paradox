// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Engine.Design;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Script component.
    /// </summary>
    [DataContract("ScriptComponent")]
    [Display(10, "Scripts", Expand = ExpandRule.Once)]
    public sealed class ScriptComponent : EntityComponent
    {
        public static PropertyKey<ScriptComponent> Key = new PropertyKey<ScriptComponent>("Key", typeof(ScriptComponent));

        public ScriptComponent()
        {
            Scripts = new ScriptCollection();
            Scripts.CollectionChanged += (sender, args) =>
            {
                var script = (Script)args.Item;
                if (script == null)
                    return;

                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        script.ScriptComponent = this;
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        script.ScriptComponent = null;
                        break;
                }
            };
        }

        /// <summary>
        /// Gets the scripts.
        /// </summary>
        /// <value>
        /// The scripts.
        /// </value>
        /// <userdoc>The list of scripts attached to the entity</userdoc>
        [Display("Script", Expand = ExpandRule.Always)]
        [MemberCollection(CanReorderItems = true)]
        public TrackingCollection<Script> Scripts { get; private set; }

        /// <inheritdoc/>
        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}