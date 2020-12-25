using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Impero;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Volo.Input;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class RecenterNotification : MonoBehaviour {
        [SerializeField] private ActiveLanguage _activeLanguage;
        [Dependency, SerializeField] private MenuActionMapProvider _menuActionMap;
        [SerializeField] private JoystickActivator _joystickActivator;

        [SerializeField] private GameObject _notificationRenderer;
        [SerializeField] private Text _notificationText;

        private string _vrRecenterPhrase;
        private string _controllerDetectionPhrase;

        private bool _isControllerDetected;

        void Awake() {
            if (_activeLanguage != null) {
                ActiveLanguage = _activeLanguage;
            }
            if (_joystickActivator != null) {
                JoystickActivator = _joystickActivator;
            }
        }

        void Update() {
            if (!_isControllerDetected) {
                _notificationText.text = _controllerDetectionPhrase;
            } else {
                _notificationText.text = _vrRecenterPhrase;
                if (_menuActionMap != null && 
                    _menuActionMap.ActionMap.V.PollButtonEvent(MenuAction.RecenterVrHeadset) == ButtonEvent.Down) {
                    _notificationRenderer.SetActive(false);
                    enabled = false;
                }
            }
        }

        [Dependency]
        public ActiveLanguage ActiveLanguage {
            get { return _activeLanguage; }
            set {
                _activeLanguage = value;
                _activeLanguage.TableUpdates.Subscribe(languageTable => {
                    _vrRecenterPhrase = languageTable.Table["vr_press_to_recenter"]
                        .Replace("$n", "DPad Up");;
                    _controllerDetectionPhrase = languageTable.Table["press_any_button_to_start"];
                });
            }
        }

        [Dependency]
        public JoystickActivator JoystickActivator {
            get { return _joystickActivator; }
            set {
                _joystickActivator = value;
                _joystickActivator.ActiveController
                    .Subscribe(controller => _isControllerDetected = controller.HasValue);
            }
        }
    }
}
