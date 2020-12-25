using UnityEngine;

public class JointControllerData : MonoBehaviour {
    [SerializeField] private float _dampeningStrength = 1f;
    [SerializeField] private float _correctionStrength = 1f;
    [SerializeField] private float _errorPower = 2f;

    public float DampeningStrength
    {
        get { return _dampeningStrength; }
        set { _dampeningStrength = value; }
    }

    public float CorrectionStrength {
        get { return _correctionStrength; }
        set { _correctionStrength = value; }
    }

    public float ErrorPower {
        get { return _errorPower; }
        set { _errorPower = value; }
    }
}
