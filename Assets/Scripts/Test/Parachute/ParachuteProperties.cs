using RamjetAnvil.Volo.Util.UnitOfMeasure;
using UnityEngine;

namespace RamjetAnvil.Volo {
    
    public struct ParachuteProperties<T> where T : MeasureSystem {
        public Measure<float> Span;
        public Measure<float> Chord;
        public Measure<Vector3> RigAttachPosition;
        public Measure<float> PilotWeight;
        public Measure<float> HeightOffset;
        public Measure<float> CanopyMass;
        public Measure<float> WeightShiftMagnitude;
        public Measure<float> RiggingAngle;

        public Measure<Vector3> Velocity;

        public ParachuteProperties(Measure<float> span, Measure<float> chord, Measure<Vector3> rigAttachPosition, 
            Measure<float> pilotWeight, Measure<float> heightOffset, Measure<float> weightShiftMagnitude,
            Measure<float> canopyMass, Measure<float> riggingAngle, Measure<Vector3> velocity) {

            Span = span;
            Chord = chord;
            RigAttachPosition = rigAttachPosition;
            PilotWeight = pilotWeight;
            HeightOffset = heightOffset;
            CanopyMass = canopyMass;
            WeightShiftMagnitude = weightShiftMagnitude;
            RiggingAngle = riggingAngle;
            Velocity = velocity;
        }

        public Measure<float> Area {
            get { return new Measure<float>(Span.Value * Chord.Value, IsMetric ? "m²" : "ft²"); }
        }

        public Measure<float> WingLoading {
            get { return new Measure<float>(PilotWeight.Value / Area.Value, IsMetric ? "kg/m²" : "oz/ft²"); }
        }

        public Measure<float> Speed {
            get {
                return new Measure<float>(Velocity.Value.magnitude, Velocity.Unit);
            }
        }

        public Measure<float> HorizontalSpeed {
            get {
                var horizontalVelocity = Velocity.Value;
                horizontalVelocity.y = 0;
                return new Measure<float>(horizontalVelocity.magnitude, Velocity.Unit);
            }
        }

        public Measure<float> VerticalSpeed {
            get {
                return new Measure<float>(Velocity.Value.y, Velocity.Unit);   
            }
        }

        public Measure<float> GlideRatio {
            get {
                return new Measure<float>(HorizontalSpeed.Value / VerticalSpeed.Value, "");
            }
        }

        private static bool IsMetric {
            get { return typeof(T) == typeof(MeasureSystem.Metric); }
        }
    }

    public static class ParachuteProperties {

        public static ParachuteProperties<MeasureSystem.Metric> FromConfig(ParachuteConfig config, Parachute parachute) {
            var props = FromConfig(config);
            props.Velocity = new Measure<Vector3>(parachute.Pilot.Torso.velocity * 3.6f, "km/h");
            return props;
        }

        public static ParachuteProperties<MeasureSystem.Metric> FromConfig(ParachuteConfig config) {
            return new ParachuteProperties<MeasureSystem.Metric>(
                span: new Measure<float>(config.Span, "m"), 
                chord: new Measure<float>(config.Chord, "m"), 
                rigAttachPosition: new Measure<Vector3>(config.RigAttachPos, "m"), 
                pilotWeight: new Measure<float>(config.PilotWeight, "kg"), 
                heightOffset: new Measure<float>(config.HeightOffset, "m"), 
                weightShiftMagnitude: new Measure<float>(config.WeightshiftMagnitude, "m"), 
                canopyMass: new Measure<float>(config.Mass, "kg"), 
                riggingAngle: new Measure<float>(config.RiggingAngle, "°"),
                velocity: new Measure<Vector3>(Vector3.zero, "km/h"));
        }

        public static ParachuteProperties<MeasureSystem.Imperial> ToImperial(this ParachuteProperties<MeasureSystem.Metric> props) {
            return new ParachuteProperties<MeasureSystem.Imperial>(
                span: new Measure<float>(MeasureSystem.MetersToFeet(props.Span.Value), "ft"), 
                chord: new Measure<float>(MeasureSystem.MetersToFeet(props.Chord.Value), "ft"), 
                rigAttachPosition: new Measure<Vector3>(MeasureSystem.MetersToFeet(props.RigAttachPosition.Value), "ft"), 
                pilotWeight: new Measure<float>(MeasureSystem.KilogramsToPounds(props.PilotWeight.Value), "lbs"), 
                heightOffset: new Measure<float>(MeasureSystem.MetersToFeet(props.HeightOffset.Value), "ft"), 
                weightShiftMagnitude: new Measure<float>(MeasureSystem.MetersToFeet(props.WeightShiftMagnitude.Value), "ft"), 
                canopyMass: new Measure<float>(MeasureSystem.KilogramsToOunces(props.CanopyMass.Value), "oz"), 
                riggingAngle: props.RiggingAngle,
                velocity: new Measure<Vector3>(MeasureSystem.KmhToMph(props.Velocity.Value), "mph"));
        }
    }
}
