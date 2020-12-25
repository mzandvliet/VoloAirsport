using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RamjetAnvil.Volo {
    
    [Serializable, CreateAssetMenu]
    public class ParachuteConfig : ScriptableObject, IEquatable<ParachuteConfig> {
        public static readonly int VersionNumber = 2;

        [SerializeField]
        public string Id;
        [SerializeField]
        public string Name;
        [SerializeField]
        public Color Color;
        [SerializeField]
        public float RadiusHorizontal = 5f;
        [SerializeField]
        public float RadiusVertical = 2f;
        [SerializeField]
        public float HeightOffset = 4f;
        [SerializeField]
        public float Span = 7.5f;
        [SerializeField]
        public float Chord = 3.5f;
        [SerializeField]
        public int NumCells = 13;
        [SerializeField]
        public float Mass = 4f; // Todo: constant multiplied by area
        [SerializeField]
        public AirfoilDefinition AirfoilDefinition;
        [SerializeField]
        public float RiggingAngle = 5f;
        [SerializeField]
        public AnimationCurve PlanformAreaEllipse;
        [SerializeField]
        public float PressureMultiplier = 6f;
        [SerializeField]
        public Vector3 RigAttachPos = new Vector3(1f, 0.75f, 0f);
        [SerializeField]
        public int NumToggleControlledCells = 5;
        [SerializeField]
        public float RearRiserPullMagnitude = 0.1f;
        [SerializeField]
        public float FrontRiserPullMagnitude = 0.1f;
        [SerializeField]
        public float WeightshiftMagnitude = 0.33f;
        [SerializeField]
        public float PilotWeight = 40f;
        [SerializeField]
        public float InputGamma = 1f; // Todo: not part of parachute but player's input config
        [SerializeField]
        public float InputSmoothing = 0.25f;

        // Mesh creation
        [SerializeField]
        public AnimationCurve ChordLineCurvature;
        [SerializeField]
        public AnimationCurve ChordUpperThickness;
        [SerializeField]
        public AnimationCurve ChordLowerThickness;
        [SerializeField]
        public AnimationCurve SpanCellCurvature;
        public int CellChordLoops;
        public int CellSpanLoops;

        // Inidates whether we allow this parachute to be edited
        public bool IsEditable;

        public static ParachuteConfig CreateNew(ParachuteConfig prefab) {
            var config = Object.Instantiate(prefab);
            config.Id = Guid.NewGuid().ToString();
            config.IsEditable = true;
            return config;
        }

        public float Thickness {
            get { return Mathf.Abs(Mathf.Log(1f + ParachuteMaths.GetRectArea(this), .2f) * .3f); }
        }

        public bool Equals(ParachuteConfig other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && string.Equals(Id, other.Id) && string.Equals(Name, other.Name) && RadiusHorizontal.Equals(other.RadiusHorizontal) && RadiusVertical.Equals(other.RadiusVertical) && HeightOffset.Equals(other.HeightOffset) && Span.Equals(other.Span) && Chord.Equals(other.Chord) && NumCells == other.NumCells && Mass.Equals(other.Mass) && Equals(AirfoilDefinition, other.AirfoilDefinition) && RiggingAngle.Equals(other.RiggingAngle) && Equals(PlanformAreaEllipse, other.PlanformAreaEllipse) && PressureMultiplier.Equals(other.PressureMultiplier) && RigAttachPos.Equals(other.RigAttachPos) && NumToggleControlledCells == other.NumToggleControlledCells && RearRiserPullMagnitude.Equals(other.RearRiserPullMagnitude) && FrontRiserPullMagnitude.Equals(other.FrontRiserPullMagnitude) && WeightshiftMagnitude.Equals(other.WeightshiftMagnitude) && PilotWeight.Equals(other.PilotWeight) && InputGamma.Equals(other.InputGamma) && InputSmoothing.Equals(other.InputSmoothing);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ParachuteConfig) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RadiusHorizontal.GetHashCode();
                hashCode = (hashCode * 397) ^ RadiusVertical.GetHashCode();
                hashCode = (hashCode * 397) ^ HeightOffset.GetHashCode();
                hashCode = (hashCode * 397) ^ Span.GetHashCode();
                hashCode = (hashCode * 397) ^ Chord.GetHashCode();
                hashCode = (hashCode * 397) ^ NumCells;
                hashCode = (hashCode * 397) ^ Mass.GetHashCode();
                hashCode = (hashCode * 397) ^ (AirfoilDefinition != null ? AirfoilDefinition.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RiggingAngle.GetHashCode();
                hashCode = (hashCode * 397) ^ (PlanformAreaEllipse != null ? PlanformAreaEllipse.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ PressureMultiplier.GetHashCode();
                hashCode = (hashCode * 397) ^ RigAttachPos.GetHashCode();
                hashCode = (hashCode * 397) ^ NumToggleControlledCells;
                hashCode = (hashCode * 397) ^ RearRiserPullMagnitude.GetHashCode();
                hashCode = (hashCode * 397) ^ FrontRiserPullMagnitude.GetHashCode();
                hashCode = (hashCode * 397) ^ WeightshiftMagnitude.GetHashCode();
                hashCode = (hashCode * 397) ^ PilotWeight.GetHashCode();
                hashCode = (hashCode * 397) ^ InputGamma.GetHashCode();
                hashCode = (hashCode * 397) ^ InputSmoothing.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ParachuteConfig left, ParachuteConfig right) {
            return Equals(left, right);
        }

        public static bool operator !=(ParachuteConfig left, ParachuteConfig right) {
            return !Equals(left, right);
        }
    }

}