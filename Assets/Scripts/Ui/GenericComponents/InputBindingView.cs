using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {

    [RequireComponent(typeof(InputBindingViewData))]
    public class InputBindingView : Button {

        public new event Action OnSubmit;

        private InputBindingViewData _data;

        private MutableString _bindingTitle;

        protected override void Awake() {
            base.Awake();
            onClick.AddListener(Submit);
            _data = GetComponent<InputBindingViewData>();
            _bindingTitle = new MutableString(64);
        }

        public void SetState(InputBindingViewModel inputBinding, bool isRebinding, string rebindingText) {
            _bindingTitle.Clear()
                .Append(inputBinding.Group)
                .Append(" - ")
                .Append(inputBinding.Name)
                .Append(" <i>(")
                .Append(inputBinding.BindingType)
                .Append(")</i>");
            _data.TitleWrapper.SetActive(!isRebinding);
            _data.Title.SetMutableString(_bindingTitle);

            if (isRebinding) {
                _data.Binding.text = rebindingText;
                _data.Binding.fontStyle = FontStyle.Italic;
            } else {
                _data.Binding.text = inputBinding.Binding;
                _data.Binding.fontStyle = FontStyle.Bold;
            }
        }

        protected void Submit() {
            if (OnSubmit != null) {
                OnSubmit();
            }
        }
    }
}
