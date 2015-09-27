// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Model
{
    [DataContract("NodeInformation")]
    [DataStyle(DataStyle.Compact)]
    public class NodeInformation : IDiffKey
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        /// <userdoc>The name of the node (as it is written in the imported file).</userdoc>
        [DataMember(10), DiffUseAsset2]
        public string Name { get; set; }

        /// <summary>
        ///  The index of the parent.
        /// </summary>
        [DataMember(20), DiffUseAsset2]
        public int Depth { get; set; }

        /// <summary>
        /// A flag stating if the node is collapsible.
        /// </summary>
        /// <userdoc>If checked, the node is kept in the runtime version of the model. 
        /// Otherwise, all the meshes of model are merged the node information is lost.
        /// Nodes should be preserved in order to be animated or linked to an entity.</userdoc>
        [DataMember(30)]
        [DefaultValue(true)]
        public bool Preserve { get; set; }

        public NodeInformation()
        {
            Preserve = true;
        }

        public NodeInformation(string name, int depth, bool preserve)
        {
            Name = name;
            Depth = depth;
            Preserve = preserve;
        }

        object IDiffKey.GetDiffKey()
        {
            return Name;
        }
    }
}