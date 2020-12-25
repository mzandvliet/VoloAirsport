using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Gui;
using UnityEngine;

public class BillboardScaler : MonoBehaviour {

    [SerializeField] private float _scaleSpeed = 10f;

    [Dependency("cameraTransform")][SerializeField] private Transform _cameraTransform;
    [Dependency][SerializeField] private AbstractUnityClock _clock;

    [SerializeField] private Vector3 _targetScale = Vector3.one;

    private Vector3 _initialScale;

    void Awake() {
        _initialScale = transform.localScale;
    }

    void Update() {
        var targetScale = RelativeTargetScale(_targetScale);
        var currentScale = Vector3.Lerp(transform.localScale, targetScale, _scaleSpeed * _clock.DeltaTime);
        RenderScale(currentScale);
    }

    private Vector3 RelativeTargetScale(Vector3 scale) {
        var distanceScale = _cameraTransform != null 
            ? GuiPlacement.RelativeScale(_cameraTransform, transform.parent.position)
            : 1f;
        var targetScale = scale * distanceScale;
        targetScale = Vector3.Scale(targetScale, _initialScale);
        return targetScale;
    }

    public void ForceScale(Vector3 targetScale) {
        TargetScale = targetScale;
        if (transform != null) {
            RenderScale(targetScale);
        }
    }

    private void RenderScale(Vector3 scale) {
        transform.localScale = Vector3.Max(scale, Vector3.zero);
    }

    public Vector3 TargetScale {
        set { _targetScale = value; }
    }
}
