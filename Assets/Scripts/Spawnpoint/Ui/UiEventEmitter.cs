using RamjetAnvil.BubblingEventSystem;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.InputModule;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Ui;
using RamjetAnvil.Volo.UIEvents;
using UnityEngine;
using UnityEngine.EventSystems;

public class UiEventEmitter : MonoBehaviour {

    [Dependency] private ICursor _cursor;
    [SerializeField] private string[] _layers;

    private int _layerMask;
    private GameObject _currentHoverObject;

    public ICursor Cursor {
        get { return _cursor; }
        set { _cursor = value; }
    }

    void Awake() {
        _layerMask = LayerMaskUtil.CreateLayerMask(_layers);
    }

    void Update() {
        if (_cursor != null) {
            RaycastHit raycastHit;
            GameObject newHoverObject = null;
            var cursorInput = _cursor.Poll();
            if (Physics.Raycast(cursorInput.Ray, out raycastHit, maxDistance: Mathf.Infinity, layerMask: _layerMask)) {
                newHoverObject = raycastHit.collider.gameObject;
            }
            UpdateState(newHoverObject);

            if (_currentHoverObject != null && cursorInput.SubmitEvent == PointerEventData.FramePressState.Pressed) {
                this.SendBubblingEventTo(_currentHoverObject, new CursorClickEvent());
            }
        }
    }

    private void UpdateState(GameObject newHoverObject) {
        var isPointerExit = _currentHoverObject != null && _currentHoverObject != newHoverObject;
        if (isPointerExit) {
            this.SendEventTo(_currentHoverObject, new CursorLeaveEvent());
        }

        var isPointerEnter = newHoverObject != null && _currentHoverObject != newHoverObject;
        if (isPointerEnter) {
            this.SendEventTo(newHoverObject, new CursorEnterEvent());
        }

        _currentHoverObject = newHoverObject;
    }
}
