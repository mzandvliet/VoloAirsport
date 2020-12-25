using UnityEngine;

namespace RamjetAnvil.Volo {

    [System.Serializable, CreateAssetMenu]
    public class AirfoilDefinition : ScriptableObject {
        [SerializeField] AirfoilProfile[] _profiles;

        public AirfoilProfile[] Profiles {
            get { return _profiles; }
        }

        public Coefficients GetInterpolated(float angle, float[] weights) {
            //Debug.Assert(weights.Length == _profiles.Length, "Incorrect number of weights specified, expected: " + _profiles.Length);

            Coefficients result = new Coefficients();
            float weightSum = 0f;
            for (int i = 0; i < _profiles.Length; i++) {
                result.Lift += _profiles[i].Lift.Evaluate(angle) * weights[i];
                result.Drag += _profiles[i].Drag.Evaluate(angle) * weights[i];
                result.Drag += _profiles[i].Moment.Evaluate(angle) * weights[i];
                weightSum += weights[i];
            }

            result.Lift /= weightSum;
            result.Drag /= weightSum;
            result.Moment /= weightSum;

            return result;
        }
    }

    [System.Serializable]
    public class AirfoilProfile {
        [SerializeField] private string _name;
        [SerializeField] private AnimationCurve _lift;
        [SerializeField] private AnimationCurve _drag;
        [SerializeField] private AnimationCurve _moment;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public AnimationCurve Lift {
            get { return _lift; }
            set { _lift = value; }
        }

        public AnimationCurve Drag {
            get { return _drag; }
            set { _drag = value; }
        }

        public AnimationCurve Moment {
            get { return _moment; }
            set { _moment = value; }
        }

        public Coefficients Get(float angle) {
            return new Coefficients(
                _lift.Evaluate(angle),
                _drag.Evaluate(angle),
                _moment.Evaluate(angle));
        }
    }

    [System.Serializable]
    public struct Coefficients {
        [SerializeField] public float Lift;
        [SerializeField] public float Drag;
        [SerializeField] public float Moment;

        public Coefficients(float lift, float drag, float moment) {
            Lift = lift;
            Drag = drag;
            Moment = moment;
        }
    }
}