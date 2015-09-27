// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Assets
{
    [DataSerializer(typeof(KeyedSortedListSerializer<RootAssetCollection, Guid, AssetReference<Asset>>))]
    public class RootAssetCollection : KeyedSortedList<Guid, AssetReference<Asset>>
    {
        /// <inheritdoc/>
        protected override Guid GetKeyForItem(AssetReference<Asset> item)
        {
            return item.Id;
        }
    }
}