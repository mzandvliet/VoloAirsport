using UnityEngine;
using System.Collections;
using RamjetAnvil.DependencyInjection;

public class WindVisualizer : MonoBehaviour
{
    [Dependency, SerializeField] private WindManager _wind;

    [SerializeField] private int _horizontalSteps = 16;
    [SerializeField] private int _verticalSteps = 16;
    [SerializeField] private float _horizontalSize = 128f;
    [SerializeField] private float _verticalSize = 128f;
    [SerializeField] private float _drawScale = 10f;

    private void Awake() {
        _wind = FindObjectOfType<WindManager>();
    }

	void OnDrawGizmos()
	{
	    if (!Application.isPlaying)
	        return;

        float horizontalStepsize = _horizontalSize / _horizontalSteps;
        float verticalStepsize = _verticalSize / _verticalSteps;
        float horizontalHalfSize = _horizontalSize * 0.5f;
        float verticalHalfSize = _verticalSize * 0.5f;

        Vector3 bottomLeft = transform.position - new Vector3(horizontalHalfSize, verticalHalfSize, horizontalHalfSize);

        for (int x = 0; x < _horizontalSteps; x++) {
            float xPos = x * horizontalStepsize;

            for (int y = 0; y < _verticalSteps; y++) {
                float yPos = y * verticalStepsize;

                for (int z = 0; z < _horizontalSteps; z++) {
                    float zPos = z * horizontalStepsize;

                    Vector3 position = bottomLeft + new Vector3(xPos, yPos, zPos);
                    Vector3 windVector = _wind.GetWindVelocity(position) * _drawScale;

                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(position, windVector);
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(position, 0.25f);
                }
            }
        }
	}
}
