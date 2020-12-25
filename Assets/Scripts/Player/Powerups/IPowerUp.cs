using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Volo {
    public interface IPowerUp {
        bool IsActive { get; set; }
        float Amount { get; }
    }
}
