using System;
using RamjetAnvil.DependencyInjection;
using UnityEngine;

namespace RamjetAnvil.Util {
    public class CameraCanvasInjector : MonoBehaviour {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Camera _camera;

        [Dependency("guiCamera")]
        public Camera Camera {
            get { return _camera; }
            set {
                _camera = value;
                _canvas.worldCamera = value;
            }
        }
    }
}
