using UnityEngine;

namespace RamjetAnvil.Volo.Util {

    public class LineSet {
        public readonly LineRenderer Renderer;
        public readonly Vector3[] Points;

        public LineSet(LineRenderer renderer, Vector3[] points) {
            Renderer = renderer;
            Points = points;
        }
    }
}
