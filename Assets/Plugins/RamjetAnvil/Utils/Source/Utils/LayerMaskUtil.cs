using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility
{
    public static class LayerMaskUtil {

        public static readonly LayerMask EmptyMask = new LayerMask();
        public static readonly LayerMask FullMask = EmptyMask.Invert();

        private static IEnumerable<string> LayerNames(this LayerMask layerMask) {
            return layerMask
                .LayerIndices()
                .Select(l => UnityEngine.LayerMask.LayerToName(l))
                .Where(l => !String.IsNullOrEmpty(l));
        }

        public static IEnumerable<int> LayerIndices(this LayerMask layerMask) {
            var output = new List<int>();
            for (int i = 0; i < 32; ++i) {
                int shifted = 1 << i;
                if ((layerMask.value & shifted) == shifted) {
                    output.Add(i);
                }
            }
            return output;
        } 

        public static bool Contains(this LayerMask layerMask, LayerMask layer) {
            return (layerMask.value & (1 << layer.value)) != 0;
        }

        // TODO Don't understand this one, please provide documentation.
        /*public static bool PartialMatch(int maskA, int maskB)
        {
            return (maskA & maskB) != 0;
        }*/

        public static LayerMask Add(this LayerMask layerMask, string layerName) {
            layerMask.value = layerMask.value | (1 << UnityEngine.LayerMask.NameToLayer(layerName));
            return layerMask;
        }

        public static LayerMask Remove(this LayerMask layerMask, string layerName) {
            layerMask.value = layerMask.value & (layerMask.value & ~(1 << UnityEngine.LayerMask.NameToLayer(layerName)));
            return layerMask;
        }

        public static LayerMask Invert(this LayerMask layerMask) {
            layerMask.value = ~layerMask.value;
            return layerMask;
        }

        public static LayerMask CreateLayerMask(params string[] layers) {
            return CreateLayerMask((IList<string>)layers);
        }

        public static LayerMask CreateLayerMask(IList<string> layers) {
            var layerMask = EmptyMask;
            for (int i = 0; i < layers.Count; i++) {
                var layer = layers[i];
                layerMask = layerMask.Add(layer);
            }
            return layerMask;
        }
    }
}
