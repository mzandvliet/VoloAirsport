using RamjetAnvil.BubblingEventSystem;
using RamjetAnvil.InputModule;
using RamjetAnvil.Volo.UIEvents;
using UnityEngine;
using IHighlightable = RamjetAnvil.Volo.CourseEditing.IHighlightable;

public class HiglightableBillboard : MonoBehaviour, IHighlightable {

    [SerializeField] private Vector3 _higlightScale = new Vector3(1.3f, 1.3f, 1.3f);
    [SerializeField] private Color _highlightColor = Color.red;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private BillboardScaler _billboardScaler;

    private Color _normalColor;
    private bool _isHighlighted;

    void Awake() {
        _isHighlighted = false;
        _normalColor = _renderer.material.color;
    }

    public void Highlight() {
//        _billboardScaler.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(0.8f, 0.66f, 0.5f));
        _billboardScaler.TargetScale = _higlightScale;
        _renderer.material.color = _highlightColor;
        _isHighlighted = true;
    }

    public void UnHighlight() {
        _billboardScaler.TargetScale = Vector3.one;
        _renderer.material.color = _normalColor;
//        _billboardScaler.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(0.66f, 0.66f, 0.66f));
        _isHighlighted = false;
    }

    void OnEnable() {
        if (_isHighlighted) {
            Highlight();
        } else {
            UnHighlight();
        }
    }

    void OnDisable() {
        if (_billboardScaler != null) {
            _billboardScaler.ForceScale(Vector3.zero);    
        }
    }
}
