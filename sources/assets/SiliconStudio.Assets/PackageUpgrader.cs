﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Offers a way for package to upgrade dependent packages.
    /// For example, if you write package A and Game1 depends on it, you might want to offer a new version of package A that would automatically perform some upgrades on Game1.
    /// </summary>
    public abstract class PackageUpgrader
    {
        public PackageUpgraderAttribute Attribute { get; internal set; }

        /// <summary>
        /// Performs the package migration, before assets are loaded
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="log">The log.</param>
        /// <param name="dependentPackage">The source package.</param>
        /// <param name="dependency">The dependency.</param>
        /// <param name="dependencyPackage">The dependency package.</param>
        /// <param name="assetFiles">The asset files.</param>
        /// <returns></returns>
        public abstract bool Upgrade(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage, IList<PackageLoadingAssetFile> assetFiles);

        /// <summary>
        /// Performs the second step of package migration, after assets have been loaded.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="log">The log.</param>
        /// <param name="dependentPackage">The source package.</param>
        /// <param name="dependency">The dependency.</param>
        /// <param name="dependencyPackage">The dependency package.</param>
        /// <param name="dependencyVersionBeforeUpdate">The version before the update.</param>
        /// <returns></returns>
        public virtual bool UpgradeAfterAssetsLoaded(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage, PackageVersionRange dependencyVersionBeforeUpdate)
        {
            return true;
        }
    }
}