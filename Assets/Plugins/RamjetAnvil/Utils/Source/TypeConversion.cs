using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RamjetAnvil.Unity.Utility {
    public static class TypeConversion {

        public static bool CanConvertTwoWay<TSource, TTarget>() {
            return CanConvert<TSource, TTarget>() && CanConvert<TTarget, TSource>();
        }

        public static bool CanConvert<TSource, TTarget>() {
            try {
                GetConverter<TSource, TTarget>();
                return true;
            } catch (Exception) {
                return false;
            }
        }

        private static readonly IDictionary<Type, object> ConverterCache = new Dictionary<Type, object>(); 
        /// <summary>
        /// Create a converter for a type to another type as long as the type has 
        /// implicit or explicit operators to convert itself to the target type.
        /// </summary>
        public static Func<TSource, TTarget> GetConverter<TSource, TTarget>() {
            var converterType = typeof (Func<TSource, TTarget>);
            object converter;
            if (!ConverterCache.TryGetValue(converterType, out converter)) {
                try {
                    ParameterExpression inputParam = Expression.Parameter(typeof (TSource), "obj");
                    Expression conversionExpression = Expression.Convert(inputParam, typeof (TTarget));
                    converter =
                        Expression.Lambda<Func<TSource, TTarget>>(conversionExpression, new[] {inputParam}).Compile();
                    ConverterCache[converterType] = converter;
                } catch (InvalidOperationException e) {
                    throw new Exception("Conversion method '" + typeof(TSource) + " -> " + typeof(TTarget) + "' unavailable", e);
                }
            }
            return converter as Func<TSource, TTarget>;
        }

        /// <summary>
        /// Creates a converter pair that converts a type to an int and vice-versa.
        /// The converter can be generated for any type that is convertible to a primitive
        /// type that is convertible to an int (int, long, short, byte and their unsigned counter parts)
        /// </summary>
        /// <typeparam name="TSource">The type to create the converter for</typeparam>
        /// <returns>A converter pair</returns>
        public static IntConvertible<TSource> IntConverter<TSource>() {
            Func<TSource, int> toInt;
            Func<int, TSource> fromInt;
            if (CanConvertTwoWay<TSource, int>()) {
                toInt = GetConverter<TSource, int>();
                fromInt = GetConverter<int, TSource>();   
            } else if (CanConvertTwoWay<TSource, long>()) {
                var to = GetConverter<TSource, long>();
                toInt = key => (int) to(key);
                var from = GetConverter<long, TSource>();
                fromInt = index => from(index);
            } else if (CanConvertTwoWay<TSource, uint>()) {
                var to = GetConverter<TSource, uint>();
                toInt = key => (int) to(key);
                var from = GetConverter<uint, TSource>();
                fromInt = index => from((uint) index);
            } else if (CanConvertTwoWay<TSource, ulong>()) {
                var to = GetConverter<TSource, ulong>();
                toInt = key => (int) to(key);
                var from = GetConverter<ulong, TSource>();
                fromInt = index => from((ulong) index);
            } else if (CanConvertTwoWay<TSource, ushort>()) {
                var to = GetConverter<TSource, ushort>();
                toInt = key => (int) to(key);
                var from = GetConverter<ushort, TSource>();
                fromInt = index => from((ushort) index);
            } else if (CanConvertTwoWay<TSource, short>()) {
                var to = GetConverter<TSource, short>();
                var from = GetConverter<short, TSource>();
                toInt = key => (int) to(key);
                fromInt = index => from((short) index);
            } else if (CanConvertTwoWay<TSource, byte>()) {
                var to = GetConverter<TSource, byte>();
                toInt = key => (int) to(key);
                var from = GetConverter<byte, TSource>();
                fromInt = index => from((byte) index);
            } else {
                throw new Exception("Type " + typeof(TSource) + " cannot be converted to int");
            }
            return new IntConvertible<TSource>(toInt, fromInt);
        }

        public struct IntConvertible<TSource> {
            public readonly Func<TSource, int> ToInt;
            public readonly Func<int, TSource> FromInt;

            public IntConvertible(Func<TSource, int> toInt, Func<int, TSource> fromInt) {
                ToInt = toInt;
                FromInt = fromInt;
            }
        }
    }
}
