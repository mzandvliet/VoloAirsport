using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RamjetAnvil.Unity.Utility {

    /// <summary>
    ///     A fast and efficient implementation of
    ///     <see cref="IEqualityComparer{T}" /> for Enum types.
    ///     Useful for dictionaries that use Enums as their keys.
    ///     From: http://www.codeproject.com/Articles/33528/Accelerating-Enum-Based-Dictionaries-with-Generic
    /// </summary>
    /// <example>
    ///     <code>
    /// var dict = new Dictionary&lt;DayOfWeek, 
    /// string&gt;(EnumComparer&lt;DayOfWeek&gt;.Instance);
    /// </code>
    /// </example>
    /// <typeparam name="TEnum">The type of the Enum.</typeparam>
    public sealed class EnumComparer<TEnum> : IEqualityComparer<TEnum>, IComparer<TEnum>
        where TEnum : struct, IComparable, IConvertible, IFormattable {
        private static readonly Func<TEnum, TEnum, int> compare;

        private static readonly Func<TEnum, TEnum, bool> equals;
        private static readonly Func<TEnum, int> getHashCode;

        /// <summary>
        ///     The singleton accessor.
        /// </summary>
        public static readonly EnumComparer<TEnum> Instance;

        /// <summary>
        ///     Initializes the <see cref="EnumComparer{TEnum}" /> class
        ///     by generating the GetHashCode and Equals methods.
        /// </summary>
        static EnumComparer() {
            getHashCode = EnumUtils.GenerateGetHashCode<TEnum>();
            equals = EnumUtils.GenerateEquals<TEnum>();
            Instance = new EnumComparer<TEnum>();
            compare = GenerateCompare();
        }

        /// <summary>
        ///     A private constructor to prevent user instantiation.
        /// </summary>
        private EnumComparer() {
            EnumUtils.AssertTypeIsEnum<TEnum>();
            EnumUtils.AssertUnderlyingTypeIsSupported<TEnum>();
        }

        int IComparer<TEnum>.Compare(TEnum x, TEnum y) {
            // Note that the delegate is generated only when first triggered.

            // I use the same approach at Equals and GetHashCode, too, because
            // even reflection is hundred times faster than compiling a lambda expression,
            // while this check against null is almost a negligible overhead
            return compare(x, y);
        }

        /// <summary>
        ///     Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">
        ///     The first object of type <typeparamref name="TEnum" />
        ///     to compare.
        /// </param>
        /// <param name="y">
        ///     The second object of type <typeparamref name="TEnum" />
        ///     to compare.
        /// </param>
        /// <returns>
        ///     true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(TEnum x, TEnum y) {
            // call the generated method
            return equals(x, y);
        }

        /// <summary>
        ///     Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">
        ///     The <see cref="T:System.Object" />
        ///     for which a hash code is to be returned.
        /// </param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///     The type of <paramref name="obj" /> is a reference type and
        ///     <paramref name="obj" /> is null.
        /// </exception>
        public int GetHashCode(TEnum obj) {
            // call the generated method
            return getHashCode(obj);
        }

        public static Func<TEnum, TEnum, int> GenerateCompare() {
            // This implementation calls CompareTo on underlying type: x.CompareTo(y)

            Type underlyingType = Enum.GetUnderlyingType(typeof (TEnum));
            ParameterExpression xParameter = Expression.Parameter(typeof (TEnum), "x");
            ParameterExpression yParameter = Expression.Parameter(typeof (TEnum), "y");
            UnaryExpression xCastedToUnderlyingType = Expression.Convert(xParameter, underlyingType);
            UnaryExpression yCastedToUnderlyingType = Expression.Convert(yParameter, underlyingType);
            MethodCallExpression compareToCall = Expression.Call(xCastedToUnderlyingType,
                underlyingType.GetMethod("CompareTo", new[] {underlyingType}), yCastedToUnderlyingType);

            return Expression.Lambda<Func<TEnum, TEnum, int>>(compareToCall, xParameter, yParameter).Compile();
        }
    }
}