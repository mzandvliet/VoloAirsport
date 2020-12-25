using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.InputModule {
    public interface IHighlightable {
        event Action OnHighlight;
        event Action OnUnHighlight;
    }
}
