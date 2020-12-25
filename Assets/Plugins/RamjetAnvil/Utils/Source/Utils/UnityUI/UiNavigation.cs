using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public static class UiNavigation {

        private static readonly IList<Selectable> ActiveSelectables = new List<Selectable>(); 
        public static void ResolveExplicitNavigation(IList<Selectable> selectables) {
            ActiveSelectables.Clear();
            for (int i = 0; i < selectables.Count; i++) {
                var selectable = selectables[i];
                if (selectable.isActiveAndEnabled && selectable.navigation.mode != Navigation.Mode.None) {
                    ActiveSelectables.Add(selectable);
                } else {
                    selectable.navigation = new Navigation {mode = Navigation.Mode.Explicit};
                }
            }

            for (int i = 0; i < ActiveSelectables.Count; i++) {
                var selectable = ActiveSelectables[i];
                selectable.navigation = new Navigation {
                    mode = Navigation.Mode.Explicit,
                    selectOnDown = ActiveSelectables.GetNext(selectable),
                    selectOnUp = ActiveSelectables.GetPrevious(selectable),
                };
            }
        }
    }
}
