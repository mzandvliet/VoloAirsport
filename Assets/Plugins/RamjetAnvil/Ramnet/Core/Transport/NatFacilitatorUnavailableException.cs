using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.RamNet {
    public class NatFacilitatorUnavailableException : Exception {
        public static readonly NatFacilitatorUnavailableException Default = new NatFacilitatorUnavailableException();
        private NatFacilitatorUnavailableException() : base("NAT facilitator is unavailable") {}
    }
}
