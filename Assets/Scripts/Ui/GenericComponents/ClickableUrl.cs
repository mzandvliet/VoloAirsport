using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {

    public class ClickableUrl : MonoBehaviour {
        [SerializeField] private Text _text;
        [SerializeField] private Button _button;
        [SerializeField] private string _url;

        void Awake() {
            _button.onClick.AddListener(() => Application.OpenURL(_url));
        }

        public void SetUrl(string name, string url) {
            _url = url;
            _text.text = name;
        }

        public string Url {
            get { return _url; }
            set { _url = value; }
        }
    }
}
