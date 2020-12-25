//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RamjetAnvil.Unity.Utility;
//using UnityEngine;
//
//namespace RamjetAnvil.Volo.Ui {
//    public class OptionsMenuTest : MonoBehaviour {
//
//        [SerializeField] private List<AbstractMenu> _availableMenus;
// 
//        private IDictionary<Menu, IMenu> _menus;
//
//        void Start() {
//            var settings = new GameSettings {
//                Graphics = new GameSettings.GraphicsSettings {
//                    DetailObjectDensity = 1,
//                    DetailObjectDistance = 1,
//                },
//                Audio = new GameSettings.AudioSettings {
//                    IsMuted = true,
//                    MusicVolume = 50f,
//                    SoundEffectsVolume = 50f
//                },
//                Other = new GameSettings.OtherSettings {
//                    Language = "en-US"
//                }
//            };
//            var model = new OptionsMenuModel(settings);
//            model.Updated += ModelOnUpdated;
//
//            _menus = new Dictionary<Menu, IMenu>();
//            foreach (var menu in _availableMenus) {
//                _menus[menu.Id] = menu;
//                menu.Initialize(model);
//            }
//            model.Update(settings);
//        }
//
//        private void ModelOnUpdated(OptionsMenuModel model) {
//            var activeMenu = _menus[model.ActiveMenu];
//            activeMenu.SetState(model, model.LanguageTable.Get);
//
//            for (int i = 0; i < _availableMenus.Count; i++) {
//                var menu = _availableMenus[i];
//                menu.gameObject.SetActive(model.ActiveMenu == menu.Id);
//            }
//        }
//    }
//}
