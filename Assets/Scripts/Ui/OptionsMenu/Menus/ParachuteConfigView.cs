using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using Assets.Scripts.OptionsMenu;
using RamjetAnvil.DependencyInjection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {

    public class ParachuteConfigView : MonoBehaviour {

        [SerializeField] private SettingsContainer _settingsContainer;

//        private ParachuteConfigViewModel _model;
        private IList<Selectable> _selectables;

        public void Initialize(ParachuteConfigViewModel model) {
            _settingsContainer.Initialize(GuiComponentDescriptor.FindDescriptors(model));

            _selectables = _settingsContainer.GuiComponents
                .Select(g => g.NavigationElement)
                .ToList();
        }

        public void Open(Action onClose) {
            this.gameObject.SetActive(true);
            UiNavigation.ResolveExplicitNavigation(_selectables);
        }

        public void Close() {
            this.gameObject.SetActive(false);
        }

        public void SetState(Func<string, string> l) {
            _settingsContainer.SetState(l);

            UiNavigation.ResolveExplicitNavigation(_selectables);
        }
    }
}

