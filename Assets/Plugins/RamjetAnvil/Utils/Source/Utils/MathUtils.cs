using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility
{
    public static class MathUtils {
        public const float TwoPi = Mathf.PI * 2f;
        public const float HalfPi = Mathf.PI * 0.5f;
        public const float OneOver90 = 1f / 90f;
        public const float OneOver180 = 1f / 180f;

        public static float ScaleQuadratically(float input, float power) {
            return Mathf.Sign(input) * Mathf.Pow(Mathf.Clamp01(Mathf.Abs(input)), power);
        }

        public static Vector3 ScaleQuadratically(Vector3 input, float power) {
            return new Vector3(
                ScaleQuadratically(input.x, power),
                ScaleQuadratically(input.y, power),
                ScaleQuadratically(input.z, power));
        }

        /// <summary>
        /// Easing equation function for an exponential (2^time) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="time">Current time in seconds.</param>
        /// <param name="startVal">Starting value.</param>
        /// <param name="endVal">Final value.</param>
        /// <param name="duration">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        public static float ExpoEaseInOut(float time, float startVal, float endVal, float duration) {
            if (Mathf.Abs(time - 0f) < Single.Epsilon)
                return startVal;

            if (Mathf.Abs(time - duration) < Single.Epsilon)
                return startVal + endVal;

            if ((time /= duration / 2f) < 1f)
                return endVal / 2f * Mathf.Pow(2f, 10f * (time - 1f)) + startVal;

            return endVal / 2f * (-Mathf.Pow(2f, -10f * --time) + 2f) + startVal;
        }

        /**
         * Make sure an angle stays within the range -360 < angle < 360.
         */

        public static float WrapAngle(float angle) {
            if (angle < -360f)
                angle += 360f;
            if (angle > 360f)
                angle -= 360f;
            return angle;
        }

        public static float ClampAngle(float angle, float min, float max, float wrapPoint) {
            if (angle < -wrapPoint)
                angle += 360.0f;
            if (angle > wrapPoint)
                angle -= 360.0f;
            return Mathf.Clamp(angle, min, max);
        }

        // Circular angle lerp using shortest path
        public static float LerpAngle(float a, float b, float l) {
            /* http://stackoverflow.com/questions/2708476/rotation-interpolation
             * 
             * "I think a better approach is to interpolate sin and cos since they don't suffer form being multiply defined.
             * Let w = "amount" so that w = 0 is angle A and w = 1 is angle B. Then:
             * 
             * CS = (1-w)*cos(A) + w*cos(B);
             * SN = (1-w)*sin(A) + w*sin(B);
             * C = atan2(SN,CS);
             * 
             * One has to convert to radians and degrees as needed. One also has to adjust the branch. For atan2 C comes back
             * in the range -pi to pi. If you want 0 to 2pi then just add pi to C."
             * 
             * -- Paul Colby
             */

            a *= Mathf.Deg2Rad;
            b *= Mathf.Deg2Rad;

            float cs = (1 - l) * Mathf.Cos(a) + l * Mathf.Cos(b);
            float cn = (1 - l) * Mathf.Sin(a) + l * Mathf.Sin(b);

            float rRad = Mathf.Atan2(cn, cs);
            float rDeg = rRad * Mathf.Rad2Deg;

            return rDeg;
        }

        /**
         * Clamps a value between -1 < 0 < 1.
         * Really, this is made redundant by Mathf.Clamp, innit?
         */

        public static float ClampUnitValue(float value) {
            if (value > 1f) value = 1f;
            if (value < -1f) value = -1f;
            return value;
        }

        /// <summary>
        /// Works same as % modulo operator, but optimized. Mod value needs to be power of two.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        public static int FastSqrModulo(int num, int mod) {
            return num & (mod - 1);
        }

        public static int PositiveModulo(this int i, int n) {
            return (i % n + n) % n;
        }

        public static float RoundToDecimals(float number, int numDecimals) {
            float scale = Mathf.Pow(10f, numDecimals);
            return Mathf.RoundToInt(number * scale) / scale;
        }

        public static double RoundToDecimals(double number, int numDecimals) {
            return Math.Round(number, numDecimals);
        }

        /// <summary>
        /// Determine the signed angle between two vectors a and b, around an axis
        /// </summary>
        public static float AngleAroundAxis(Vector3 a, Vector3 b, Vector3 axis) {
            // Naive

            // Project vectors onto plane defined by axis
            //Vector3 projectedA = (a - Vector3.Project(a, axis)).normalized;
            //Vector3 projectedB = (startVal - Vector3.Project(startVal, axis)).normalized;
            //// Find angle
            //float dot = Vector3.Dot(projectedA, projectedB);
            //return Mathf.Acos(dot)*Mathf.Rad2Deg;

            // Faster

            return Mathf.Atan2(
                Vector3.Dot(axis, Vector3.Cross(a, b)),
                Vector3.Dot(a, b)
                ) * Mathf.Rad2Deg;
        }

        public static Vector3 ProjectOnPlane(Vector3 inVector, Vector3 planeNormal) {
            // Todo: Is this cheaper than normalizing a flattened vector?
            return Vector3.Cross(planeNormal, (Vector3.Cross(inVector, planeNormal)));
        }

        public static bool Intersect(Vector3 bMin, Vector3 bMax, Vector3 sPos, float sRadius) {
            float sqrRadius = Sqr(sRadius);
            float minDist = 0f;
            for (int i = 0; i < 3; i++) {
                if (sPos[i] < bMin[i]) minDist += Sqr(sPos[i] - bMin[i]);
                else if (sPos[i] > bMax[i]) minDist += Sqr(sPos[i] - bMax[i]);
            }
            return minDist <= sqrRadius;
        }

        public static float Sqr(float val) {
            return val * val;
        }
    }
}