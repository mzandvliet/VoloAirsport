using System;
using System.Collections.Generic;

namespace RamjetAnvil.DependencyInjection
{
    public static class ReflectionUtil
    {
        public static IEnumerable<Type> GetAllTypes(this Type type) {
            yield return type;

            // is there any base type?
            if ((type == null) || (type.BaseType == null)) {
                yield break;
            }

            // return all implemented or inherited interfaces
            foreach (var interfaceType in type.GetInterfaces()) {
                yield return interfaceType;
            }

            // return all inherited types
            var currentBaseType = type.BaseType;
            while (currentBaseType != null) {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType;
            }
        }
    }
}
