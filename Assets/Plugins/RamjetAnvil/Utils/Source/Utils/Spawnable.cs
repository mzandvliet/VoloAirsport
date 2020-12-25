using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Unity.Utility {

    public interface ISpawnable {
        void OnSpawn();
        void OnDespawn();
    }
}