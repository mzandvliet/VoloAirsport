using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {
    public abstract class AbstractMenu : MonoBehaviour, IMenu {
        public abstract void Initialize(OptionsMenuModel model);
        public abstract void SetState(OptionsMenuModel model, Func<string, string> l);
        public abstract Menu Id { get; }
    }
}
