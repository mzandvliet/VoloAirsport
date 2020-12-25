using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Volo.Util.UnitOfMeasure;
using StringLeakTest;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {

    public struct FlightViewData<T> where T : MeasureSystem {
        public readonly float LeftLinePull;
        public readonly float RightLinePull;
        public readonly Measure<Vector3> Speed;
        public readonly Measure<float> Altitude;
        public readonly float GlideRatio;
        public readonly float Gforce;
        public readonly ParachuteLine? ActiveParachuteLines;

        public FlightViewData(Measure<Vector3> speed, Measure<float> altitude, float glideRatio, float gforce, ParachuteLine? activeParachuteLines, 
            float leftLinePull, float rightLinePull) {

            Speed = speed;
            Altitude = altitude;
            GlideRatio = glideRatio;
            Gforce = gforce;
            ActiveParachuteLines = activeParachuteLines;
            LeftLinePull = leftLinePull;
            RightLinePull = rightLinePull;
        }

        public Measure<float> AirSpeed {
            get { return new Measure<float>(Speed.Value.magnitude, Speed.Unit); }
        }

        public Measure<float> HorizontalAirSpeed {
            get {
                var horizontalVelocity = Speed.Value;
                horizontalVelocity.y = 0;
                return new Measure<float>(horizontalVelocity.magnitude, Speed.Unit);
            }
        }

        public Measure<float> VerticalAirSpeed {
            get { return new Measure<float>(-Speed.Value.y, Speed.Unit); }
        }

        public static FlightViewData<T> Create(Vector3 speed, float altitude, float glideRatio, float gforce, ParachuteLine? holdLines,
            Vector2 linePull) {

            var metricData = new FlightViewData<T>(
                speed: new Measure<Vector3>((speed * 60 * 60) / 1000, "km/h"),
                altitude: new Measure<float>(altitude, "m"),
                glideRatio: glideRatio,
                gforce: gforce,
                activeParachuteLines: holdLines,
                leftLinePull: linePull.x,
                rightLinePull: linePull.y);
            if (typeof(T) == typeof(MeasureSystem.Imperial)) {
                return new FlightViewData<T>(
                    speed: new Measure<Vector3>(MeasureSystem.KmhToMph(metricData.Speed.Value), "mph"),
                    altitude: new Measure<float>(MeasureSystem.MetersToFeet(metricData.Altitude.Value), "feet"),
                    glideRatio: metricData.GlideRatio,
                    gforce: metricData.Gforce,
                    activeParachuteLines: metricData.ActiveParachuteLines,
                    leftLinePull: linePull.x,
                    rightLinePull: linePull.y);
            }
            return metricData;
        }
    }

    public class MetersViewModel {

        public event Action<MeasurementViewData> DataUpdated;
        public event Action<MeasurementTitles> TitlesUpdated;

        private MeasurementTitles _titles;
        private readonly MeasurementViewData _data;

        public MetersViewModel() {
            _data = new MeasurementViewData();
            _titles = new MeasurementTitles {
                Speed = "Speed",
                Altitude = "Altitude",
                GlideRatio = "Glide Ratio",
                GForce = "G-Force"
            };
        }

        public void SetLanguage(LanguageTable languageTable) {
            _titles = new MeasurementTitles {
                Speed = languageTable.Table["air_speed"],
                Altitude = languageTable.Table["altitude"],
                GlideRatio = languageTable.Table["glide_ratio"],
                GForce = languageTable.Table["g_force"]
            };
            if (TitlesUpdated != null) {
                TitlesUpdated(_titles);
            }
        }

        public void UpdateRawData<T>(ref FlightViewData<T> rawData) where T : MeasureSystem {
            _data.Clear();

            _data.LeftLinePull = rawData.LeftLinePull;
            _data.RightLinePull = rawData.RightLinePull;

            _data.HoldLines.Append(rawData.ActiveParachuteLines.HasValue ? rawData.ActiveParachuteLines.Value.Name() : "");
            _data.LineColor = rawData.ActiveParachuteLines.HasValue ? rawData.ActiveParachuteLines.Value.Color() : Color.white;

            _data.Speed.Append(Mathf.RoundToInt(rawData.AirSpeed.Value));
            _data.SpeedUnit = rawData.AirSpeed.Unit;

            _data.HorizontalSpeed.Append(rawData.HorizontalAirSpeed.Value, 0);
            _data.VerticalSpeed.Append(rawData.VerticalAirSpeed.Value, 0);

            _data.Altitude.Append(Mathf.RoundToInt(rawData.Altitude.Value));
            _data.AltitudeUnit = rawData.Altitude.Unit;

            _data.GlideRatio.Append(rawData.GlideRatio, 2);
            _data.GForce.Append(rawData.Gforce, 2);

            if (DataUpdated != null) {
                DataUpdated(_data);
            }
        }
    }
}
