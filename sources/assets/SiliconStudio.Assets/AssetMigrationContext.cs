﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Context used by <see cref="IAssetUpgrader"/>.
    /// </summary>
    public class AssetMigrationContext
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AssetMigrationContext"/>.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="log"></param>
        public AssetMigrationContext(Package package, ILogger log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            Package = package;
            Log = log;
        }

        /// <summary>
        /// The current package where the current asset is being migrated. This is null when the asset being migrated is a package.
        /// </summary>
        public Package Package { get; internal set; }

        /// <summary>
        /// The logger for this context.
        /// </summary>
        public ILogger Log { get; internal set; }
    }
}