using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RamjetAnvil.Unity.Utility {
    public static class EnumUtils {

        public static T Parse<T>(string name) {
            return (T)Enum.Parse(typeof (T), name);
        }

        public static T FromString<T>(string value) {
            return (T) Enum.Parse(typeof (T), value);
        }

        public static string ToPrettyString<TEnum>(TEnum @enum) {
            return Regex.Replace(@enum.ToString(), "([A-Z])", "$1");
        }

        private static readonly IDictionary<Type, object> CachedEnumValues = new Dictionary<Type, object>(); 
        public static IList<TEnum> GetValues<TEnum>() {
            AssertTypeIsEnum<TEnum>();
            object enumValues;
            if (!CachedEnumValues.TryGetValue(typeof (TEnum), out enumValues)) {
                enumValues = Enum.GetValues(typeof (TEnum)).Cast<TEnum>().ToListOptimized();
                CachedEnumValues[typeof (TEnum)] = enumValues;
            }
            return enumValues as IList<TEnum>;
        }

        public static int IndexOf<TEnum>(TEnum elem) {
            AssertTypeIsEnum<TEnum>();
            return GetValues<TEnum>().IndexOf(elem);
        }

        private static readonly IDictionary<Type, object> EnumToIntCache = new Dictionary<Type, object>();
        public static Func<TEnum, int> EnumToInt<TEnum>() {
            AssertTypeIsEnum<TEnum>();
            object enumToInt;
            if (!EnumToIntCache.TryGetValue(typeof (TEnum), out enumToInt)) {
                enumToInt = GenerateGetHashCode<TEnum>();
                EnumToIntCache[typeof (TEnum)] = enumToInt;
            }
            return enumToInt as Func<TEnum, int>;
        }

        private static readonly IDictionary<Type, object> IntToEnumCache = new Dictionary<Type, object>();
        public static Func<int, TEnum> IntToEnum<TEnum>() {
            AssertTypeIsEnum<TEnum>();
            object intToEnum;
            if (!IntToEnumCache.TryGetValue(typeof (TEnum), out intToEnum)) {
                var indexBasedArray = IndexBasedArray<TEnum>();
                intToEnum = (Func<int, TEnum>)(i => indexBasedArray[i]);
                IntToEnumCache[typeof (TEnum)] = intToEnum;
            }
            return intToEnum as Func<int, TEnum>;
        }

        public static TEnum[] IndexBasedArray<TEnum>() {
            AssertTypeIsEnum<TEnum>();
            var getHashCode = GenerateGetHashCode<TEnum>();
            var enumValues = GetValues<TEnum>();
            var highestIndex = 0;
            for (int i = 0; i < enumValues.Count; i++) {
                var enumValue = enumValues[i];
                var enumIndex = getHashCode(enumValue);
                highestIndex = highestIndex < enumIndex ? enumIndex : highestIndex;
            }
            var enumArray = new TEnum[highestIndex + 1];
            for (int i = 0; i < enumValues.Count; i++) {
                var enumValue = enumValues[i];
                enumArray[getHashCode(enumValue)] = enumValue;
            }
            return enumArray;
        }

        private static readonly IDictionary<Type, int> HighestEnumIndexCache = new Dictionary<Type, int>();
        public static int HighestEnumIndex<TEnum>() {
            AssertTypeIsEnum<TEnum>();
            int highestIndex;
            if (!HighestEnumIndexCache.TryGetValue(typeof (TEnum), out highestIndex)) {
                var getHashCode = GenerateGetHashCode<TEnum>();
                var enumValues = GetValues<TEnum>();
                highestIndex = 0;
                for (int i = 0; i < enumValues.Count; i++) {
                    var enumValue = enumValues[i];
                    var enumIndex = getHashCode(enumValue);
                    highestIndex = highestIndex < enumIndex ? enumIndex : highestIndex;
                }
                HighestEnumIndexCache[typeof (TEnum)] = highestIndex;
            }
            return highestIndex;
        }

        public static void AssertTypeIsEnum<TEnum>() {
            if (typeof (TEnum).IsEnum) {
                return;
            }
            string message = string.Format(
                "The type parameter {0} is not an Enum. LcgEnumComparer supports Enums only.", typeof (TEnum));
            throw new NotSupportedException(message);
        }

        public static void AssertUnderlyingTypeIsSupported<TEnum>() {
            Type underlyingType = Enum.GetUnderlyingType(typeof (TEnum));
            ICollection<Type> supportedTypes =
                new[] {
                    typeof (byte), typeof (sbyte), typeof (short), typeof (ushort),
                    typeof (int), typeof (uint), typeof (long), typeof (ulong)
                };
            if (supportedTypes.Contains(underlyingType)) {
                return;
            }
            string message =
                string.Format("The underlying type of the type parameter {0} is {1}. " +
                              "LcgEnumComparer only supports Enums with underlying type of " +
                              "byte, sbyte, short, ushort, int, uint, long, or ulong.",
                    typeof (TEnum), underlyingType);
            throw new NotSupportedException(message);
        }

        public static Func<TEnum, TEnum, bool> GenerateEquals<TEnum>() {
            ParameterExpression xParam = Expression.Parameter(typeof (TEnum), "x");
            ParameterExpression yParam = Expression.Parameter(typeof (TEnum), "y");
            BinaryExpression equalExpression = Expression.Equal(xParam, yParam);
            return Expression.Lambda<Func<TEnum, TEnum, bool>>(equalExpression, new[] {xParam, yParam}).Compile();
        }

        public static Func<TEnum, int> GenerateGetHashCode<TEnum>() {
            ParameterExpression objParam = Expression.Parameter(typeof (TEnum), "obj");
            Type underlyingType = Enum.GetUnderlyingType(typeof (TEnum));
            UnaryExpression convertExpression = Expression.Convert(objParam, underlyingType);
            MethodInfo getHashCodeMethod = underlyingType.GetMethod("GetHashCode");
            MethodCallExpression getHashCodeExpression = Expression.Call(convertExpression, getHashCodeMethod);
            return Expression.Lambda<Func<TEnum, int>>(getHashCodeExpression, new[] {objParam}).Compile();
        }

        public static Func<int, TEnum> GenerateHashCodeToEnum<TEnum>() {
            ParameterExpression objParam = Expression.Parameter(typeof (TEnum), "obj");
            Type underlyingType = Enum.GetUnderlyingType(typeof (TEnum));
            UnaryExpression convertExpression = Expression.Convert(objParam, underlyingType);
            MethodInfo getHashCodeMethod = underlyingType.GetMethod("GetHashCode");
            MethodCallExpression getHashCodeExpression = Expression.Call(convertExpression, getHashCodeMethod);
            return Expression.Lambda<Func<int, TEnum>>(getHashCodeExpression, new[] {objParam}).Compile();
        }
    }
}