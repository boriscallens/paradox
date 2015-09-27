﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;

using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using ITypeDescriptor = SharpYaml.Serialization.ITypeDescriptor;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// A Yaml Serializer for <see cref="AssetBase"/>. Because this type is immutable
    /// we need to implement a special serializer.
    /// </summary>
    [YamlSerializerFactory]
    internal class AssetItemSerializer : ObjectSerializer, IDataCustomVisitor
    {
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            return CanVisit(typeDescriptor.Type) ? this : null;
        }

        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            objectContext.Instance = objectContext.SerializerContext.IsSerializing ? new AssetItemMutable((AssetItem)objectContext.Instance) : new AssetItemMutable();
        }

        protected override void TransformObjectAfterRead(ref ObjectContext objectContext)
        {
            objectContext.Instance = ((AssetItemMutable)objectContext.Instance).ToAssetItem();
        }

        private class AssetItemMutable
        {
            public AssetItemMutable()
            {
            }

            public AssetItemMutable(AssetItem item)
            {
                Location = item.Location;
                SourceFolder = item.SourceFolder;
                Asset = item.Asset;
                ProjectFile = item.SourceProject;
            }

            [DataMember(0)]
            public UFile Location;

            [DataMember(1)]
            [DefaultValue(null)]
            public UDirectory SourceFolder;

            [DataMember(2)]
            public Asset Asset;

            [DataMember(3)]
            public UFile ProjectFile;

            public AssetItem ToAssetItem()
            {
                return new AssetItem(Location, Asset) { SourceFolder = SourceFolder, SourceProject = ProjectFile };
            }
        }

        public bool CanVisit(Type type)
        {
            return type == typeof(AssetItem);
        }

        public void Visit(ref VisitorContext context)
        {
            context.Visitor.VisitObject(context.Instance, context.Descriptor, true);
        }
    }
}