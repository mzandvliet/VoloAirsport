using System.Collections.Generic;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility
{
    /* Taken from decompiled AudioSourceInspector in UnityEditor library. */
    public static class AnimationCurveUtils
    {
        public static AnimationCurve Logarithmic(float timeStart, float timeEnd, float logBase)
        {
            List<Keyframe> list = new List<Keyframe>();
            float num1 = 2f;
            timeStart = Mathf.Max(timeStart, 0.0001f);
            float num2 = timeStart;
            while ((double)num2 < (double)timeEnd)
            {
                float num3 = LogarithmicValue(num2, timeStart, logBase);
                float num4 = num2 / 50f;
                float num5 = (float)(((double)LogarithmicValue(num2 + num4, timeStart, logBase) - (double)LogarithmicValue(num2 - num4, timeStart, logBase)) / ((double)num4 * 2.0));
                list.Add(new Keyframe(num2, num3, num5, num5));
                num2 *= num1;
            }
            float num6 = LogarithmicValue(timeEnd, timeStart, logBase);
            float num7 = timeEnd / 50f;
            float num8 = (float)(((double)LogarithmicValue(timeEnd + num7, timeStart, logBase) - (double)LogarithmicValue(timeEnd - num7, timeStart, logBase)) / ((double)num7 * 2.0));
            list.Add(new Keyframe(timeEnd, num6, num8, num8));
            return new AnimationCurve(list.ToArray());
        }

        public static float LogarithmicValue(float distance, float minDistance, float rolloffScale)
        {
            if ((double)distance > (double)minDistance && (double)rolloffScale != 1.0)
            {
                distance -= minDistance;
                distance *= rolloffScale;
                distance += minDistance;
            }
            if ((double)distance < 9.99999997475243E-07)
                distance = 1E-06f;
            return minDistance / distance;
        }

        public static AnimationCurve Constant(float value)
        {
            AnimationCurve curve = new AnimationCurve(new[] { new Keyframe(0f, value) });
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            return curve;
        }
    }
}