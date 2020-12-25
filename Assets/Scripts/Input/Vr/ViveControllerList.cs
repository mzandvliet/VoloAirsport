using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Valve.VR;

namespace RamjetAnvil.Volo.Input {
    public class ViveControllerList : MonoBehaviour {

        private List<int> _controllerIndices;

        void Awake() {
            _controllerIndices = new List<int>();
        }

        private void OnDeviceConnected(int index, bool connected) {
            var system = OpenVR.System;
            if (system == null || system.GetTrackedDeviceClass((uint)index) != ETrackedDeviceClass.Controller) {
                return;
            }

            if (connected) {
                Debug.Log("Controller connected: " + index);
                _controllerIndices.Add(index);
            }
            else {
                Debug.Log("Controller disconnected: " + index);
                _controllerIndices.Remove(index);
            }
        }

        private void OnEnable() {
            SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);
        }

        private void OnDisable() {
            SteamVR_Events.DeviceConnected.Remove(OnDeviceConnected);
        }

        public List<int> ControllerIndices {
            get { return _controllerIndices; }
        }
    }
}
