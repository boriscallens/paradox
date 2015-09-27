﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;
using System.Text;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;


namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// Describes an effect asset. 
    /// </summary>
    [DataContract("EffectLibrary")]
    [AssetDescription(FileExtension, false, AlwaysMarkAsRoot = true)]
    [AssetCompiler(typeof(EffectLogAssetCompiler))]
    [Display(98, "Effect Library", "An effect library")]
    public sealed class EffectLogAsset : SourceCodeAsset
    {
        /// <summary>
        /// The default file name used to store effect compile logs.
        /// </summary>
        public const string DefaultFile = "EffectCompileLog";

        /// <summary>
        /// The default file extension used by the <see cref="EffectLogAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxeffectlog";

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectLogAsset"/> class.
        /// </summary>
        public EffectLogAsset()
        {
        }

        protected override int InternalBuildOrder
        {
            get { return 100; }
        }
    }
}