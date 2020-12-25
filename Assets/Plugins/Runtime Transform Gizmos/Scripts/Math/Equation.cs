using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This is a static class which contains functions that can be used to
    /// solve equations.
    /// </summary>
    public static class Equation
    {
        #region Public Static Functions
        /// <summary>
        /// Solves a quadratic equation using the specified coefficients. t1 and t2 are
        /// output parameters which represent the solutions to the equation. If the 
        /// equation has no solutions, both of these will be set to 'float.MaxValue'. If
        /// the equation has a single solution, both will be set to the same value. If the
        /// equation has 2 solutions, t1 and t1 will contains the 2 solutions with t1 being
        /// smaller than t2.
        /// </summary>
        /// <returns>
        /// True if the equation has at least one solution or false otherwise.
        /// </returns>
        public static bool SolveQuadratic(float a, float b, float c, out float t1, out float t2)
        {
            // Calculate delta. If negative, the equation has no solutions.
            float delta = b * b - 4.0f * a * c;
            if (delta < 0.0f)
            {
                t1 = t2 = float.MaxValue;
                return false;
            }

            // If delta is 0, the equation has only one solution.
            if (delta == 0.0f)
            {
                t1 = t2 = -b / (2.0f * a);
                return true;
            }
            else
            {
                // delta is greater than 0 which means that the equation has 2 solutions. Calculate
                // t1 and t2 using the forumla t = (+/-sqrt(delta))/(2 * a);
                float _2TimesA = 2.0f * a;
                float sqrtDelta = Mathf.Sqrt(delta);

                t1 = (-b + sqrtDelta) / _2TimesA;
                t2 = (-b - sqrtDelta) / _2TimesA;

                // Swap t values if t1 is greater than t2
                if (t1 > t2)
                {
                    float tSwap = t1;
                    t1 = t2;
                    t2 = tSwap;
                }

                return true;
            }
        }
        #endregion
    }
}