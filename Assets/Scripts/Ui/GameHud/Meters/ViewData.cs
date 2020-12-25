using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {

    public struct MeasurementTitles {
        public string Speed;
        public string Altitude;
        public string GlideRatio;
        public string GForce;
    }

    public class MeasurementViewData {
        public MutableString HoldLines;
        public Color LineColor;

        public float LeftLinePull;
        public float RightLinePull;

        public MutableString Speed;
        public string SpeedUnit;

        public MutableString HorizontalSpeed;
        public MutableString VerticalSpeed;

        public MutableString Altitude;
        public string AltitudeUnit;

        public MutableString GlideRatio;
        public MutableString GForce;

        public MeasurementViewData() {
            HoldLines = new MutableString(32);
            Speed = new MutableString(16);
            HorizontalSpeed = new MutableString(16);
            VerticalSpeed = new MutableString(16);
            Altitude = new MutableString(16);
            GlideRatio = new MutableString(16);
            GForce = new MutableString(16);
        }

        public void Clear() {
            LineColor = Color.white;
            HoldLines.Clear();
            Speed.Clear();
            HorizontalSpeed.Clear();
            VerticalSpeed.Clear();
            Altitude.Clear();
            GlideRatio.Clear();
            GForce.Clear();
        }
    }
}
