﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Core.Serialization.Contents
{
    /// <summary>
    /// Describe a reference between an object and another.
    /// </summary>
    /// <remarks>This class is IEquatable, and equality is true if and only if Location and ObjType properties match</remarks>
    [DataSerializer(typeof(ChunkReference.Serializer))]
    public struct ChunkReference : IEquatable<ChunkReference>
    {
        public readonly string Location;

        public readonly Type ObjectType;

        public const int NullIdentifier = -1;

        public ChunkReference(Type objectType, string location)
        {
            ObjectType = objectType;
            Location = location;
        }

        public bool Equals(ChunkReference other)
        {
            return string.Equals(Location, other.Location) && ObjectType == other.ObjectType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ChunkReference && Equals((ChunkReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Location.GetHashCode()*397) ^ ObjectType.GetHashCode();
            }
        }

        public static bool operator ==(ChunkReference left, ChunkReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ChunkReference left, ChunkReference right)
        {
            return !left.Equals(right);
        }

        internal class Serializer : DataSerializer<ChunkReference>
        {
            public override Type SerializationType
            {
                get { return typeof(ChunkReference); }
            }

            public override void Serialize(ref ChunkReference chunkReference, ArchiveMode mode, SerializationStream stream)
            {
                if (mode == ArchiveMode.Serialize)
                {
                    stream.Write(chunkReference.ObjectType.AssemblyQualifiedName);
                    stream.Write(chunkReference.Location);
                }
                else if (mode == ArchiveMode.Deserialize)
                {
                    string typeName = stream.ReadString();
                    chunkReference = new ChunkReference(AssemblyRegistry.GetType(typeName), stream.ReadString());
                }
            }
        }
    }
}