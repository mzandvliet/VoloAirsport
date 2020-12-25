using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Unity.Utility {

    public interface IUpdate {
        void OnUpdate(IClock clock);
    }

    public interface IFixedUpdate {
        void OnFixedUpdate(IClock clock);
    }

    public interface IOnFirstFixedUpdate {
        void OnFirstFixedUpdate(IClock clock);
    }

    public interface ILateUpdate {
        void OnLateUpdate(IClock clock);
    }
}
