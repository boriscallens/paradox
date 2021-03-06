﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Convertor
{
    public static class HlslTypes
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A Typedeclaration and dimensions</returns>
        public static Tuple<TypeBase, int, int> GetType(string type)
        {
            string prefix = null;
            if (type.StartsWith("matrix"))
            {
                var dimStr = type.Substring("matrix".Length);
                if (dimStr.Length == 0)
                {
                    return new Tuple<TypeBase, int, int>(new MatrixType(), 4, 4);
                }

                return new Tuple<TypeBase, int, int>(new MatrixType(), int.Parse(dimStr[0].ToString()), int.Parse(dimStr[2].ToString()));
            }

            TypeBase declaration = null;

            if (type.StartsWith("float"))
            {
                prefix = "float";
                declaration = ScalarType.Float;
            }
            else if (type.StartsWith("int"))
            {
                prefix = "int";
                declaration = ScalarType.Int;
            }
            else if (type.StartsWith("half"))
            {
                prefix = "half";
                declaration = ScalarType.Half;
            }
            else if (type.StartsWith("uint"))
            {
                prefix = "uint";
                declaration = ScalarType.UInt;
            }
            else if (type.StartsWith("bool"))
            {
                prefix = "bool";
                declaration = ScalarType.Bool;
            }
            else if (type.StartsWith("double"))
            {
                prefix = "double";
                declaration = ScalarType.Double;
            }

            if (prefix == null)
            {
                return null;
            }

            return new Tuple<TypeBase, int, int>(declaration, int.Parse(type.Substring(prefix.Length, 1)), 0);
        }
   }
}