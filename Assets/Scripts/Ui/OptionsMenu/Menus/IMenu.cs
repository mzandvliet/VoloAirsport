using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Volo.Ui {
    public interface IMenu {
        void Initialize(OptionsMenuModel model);
        void SetState(OptionsMenuModel model, Func<string, string> l);
        Menu Id { get; }
    }
}
