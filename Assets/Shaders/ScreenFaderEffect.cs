using UnityEngine;
using UnityStandardAssets.ImageEffects;


[AddComponentMenu("Image Effects/Custom/Screenfader Post Effect")]
public class ScreenFaderEffect : ImageEffectBase, IScreenFader {

    [SerializeField] private float _opacity;
    [SerializeField] private Color _fadeColor = Color.black;

    void Update() {
        material.SetColor("_FadeColor", _fadeColor);
        material.SetFloat("_Opacity", _opacity);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(source, destination, material);
    }

    public float Opacity {
        get { return _opacity; }
        set {
            _opacity = value;
            bool shouldBeEnabled = _opacity > 0f;
            if (enabled != shouldBeEnabled)
            enabled = shouldBeEnabled;
        }
    }
}
