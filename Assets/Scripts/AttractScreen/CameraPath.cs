using System;
using RamjetAnvil.Coroutine;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using UnityEngine;

public class CameraPath : MonoBehaviour, ICameraPath {
    [SerializeField] private AnimationCurve _curve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f),
        new Keyframe(1f, 1f, 0f, -1f));

    [SerializeField] private float _duration = 1f;
    [SerializeField] private Transform _nodeA;
    [SerializeField] private Transform _nodeB;

    public TimeSpan Duration {
        get { return TimeSpan.FromSeconds(_duration); }
    }

    public Routines.Animation Animation {
        get { return _curve.Evaluate; }
    }

    public ImmutableTransform From {
        get { return _nodeA.MakeImmutable(); }
    }

    public ImmutableTransform To {
        get { return _nodeB.MakeImmutable(); }
    }
}