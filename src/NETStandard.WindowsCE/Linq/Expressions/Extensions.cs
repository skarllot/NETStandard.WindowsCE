﻿//
// Extensions.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions
{
    internal static class Extensions
    {
        public static bool IsGenericInstanceOf(this Type self, Type type)
        {
            return self.IsGenericType && self.GetGenericTypeDefinition() == type;
        }

        public static bool IsNullable(this Type self)
        {
            return self.IsGenericInstanceOf(typeof(Nullable<>));
        }

        public static bool IsExpression(this Type self)
        {
            return self == typeof(Expression) || self.IsSubclassOf(typeof(Expression));
        }

        public static bool IsGenericImplementationOf(this Type self, Type type)
        {
            return self.GetInterfaces()
                .Any(iface => iface.IsGenericInstanceOf(type));
        }

        public static bool IsAssignableTo(this Type self, Type type)
        {
            return type.IsAssignableFrom(self) ||
                ArrayTypeIsAssignableTo(self, type);
        }

        public static Type GetFirstGenericArgument(this Type self)
        {
            return self.GetGenericArguments()[0];
        }

        public static Type MakeGenericTypeFrom(this Type self, Type type)
        {
            return self.MakeGenericType(type.GetGenericArguments());
        }

        public static Type MakeNullableType(this Type self)
        {
            return typeof(Nullable<>).MakeGenericType(self);
        }

        public static Type GetNotNullableType(this Type self)
        {
            return self.IsNullable() ? self.GetFirstGenericArgument() : self;
        }

        public static MethodInfo GetInvokeMethod(this Type self)
        {
            return self.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        }

        public static MethodInfo MakeGenericMethodFrom(this MethodInfo self, MethodInfo method)
        {
            return self.MakeGenericMethod(method.GetGenericArguments());
        }

        public static Type[] GetParameterTypes(this MethodBase self)
        {
            var parameters = self.GetParameters();
            var types = new Type[parameters.Length];

            for (int i = 0; i < types.Length; i++)
                types[i] = parameters[i].ParameterType;

            return types;
        }

        private static bool ArrayTypeIsAssignableTo(Type type, Type candidate)
        {
            return type.IsArray
                   && candidate.IsArray
                   && type.GetArrayRank() == candidate.GetArrayRank()
                   && type.GetElementType().IsAssignableTo(candidate.GetElementType());
        }

        public static void OnFieldOrProperty(
            this MemberInfo self, Action<FieldInfo> onfield, Action<PropertyInfo> onprop)
        {
            switch (self.MemberType)
            {
                case MemberTypes.Field:
                    onfield((FieldInfo)self);
                    return;
                case MemberTypes.Property:
                    onprop((PropertyInfo)self);
                    return;
                default:
                    throw new ArgumentException();
            }
        }

        public static T OnFieldOrProperty<T>(
            this MemberInfo self, Func<FieldInfo, T> onfield, Func<PropertyInfo, T> onprop)
        {
            switch (self.MemberType)
            {
                case MemberTypes.Field:
                    return onfield((FieldInfo)self);
                case MemberTypes.Property:
                    return onprop((PropertyInfo)self);
                default:
                    throw new ArgumentException();
            }
        }

        public static Type MakeStrongBoxType(this Type self)
        {
            return typeof(StrongBox<>).MakeGenericType(self);
        }

        private static class ReadOnlyCollectionOf<T>
        {
            public static readonly ReadOnlyCollection<T> Empty = new ReadOnlyCollection<T>(new T[0]);
        }

        internal static ReadOnlyCollection<TSource> ToReadOnlyCollection<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                return ReadOnlyCollectionOf<TSource>.Empty;

            if (!(source is ReadOnlyCollection<TSource> ro))
                return new ReadOnlyCollection<TSource>(source.ToArray());

            return ro;
        }

        public static Type MakeArrayType(this Type self)
        {
            return Type.GetType(self.FullName + "[]");
        }

        public static Type MakeArrayType(this Type self, int rank)
        {
            var builder = new Text.StringBuilder();
            for (int i = 1; i < rank; i++)
                builder.Append(",");

            return Type.GetType(self.FullName + "[" + builder + "]");
        }
    }
}
