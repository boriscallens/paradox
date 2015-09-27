// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public static class ShaderGeneratorContextExtensions
    {
        public static void AddLoadingFromSession(this ShaderGeneratorContextBase context, Package package)
        {
            var previousGetAssetFriendlyName = context.GetAssetFriendlyName;
            var previousFindAsset = context.FindAsset;

            // Setup the GetAssetFriendlyName callback
            context.GetAssetFriendlyName = runtimeAsset =>
            {
                string assetFriendlyName = null;

                if (previousGetAssetFriendlyName != null)
                {
                    assetFriendlyName = previousGetAssetFriendlyName(runtimeAsset);
                }

                if (string.IsNullOrEmpty(assetFriendlyName))
                {
                    var referenceAsset = AttachedReferenceManager.GetAttachedReference(runtimeAsset);
                    assetFriendlyName = string.Format("{0}:{1}", referenceAsset.Id, referenceAsset.Url);
                }

                return assetFriendlyName;
            };

            // Setup the FindAsset callback
            context.FindAsset = runtimeAsset =>
            {
                object newAsset = null; 
                if (previousFindAsset != null)
                {
                    newAsset = previousFindAsset(runtimeAsset);
                }

                if (newAsset != null)
                {
                    return newAsset;
                }

                var reference = AttachedReferenceManager.GetAttachedReference(runtimeAsset);


                var assetItem = package.FindAsset(reference.Id) ?? package.FindAsset(reference.Url);

                return assetItem?.Asset;
            };            
        }
    }
}