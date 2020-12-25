using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.OptionsMenu;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {
    public class SettingsContainer : MonoBehaviour {

        [SerializeField] private GuiComponentPrefabs _guiComponentPrefabs;
        [SerializeField] private SettingsView _content;

        private GuiComponentDescriptor[] _componentDescriptors;
        private IComponentRenderer[] _guiComponents; 

        public void Initialize(GuiComponentDescriptor[] componentDescriptors) {
            _componentDescriptors = componentDescriptors;
            _guiComponents = new IComponentRenderer[_componentDescriptors.Length];
            for (int i = 0; i < _componentDescriptors.Length; i++) {
                var descriptor = _componentDescriptors[i];
                _guiComponents[i] = _guiComponentPrefabs.CreateGuiComponent(descriptor, _content);
            }
        }

        public void SetState(Func<string, string> l) {
            for (int i = 0; i < _guiComponents.Length; i++) {
                _guiComponents[i].Update(l);
            }
        }

        public IComponentRenderer[] GuiComponents {
            get { return _guiComponents; }
        }
    }
}
