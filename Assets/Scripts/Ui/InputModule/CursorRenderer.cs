using RamjetAnvil.DependencyInjection;
using RamjetAnvil.InputModule;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {
    public class CursorRenderer : MonoBehaviour {

        [SerializeField, Dependency] private ICursor _cursor;
        [SerializeField] private Transform _renderable;

        void Update() {
            _renderable.transform.position = _cursor.Poll().Ray.GetPoint(1f);
        }
    }
}
