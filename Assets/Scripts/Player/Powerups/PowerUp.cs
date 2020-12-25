using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Volo {
    public abstract class PowerUp : MonoBehaviour, IPowerUp {
        public abstract bool IsActive { get; set; }
        public abstract float Amount { get; }
    }
}
