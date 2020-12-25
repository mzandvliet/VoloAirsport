using RamjetAnvil.DependencyInjection;
using UnityEngine;

namespace RamjetAnvil.Volo {
    public class NewsFlashCameraMount : CameraMount {
        [SerializeField] private CameraMount _mount;

        [Dependency("newsFlashCameraMount")]
        public CameraMount Mount {
            get { return _mount; }
            set {
                _mount = value;
                transform.position = _mount.transform.position;
                transform.rotation = _mount.transform.rotation;
            }
        }
    }
}
