using UnityEngine;
using System.Collections;

namespace RTEditor
{
    /// <summary>
    /// Contains helper functions which are useful when performing different
    /// math related calculations.
    /// </summary>
    public static class MathHelper
    {
        #region Public Static Functions
        /// <summary>
        /// When using the 'Mathf.Acos' function we run into the risk of having an exception thrown if the
        /// argument to that function resides outside the [-1, 1] range. This shouldn't happen too often,
        /// but it is certainly possible. Imagine a scenario in which we perform a dot product between 2
        /// normalized vectors which are perfectly aligned. Ideally, the dot product would return a result
        /// of exactly 1.0f, but because of floating point rounding errors, the result might exceed the value
        /// 1.0f a little bit. We might get somehting like 1.000001f. If we then use the resulting value as an
        /// argument to 'Mathf.Acos', we will be thrown an exception because the value that we specified is
        /// outside of the valid range. This function will make sure that the specified parameter is clamped
        /// to the correct range before 'Mathf.Acos' is called.
        /// </summary>
        /// <param name="cosine">
        /// The value whose arc cosine must be calculated. The function will make sure that this
        /// parameter resides inside the [-1, 1] range.
        /// </param>
        /// <returns>
        /// The arc cosine of the specified cosine value.
        /// </returns>
        public static float SafeAcos(float cosine)
        {
            // Clamp the specified value and then return the arc cosine
            cosine = Mathf.Max(-1.0f, Mathf.Min(1.0f, cosine));
            return Mathf.Acos(cosine);
        }

        /// <summary>
        /// Returns the number of digits inside 'number'.
        /// </summary>
        public static int GetNumberOfDigits(int number)
        {
            return number == 0 ? 1 : Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(number)) + 1);
        }
        #endregion
    }
}
