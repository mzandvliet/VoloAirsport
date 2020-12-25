using System;
using RamjetAnvil.Coroutine.Time;
using RamjetAnvil.DependencyInjection;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {
    public class ActiveJoystickNotifier : MonoBehaviour {
        [SerializeField, Dependency] private NotificationList _notificationList;
        [SerializeField, Dependency] private JoystickActivator _joystickActivator;

        private bool _isInitialized;

        void OnEnable() {
            if (!_isInitialized) {
                _joystickActivator.ActiveController.Subscribe(controller => {
                    if (enabled && controller.HasValue) {
                        _notificationList.AddTimedNotification(
                            "'" + controller.Value.Name + "' connected",
                            5.Seconds());   
                    }
                });

                _isInitialized = true;
            }
            
        }
    }
}
