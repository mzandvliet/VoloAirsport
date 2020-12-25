using RamjetAnvil.Unity.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class MeterView : MonoBehaviour {

        [SerializeField] private Text _title;
        [SerializeField] private Text _value;

        public void SetTitle(string title) {
            _title.text = title;
        }

        public void SetValue(MutableString s) {
            _value.SetMutableString(s);
        } 
    }
}
