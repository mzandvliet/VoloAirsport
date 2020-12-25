using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.DependencyInjection {

    public interface IInjector {
        void Inject<TDep>(object subject, Injection<TDep> injection, TDep dependencies, bool overrideExisting);
        bool IsTypeSupported(Type t);
    }
}
