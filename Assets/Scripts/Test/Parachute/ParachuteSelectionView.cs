using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.InputModule;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.Ui;
using RamjetAnvil.Volo.Util;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo {
    public class ParachuteSelectionView : MonoBehaviour, IUiContext {

        public event Action BackToFlight;

        [SerializeField] private int _maxParachutes = 30;
        [SerializeField] private int _parachuteNameCharLimit = 30;

        [SerializeField] private EditableText _parachuteNameEditor;
        [SerializeField] private ParachuteConfig _parachuteConfigTemplate;
        [SerializeField] private GameObject _thumbnailRoot;
        [SerializeField] private GameObject _parachuteThumbnailPrefab;
        [SerializeField] private Button _addParachute;
        [SerializeField] private Button _openParachuteFolder;
        [SerializeField] private Button _backToFlight;

        private ParachuteThumbnail[] _parachuteThumbnails;

        public void Initialize(ITypedDataCursor<ParachuteStorageViewState> storage, bool isVrActive) {
            _openParachuteFolder.gameObject.SetActive(!isVrActive);
            _addParachute.gameObject.SetActive(!isVrActive);

            _backToFlight.onClick.AddListener(() => {
                if (BackToFlight != null) {
                    BackToFlight();
                }
            });

            var editorState = storage.To(s => s.EditorState);
            var availableChutes = storage.To(s => s.AvailableParachutes);

            var parachuteName = editorState.To(s => s.Config).To(c => c.Name);
            Action<string> updateParachuteName = value => {
                parachuteName.Set(value.Limit(_parachuteNameCharLimit, ""));
            };
            _parachuteNameEditor.Limit = _parachuteNameCharLimit;
            _parachuteNameEditor.TextChanged += updateParachuteName;

            _parachuteThumbnails = new ParachuteThumbnail[_maxParachutes];
            for (int i = 0; i < _maxParachutes; i++) {
                var parachuteThumbnail = GameObject.Instantiate(_parachuteThumbnailPrefab).GetComponent<ParachuteThumbnail>();
                parachuteThumbnail.transform.SetParent(_thumbnailRoot.transform);
                parachuteThumbnail.transform.localPosition = Vector3.zero;
                parachuteThumbnail.transform.localRotation = Quaternion.identity;
                parachuteThumbnail.transform.localScale = Vector3.one;
                parachuteThumbnail.gameObject.SetActive(false);
                _parachuteThumbnails[i] = parachuteThumbnail;
            }

            var storageDir = storage.To(s => s.StorageDir);
            _openParachuteFolder.onClick.AddListener(() => {
                UnityFileBrowserUtil.Open(storageDir.Get());
            });

            Action<string> selectChute = parachuteId => {
                var state = storage.Get();
                if (state.EditorState.Config.Id != parachuteId) {
                    for (int i = 0; i < state.AvailableParachutes.Count; i++) {
                        var parachute = state.AvailableParachutes[i];
                        if (parachute.Id == parachuteId) {
                            editorState.Set(new ParachuteEditor.EditorState(parachute));        
                        }
                    }
                }
            };

            Action<string> deleteChute = parachuteId => {
                var state = storage.Get();
                int selectedParachuteIndex = 0;
                for (int i = state.AvailableParachutes.Count - 1; i >= 0; i--) {
                    var parachute = state.AvailableParachutes[i];
                    if (parachute.Id == parachuteId) {
                        state.AvailableParachutes.RemoveAt(i);
                        selectedParachuteIndex = Math.Max(i - 1, 0);
                        break;
                    }
                }
                if (state.EditorState.Config.Id == parachuteId) {
                    var defaultChute = state.AvailableParachutes[selectedParachuteIndex];
                    editorState.Set(new ParachuteEditor.EditorState(defaultChute));
                }
                availableChutes.Set(state.AvailableParachutes);
            };

            Action<string> cloneChute = parachuteId => {
                var state = storage.Get();
                if (state.AvailableParachutes.Count < _maxParachutes) {
                    ParachuteConfig existingParachute = null;
                    for (int i = 0; i < state.AvailableParachutes.Count; i++) {
                        var parachute = state.AvailableParachutes[i];
                        if (parachute.Id == parachuteId) {
                            existingParachute = parachute;
                        }
                    }

                    var newParachute = ParachuteConfig.CreateNew(existingParachute);
                    newParachute.Name = (existingParachute.Name + " Copy").Limit(_parachuteNameCharLimit, "");
                    state.AvailableParachutes.Add(newParachute);
                    availableChutes.Set(state.AvailableParachutes);
                    selectChute(newParachute.Id);
                }
            };

            var isEditingState = storage.To(s => s.EditorState).To(s => s.IsEditing);

            Action<string> editChute = parachuteId => {
                var state = storage.Get();
                string currentlyEditingParachute = state.EditorState.IsEditing ? state.EditorState.Config.Id : null;
                if (currentlyEditingParachute == parachuteId) {
                    // Switch to read-only mode
                    isEditingState.Set(false);
                } else {
                    // Enable editing
                    // TODO Do selection and edit switching in one transformation
                    selectChute(parachuteId);
                    isEditingState.Set(true);
                }
            };

            _addParachute.onClick.AddListener(() => {
                var state = storage.Get();
                if (state.AvailableParachutes.Count < _maxParachutes) {
                    var newParachute = ParachuteConfig.CreateNew(_parachuteConfigTemplate);
                    newParachute.Name = "New " + _parachuteConfigTemplate.Name;
                    state.AvailableParachutes.Add(newParachute);
                    availableChutes.Set(state.AvailableParachutes);
                    editorState.Set(new ParachuteEditor.EditorState(newParachute));
                }
            });
            
            storage.OnUpdate.Subscribe(state => {
                for (int i = 0; i < _parachuteThumbnails.Length; i++) {
                    var thumbnail = _parachuteThumbnails[i];
                    thumbnail.OnSelect -= selectChute;
                    thumbnail.OnDelete -= deleteChute;
                    thumbnail.OnClone -= cloneChute;
                    thumbnail.OnEdit -= editChute;
                }

                _parachuteNameEditor.IsEditable = state.EditorState.IsEditing;
                _parachuteNameEditor.Text = state.EditorState.Config.Name;

                for (int i = 0; i < _parachuteThumbnails.Length; i++) {
                    var thumbnail = _parachuteThumbnails[i];

                    if (i < state.AvailableParachutes.Count) {
                        var parachute = state.AvailableParachutes[i];
                        var isSelected = state.EditorState.Config.Id == parachute.Id;
                        var parachuteControlState = new ParachuteThumbnail.ParachuteControlState(
                            parachute.Id,
                            parachute.Name,
                            isEditorAvailable: !isVrActive,
                            isSelected: state.EditorState.Config.Id == parachute.Id,
                            isEditable: parachute.IsEditable,
                            isDeletable: state.AvailableParachutes.Count > 1 && parachute.IsEditable,
                            isEditing: state.EditorState.IsEditing && isSelected);
                        thumbnail.SetState(parachuteControlState);
                        if (parachuteControlState.IsDeletable) {
                            thumbnail.OnDelete += deleteChute;
                        }
                        if (parachuteControlState.IsEditable) {
                            thumbnail.OnEdit += editChute;
                        }
                        thumbnail.OnSelect += selectChute;
                        thumbnail.OnClone += cloneChute;
                        thumbnail.gameObject.SetActive(true);

                        if (parachuteControlState.IsSelected) {
                            FirstObject = thumbnail.gameObject;
                        }
                    } else {
                        thumbnail.gameObject.SetActive(false);   
                    }
                }
            });
        }

        private ParachuteConfig FindChute(IList<ParachuteConfig> parachutes, string parachuteId) {
            for (int i = 0; i < parachutes.Count; i++) {
                var parachute = parachutes[i];
                if (parachute.Id == parachuteId) {
                    return parachute;
                }
            }
            return null;
        }

        public GameObject FirstObject { get; private set; }
    }

    public class ParachuteStorageViewState {
        public ParachuteEditor.EditorState EditorState;
        public readonly IList<ParachuteConfig> AvailableParachutes;
        public readonly string StorageDir;

        public ParachuteStorageViewState(ParachuteConfig selectedConfig, IList<ParachuteConfig> availableChutes, string storageDir) {
            StorageDir = storageDir;
            EditorState = new ParachuteEditor.EditorState(selectedConfig);
            AvailableParachutes = availableChutes;
        }
    }
}
