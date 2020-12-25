using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class LinePullGaugeView : MonoBehaviour {
        [SerializeField] private Image _meter;
        [SerializeField] private LayoutElement _layoutElement;
        [SerializeField] private float _maxHeight = 10f;

        public void SetPullForce(Color lineColor, float pullForce) {
            _meter.color = lineColor;
            _layoutElement.preferredHeight = Mathf.Lerp(0, _maxHeight, pullForce);
        }
    }
}
