using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Gui;
using UnityEngine;

namespace RamjetAnvil.Util
{
    public class WorldCanvasScaler : MonoBehaviour {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private float _planeDistance;

        void Update() {
            _canvas.planeDistance = _planeDistance;
        }
    }
}
