using System;
using System.Collections.Generic;
using System.Reflection;

namespace Eurofurence.App.Server.Web.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> BaseTypesAndSelf(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.GetTypeInfo().BaseType;
            }
        }
    }
}