using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.RamNet;

namespace RamjetAnvil.RamNet {

    // TODO What happens if we switch role but the behavior is still enabled?
    public interface INetworkBehavior {
        ObjectRole Role { get; }
        void OnRoleEnabled(ObjectRole role);
        void OnRoleDisabled(ObjectRole role);
    }

    public static class NetworkBehavior {

        /// <summary>
        /// Check whether a role is suited for another.
        /// 
        /// The ObjectRole.Others is a bit different. If the role
        /// explicity states that it only supports the Others role then the other role
        /// doesn't suit the Authority and Owner role unless it is explicity added like:
        /// ObjectRole.Others | ObjectRole.Authority | ObjectRole.Owner.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool Suits(this ObjectRole role, ObjectRole other) {
            if ((role & ObjectRole.Others) != 0) {
                return
                    ((other & ObjectRole.Owner) == 0 || (role & ObjectRole.Owner) != 0) &&
                    ((other & ObjectRole.Authority) == 0 || (role & ObjectRole.Authority) != 0) &&
                    other != ObjectRole.Nobody;
            } 
            return (role & other) != 0;
        }
    }
}
