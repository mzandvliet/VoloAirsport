//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RamjetAnvil.Unity.Utility;
//using UnityEngine;
//
//public class TransformStateTest  : MonoBehaviour {
//
//    private float _rotationBla;
//
//    void Update() {
//        if (Input.GetKeyDown(KeyCode.F)) {
//            _rotationBla += 5f;
//            Teleport(ImmutableTransform.Identity
//                .UpdatePosition(_current.position)
//                .Rotate(new Vector3(0f, _rotationBla)));
//        }
//    }
//
//    [SerializeField] private Transform _current;
//    [SerializeField] private Transform _previous;
//
////    void Awake() {
////        _previousState = new ImmutableTransform(_current.position, _current.rotation);
////    }
////
////    void FixedUpdate() {
////        _previousState = new ImmutableTransform(_current.position, _current.rotation);
////    }
//
//    public void Teleport(ImmutableTransform bodyState) {
//        // Update the transform state that is associated with this rigidbody
//        var currentWorldRotation = _current.rotation;
//        var localPreviousPosition = _current.InverseTransformPoint(_previous.position);
//
//        _current.position = bodyState.Position;
//        _current.rotation = bodyState.Rotation;
//
//        var rotationDiff = Quaternion.Inverse(currentWorldRotation) * _current.rotation;
//        _previous.position = _current.TransformPoint(localPreviousPosition);
//        _previous.rotation *= rotationDiff;
//    }
//
//}
