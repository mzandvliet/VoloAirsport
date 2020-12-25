using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Volo.CourseEditing;
using UnityEngine;

public class SimpleSelectable : MonoBehaviour, ISelectable {

    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _selectedColor;

    void Awake() {
        UnSelect();
    }

    public void Select() {
        GetComponent<Renderer>().material.color = _selectedColor;
    }

    public void UnSelect() {
        GetComponent<Renderer>().material.color = _normalColor;
    }
}
