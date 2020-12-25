using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Volo.Ui;
using RamjetAnvil.Volo.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RamjetAnvil.Volo {
    public class ParachuteThumbnail : MonoBehaviour {
        [SerializeField] private CustomizableButton _selectButton;
        private ColorBlock _normalSelectionColors;
        private ColorBlock _normalEditColors;
        [SerializeField] private GameObject _controlButtonsParent;
        [SerializeField] private Button _editButton;
        [SerializeField] private Button _cloneButton;
        [SerializeField] private Button _deleteButton;

        public event Action<string> OnSelect;
        public event Action<string> OnHighlight; 
        public event Action<string> OnEdit; 
        public event Action<string> OnClone;
        public event Action<string> OnDelete;

        private string _parachuteId;

        void Awake() {
            _normalSelectionColors = _selectButton.Button.colors;
            _normalEditColors = _editButton.colors;

            _selectButton.Button.onClick.AddListener(() => {
                if (OnSelect != null) {
                    OnSelect(_parachuteId);
                }
            });
            _editButton.onClick.AddListener(() => {
                if (OnEdit != null) {
                    OnEdit(_parachuteId);
                }
            });
            _deleteButton.onClick.AddListener(() => {
                if (OnDelete != null) {
                    OnDelete(_parachuteId);
                }
            });
            _cloneButton.onClick.AddListener(() => {
                if (OnClone != null) {
                    OnClone(_parachuteId);
                }
            });
        }

        public void SetState(ParachuteControlState parachute) {
            _parachuteId = parachute.Id;
            // TODO Change selection color scheme
            _selectButton.ButtonText.text = string.IsNullOrEmpty(parachute.Name) ? "<i>Untitled</i>" : parachute.Name.Limit(15);
            var selectionColor = _normalSelectionColors;
            if (parachute.IsSelected) {
                selectionColor.normalColor = selectionColor.pressedColor;
            }
            _selectButton.Button.colors = selectionColor;

            _controlButtonsParent.SetActive(parachute.IsSelected && parachute.IsEditorAvailable);

            _editButton.interactable = parachute.IsEditable;
            var editButtonColors = _normalEditColors;
            if (parachute.IsEditing) {
                editButtonColors.normalColor = editButtonColors.pressedColor;
            }
            _editButton.colors = editButtonColors;

            _cloneButton.interactable = true;
            _deleteButton.interactable = parachute.IsDeletable;
        }

        public struct ParachuteControlState {
            public readonly string Id;
            public readonly string Name;
            public readonly bool IsEditorAvailable;
            public readonly bool IsSelected;
            public readonly bool IsEditable;
            public readonly bool IsDeletable;
            public readonly bool IsEditing;

            public ParachuteControlState(string id, string name, bool isEditorAvailable, bool isSelected, bool isEditable, bool isDeletable, bool isEditing) {
                Id = id;
                Name = name;
                IsSelected = isSelected;
                IsEditable = isEditable;
                IsDeletable = isDeletable;
                IsEditing = isEditing;
                IsEditorAvailable = isEditorAvailable;
            }
        }
    }
}
