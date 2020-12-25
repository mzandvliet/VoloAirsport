using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Volo.CourseEditing;
using UnityEngine;

public class SimpleHighlightable : MonoBehaviour, IHighlightable
{
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _highlightColor;

    void Awake() {
        UnHighlight();
    }

    public void Highlight() {
        GetComponent<Renderer>().material.color = _highlightColor;
    }

    public void UnHighlight() {
        GetComponent<Renderer>().material.color = _normalColor;
    }

}
