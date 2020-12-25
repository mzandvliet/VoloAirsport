using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {

    public class MetersView : MonoBehaviour {

        [SerializeField] private LinePullGaugeView _leftLinePullGauge;
        [SerializeField] private LinePullGaugeView _rightLinePullGauge;
        [SerializeField] private Text _holdLines;
        [SerializeField] private MeterView _speed;
        [SerializeField] private Text _speedDetail;
        [SerializeField] private MeterView _altitude;
        [SerializeField] private MeterView _glideRatio;
        [SerializeField] private MeterView _gforce;
        
        private MutableString _speedStr;
        private MutableString _speedDetailStr;
        private MutableString _altitudeStr;
        private MutableString _glideRatioStr;
        private MutableString _gForceStr;

        void Awake() {
            _speedStr = new MutableString(32);
            _speedDetailStr = new MutableString(64);
            _altitudeStr = new MutableString(32);
            _glideRatioStr = new MutableString(32);
            _gForceStr = new MutableString(32);
        }

        public void SetTitles(MeasurementTitles titles) {
            _speed.SetTitle(titles.Speed);
            _altitude.SetTitle(titles.Altitude);
            _glideRatio.SetTitle(titles.GlideRatio);
            _gforce.SetTitle(titles.GForce);
        }

        public void SetData(MeasurementViewData data) {
            _leftLinePullGauge.SetPullForce(data.LineColor, data.LeftLinePull);
            _rightLinePullGauge.SetPullForce(data.LineColor, data.RightLinePull);

            _holdLines.color = data.LineColor;
            _holdLines.SetMutableString(data.HoldLines);
            
            _speedStr
                .Clear()
                .Append("↘ ")
                .Append(data.Speed)
                .Append(" ")
                .Append(data.SpeedUnit);
            _speed.SetValue(_speedStr);

            _speedDetailStr.Clear();
            const string indentation = "    ";
            _speedDetailStr.Append(indentation)
                .Append("→ ")
                .Append(data.HorizontalSpeed)
                .Append(" ")
                .Append(data.SpeedUnit)
                .Append(Environment.NewLine)
                .Append(indentation)
                .Append("↓ ")
                .Append(data.VerticalSpeed)
                .Append(" ")
                .Append(data.SpeedUnit);
            _speedDetail.SetMutableString(_speedDetailStr);

            _altitudeStr
                .Clear()
                .Append("↕ ")
                .Append(data.Altitude)
                .Append(data.AltitudeUnit);
            _altitude.SetValue(_altitudeStr);

            WriteValue(_glideRatio, _glideRatioStr, data.GlideRatio);
            WriteValue(_gforce, _gForceStr, data.GForce);
        }

        private void WriteValue(MeterView widget, MutableString s, MutableString value, string unitOfMeasure = "") {
            s.Clear().
                Append(value)
                .Append(" ")
                .Append(unitOfMeasure);

            widget.SetValue(s);
        }

        public void Show() {
            this.gameObject.SetActive(true);
        }

        public void Hide() {
            this.gameObject.SetActive(false);
        }
    }
}
