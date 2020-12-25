using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.BubblingEventSystem;

namespace RamjetAnvil.Volo.UIEvents {

    public interface ICursorHoverListener : IBubblingEventListener<CursorEnterEvent>, IBubblingEventListener<CursorLeaveEvent> { }
    public interface ICursorClickListener : IBubblingEventListener<CursorClickEvent> { }

    public struct CursorEnterEvent { }
    public struct CursorLeaveEvent { }

    public struct CursorClickEvent { }
}
