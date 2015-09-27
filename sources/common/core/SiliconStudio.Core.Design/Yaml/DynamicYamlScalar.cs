// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Dynamic;
using System.Linq.Expressions;
using SharpYaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Dynamic version of <see cref="YamlScalarNode"/>.
    /// </summary>
    public class DynamicYamlScalar : DynamicYamlObject
    {
        internal YamlScalarNode node;

        public YamlScalarNode Node
        {
            get
            {
                return node;
            }
        }

        public DynamicYamlScalar(YamlScalarNode node)
        {
            this.node = node;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = binder.Type.IsEnum
                ? Enum.Parse(binder.Type, node.Value)
                : Convert.ChangeType(node.Value, binder.Type);

            return true;
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            var str = arg as string;
            if (str != null)
            {
                if (binder.Operation == ExpressionType.Equal)
                {
                    result = node.Value == str;
                    return true;
                }
                if (binder.Operation == ExpressionType.NotEqual)
                {
                    result = node.Value != str;
                    return true;
                }
            }
            return base.TryBinaryOperation(binder, arg, out result);
        }
    }
}