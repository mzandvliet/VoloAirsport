using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class LocalizedText : MonoBehaviour {
        [SerializeField] private string _key;
        [SerializeField] private Text _text;

        public void UpdateLocalization(Func<string, string> l) {
            _text.text = l(_key);
        }
    }
}
