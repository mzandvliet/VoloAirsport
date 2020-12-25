using UnityEngine;

public class RingAnimator : MonoBehaviour {
    [SerializeField] private Ring _ring;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Light _light;
    [SerializeField] private float _flashFreq = 1f;
    [SerializeField] private float _flashIntensity = 2f;
    [SerializeField] private float _minPortalIntensity = 1f;
    [SerializeField] private float _maxPortalIntensity = 4f;
    [SerializeField] private float _minLightIntensity = 0.25f;
    [SerializeField] private float _maxLightIntensity = 1f;

	void Update () {
	    float flashLerp = Mathf.Pow(Mathf.Sin(Time.time*Mathf.PI*_flashFreq) * 0.5f + 0.5f, _flashIntensity);

        /* Todo: Do mesh scaling in vertex shader, or change the mesh to something else and just rotate it. */

        Color c = Color.white;
	    c.a = flashLerp;

        _renderer.material.SetColor("_TintColor", c);
        _renderer.material.SetFloat("_EmissiveIntensity", Mathf.Lerp(_minPortalIntensity, _maxPortalIntensity, flashLerp));
        _light.intensity = Mathf.Lerp(_minLightIntensity, _maxLightIntensity, flashLerp);
	}
}
