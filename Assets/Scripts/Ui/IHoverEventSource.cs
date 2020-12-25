using System;

namespace RamjetAnvil.Volo.Ui {
    public interface IHoverEventSource {
        event Action OnCursorEnter;
        event Action OnCursorExit;
    }
}
