using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui
{
    public class TooltipView : MonoBehaviour {
        [SerializeField] private Text _textArea;

        public void SetState(string name, string description) {
            if (!string.IsNullOrEmpty(description)) {
                _textArea.text = "<b>" + name + ":</b>\n" + description + "";
            } else {
                _textArea.text = "<b>" + name + "</b>";
            }
        }
    }
}
