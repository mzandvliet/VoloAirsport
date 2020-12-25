using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class EntryTitle : MonoBehaviour {

        [SerializeField] private Text _text;

        public void SetText(string text) {
            _text.text = text;
        }
    }
}
