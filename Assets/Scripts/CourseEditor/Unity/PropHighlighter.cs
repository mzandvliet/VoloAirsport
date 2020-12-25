using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Volo.CourseEditing;
using UnityEngine;

public class PropHighlighter : MonoBehaviour, IHighlightable {
    [SerializeField] private GameObject _subject;
    [SerializeField] private Color _colorHighlighted;

    private Color _colorUnhighlighted;

    void Awake() {
        _colorUnhighlighted = _subject.GetComponent<Renderer>().material.color;
    }

    public void Highlight() {
        _subject.GetComponent<Renderer>().material.color = _colorHighlighted;
    }

    public void UnHighlight() {
        _subject.GetComponent<Renderer>().material.color = _colorUnhighlighted;
    }
}
