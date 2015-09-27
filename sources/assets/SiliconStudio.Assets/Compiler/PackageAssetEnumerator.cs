﻿using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Analysis;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Enumerate all assets of this package and its references.
    /// </summary>
    public class PackageAssetEnumerator : IPackageCompilerSource
    {
        private readonly Package package;

        public PackageAssetEnumerator(Package package)
        {
            this.package = package;
        }

        /// <inheritdoc/>
        public IEnumerable<AssetItem> GetAssets(AssetCompilerResult assetCompilerResult)
        {
            // Check integrity of the packages
            var packageAnalysis = new PackageSessionAnalysis(package.Session, new PackageAnalysisParameters());
            packageAnalysis.Run(assetCompilerResult);
            if (assetCompilerResult.HasErrors)
            {
                return Enumerable.Empty<AssetItem>();
            }

            // Get the dependencies (in reverse order so that we start depth first)
            // TODO: result.Error("Unable to find package [{0}]", packageDependency); < when resolving dependencies
            // TODO: result.Info("Compiling package [{0}]", package.FullPath); < probably doesn't make sense anymore
            var packages = package.GetPackagesWithRecursiveDependencies().Reverse();

            // For each package, list assets and sort by build order
            return packages.SelectMany(x =>
            {
                var packageAssets = x.Assets.ToList();
                // Sort the items to build by build order
                packageAssets.Sort((item1, item2) => item1.Asset != null && item2.Asset != null ? item1.Asset.InternalBuildOrder.CompareTo(item2.Asset.InternalBuildOrder) : 0);
                return packageAssets;
            }).ToList();
        }
    }
}