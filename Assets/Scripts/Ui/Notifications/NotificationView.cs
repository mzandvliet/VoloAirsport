using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class NotificationView : MonoBehaviour {

        [SerializeField] private Text _textField;

        public void SetText(string s) {
            _textField.SetMutableString(s);
        }
    }
}
