using System.Collections;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using UnityEngine;

/* Would have used our Scheduler, but there's a bug in it when using the .Dispose call to cancel them. */
public class VoloLogo : MonoBehaviour {

    [Dependency, SerializeField] private CanvasGroup _logoGui;
    [Dependency, SerializeField] private Renderer _particleSystemRenderer;

    private Material _particleMat;
    private bool _isVisible;

    public bool IsVisible {
        get { return _isVisible; }
    }

    void Awake() {
        _particleMat = _particleSystemRenderer.material;
        Opacity = 0f;
    }

    public void Show(bool show) {
        _isVisible = show;

        StopAllCoroutines();

        if (show) {
            gameObject.SetActive(true);
            StartCoroutine(Animate());
        }
        else {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator Animate() {
        

        float lerp = 0f;
        while (lerp < 1.0f) {
            lerp += Time.deltaTime;
            Opacity = lerp;
            yield return new WaitForEndOfFrame();
        }
    }

    private float Opacity {
        set {
            value = Mathf.Clamp01(value);

            _logoGui.alpha = value;

            Color color = _particleMat.GetColor("_Color");
            color.a = value;
            _particleMat.SetColor("_Color", color);
        }
    }
}
