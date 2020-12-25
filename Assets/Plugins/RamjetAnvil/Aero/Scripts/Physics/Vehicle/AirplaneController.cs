using UnityEngine;
using UnityEngine.SceneManagement;

public class AirplaneController : MonoBehaviour
{
	[SerializeField] ControlSurface _aileronLeftBottom;
    [SerializeField] ControlSurface _aileronLeftTop;
	[SerializeField] ControlSurface _aileronRightBottom;
    [SerializeField] ControlSurface _aileronRightTop;
	[SerializeField] ControlSurface _elevator;
	[SerializeField] ControlSurface _rudder;
	[SerializeField] JetEngine _engine;
    [SerializeField] private Suspension[] _brakes;
    [SerializeField] private ControlSurface _frontWheel;
	
	[SerializeField]
	Vector3 _centerOfMass;
	
	public JetEngine Engine
	{
		get { return _engine; }	
	}
	
	void Start()
	{
		GetComponent<Rigidbody>().centerOfMass = _centerOfMass;	
	}
	
	void Update()
	{
	    float thrustInput = Input.GetButton("Thrust") ? 1f : 0f;

	    float inputPitch = Input.GetAxis("Pitch");
        float inputRoll = Input.GetAxis("Roll");
	    float inputYaw = Input.GetAxis("Yaw");

        // Normally when you hold the stick at 45deg angle, input for both axes reads .72, we need
        // that to read 1, otherwise the player will not be able to pan full range effectively.
        // For this we convert the input to polar coordinates and work from there.
        // Todo: This flips roll sign
        //float stickInputAngle = Mathf.Atan2(inputPitch, inputRoll);
        //float stickInputMagnitude = Mathf.Clamp(Mathf.Sqrt(inputPitch * inputPitch + inputRoll * inputRoll), 0f, 1f);

        //float factorY = ((Mathf.Abs(stickInputAngle) - Mathx.HalfPi) / Mathx.HalfPi) * 2f;
        //float factorX = Mathf.Sign(inputPitch) * (2f - Mathf.Abs(factorY));
        //factorX = Mathf.Clamp(factorX, -1f, 1f);
        //factorY = Mathf.Clamp(factorY, -1f, 1f);

        //inputPitch = factorX * stickInputMagnitude;
        //inputRoll = -factorY * stickInputMagnitude;

	    inputPitch = ScaleQuadratically(inputPitch, 1.5f);
        inputRoll = ScaleQuadratically(inputRoll, 1.5f);
	    inputYaw = ScaleQuadratically(inputYaw, 1.5f);

        _elevator.Input = -inputPitch;
        _aileronLeftBottom.Input = -inputRoll;
        _aileronLeftTop.Input = -inputRoll;
        _aileronRightBottom.Input = inputRoll;
        _aileronRightTop.Input = inputRoll;
        _rudder.Input = inputYaw;

        _engine.ThrustInput = thrustInput;

	    if (Input.GetButtonDown("Respawn")) {
	        SceneManager.LoadScene(0);
	    }

	    float brakeInput = Input.GetButton("Brake") ? 1f : 0f;
        foreach (Suspension suspension in _brakes)
        {
            suspension.Brake(brakeInput);
        }

	    _frontWheel.Input = -inputYaw;
	}

    private float ScaleQuadratically(float input, float power)
    {
        return Mathf.Sign(input) * Mathf.Pow(Mathf.Abs(input), power);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(transform.TransformPoint(_centerOfMass), 0.33f);
    }
}
