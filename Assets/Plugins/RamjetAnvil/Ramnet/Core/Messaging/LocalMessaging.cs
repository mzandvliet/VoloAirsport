using System;
using System.Collections.Generic;
using System.Linq;
using RamjetAnvil.DependencyInjection;
using UnityEngine;

namespace RamjetAnvil.RamNet {

    public class LocalMessaging : MonoBehaviour {

        private static readonly Lazy<object[]> ObjectMessagePools = new Lazy<object[]>(() => {
            return MessageTypes.CreateMessagePool(MessageTypes.ObjectMessageTypes).Pools.ToArray();
        });

        void Awake() {
            // TODO Re-use DependencyContainer
            var diContainer = new DependencyContainer();
            diContainer.AddDependency("objectMessagePools", ObjectMessagePools.Value);
            var objectMessageRouter = new ObjectMessageRouter(MessageTypes.ObjectMessageTypes, gameObject);
            diContainer.AddDependency("localObjectMessageSender", new LocalObjectMessageSender(objectMessageRouter));
            DependencyInjector.Default.Inject(this.gameObject, diContainer);
            (ObjectRole.Authority | ObjectRole.Owner).ApplyTo(this.gameObject);
        }
    }
}
