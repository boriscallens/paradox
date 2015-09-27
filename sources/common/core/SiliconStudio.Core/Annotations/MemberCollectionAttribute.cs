﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Core.Annotations
{
    /// <summary>
    /// This attributes provides additional information on a member collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class MemberCollectionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets whether this collection is read-only. If <c>true</c>, applications using this collection
        /// should not allow to add or remove items.
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets whether the items of this collection can be reordered. If <c>true</c>, applications using
        /// this collection should provide users a way to reorder items.
        /// </summary>
        public bool CanReorderItems { get; set; }
    }
}
