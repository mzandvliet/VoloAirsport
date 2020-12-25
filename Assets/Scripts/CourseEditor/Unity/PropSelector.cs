using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Volo.CourseEditing;
using UnityEngine;

public class PropSelector : MonoBehaviour, ISelectable
{
    [SerializeField] private GameObject _subject;
    [SerializeField] private Color _colorSelected;

    private Color _colorUnselected;

    void Awake() {
        _colorUnselected = _subject.GetComponent<Renderer>().material.color;
    }

    public void Select() {
        _subject.GetComponent<Renderer>().material.color = _colorSelected;
    }

    public void UnSelect() {
        _subject.GetComponent<Renderer>().material.color = _colorUnselected;
    }
}
