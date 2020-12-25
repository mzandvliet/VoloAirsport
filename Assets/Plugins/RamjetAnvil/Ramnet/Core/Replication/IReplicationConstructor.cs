using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Lidgren.Network;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using UnityEngine;
using Whathecode.System;

namespace RamjetAnvil.RamNet {

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class InitialState : Attribute {}

    // TODO Search a component hierachy for initial state attributes
    // TODO Create delegates from these initial state setter methods

    public static class InitialStateLogic {

        private static readonly List<MonoBehaviour> ComponentCache = new List<MonoBehaviour>(); 

        public static IList<InitialStateHandler> FindInitialStateConstructors(GameObject gameObject, 
            IReadOnlyDictionary<Type, MessageType> messageTypes) {

            ComponentCache.Clear();
            gameObject.GetComponentsInChildren(ComponentCache);

            var initialStateConstructors = new List<InitialStateHandler>();

            for (int componentIndex = 0; componentIndex < ComponentCache.Count; componentIndex++) {
                var component = ComponentCache[componentIndex];
                var componentType = component.GetType();
                var callSites = GetInitialStateCallSites(componentType);
                for (int i = 0; i < callSites.Count; i++) {
                    var callSite = callSites[i];
                    // TODO Verify method signature
                    var handlerType = callSite.GetParameters()[0].ParameterType;
                    var messageType = messageTypes[handlerType];
                    var initialStateHandler = DelegateHelper.CreateDelegate<Action<IObjectMessage>>(callSite, component);
                    initialStateConstructors.Add(new InitialStateHandler(messageType, initialStateHandler));
                }
            }

            return initialStateConstructors;
        } 

        public static readonly Func<Type, IList<MethodInfo>> GetInitialStateCallSites = Memoization
            .Memoize<Type, IList<MethodInfo>>(
                componentType => {
                    var callSites = new List<MethodInfo>();
                    var methods = componentType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    for (int methodIndex = 0; methodIndex < methods.Length; methodIndex++) {
                        var method = methods[methodIndex];
                        var attributes = method.GetCustomAttributes(typeof (InitialState), inherit: true);
                        if (attributes.Length > 0) {
                            callSites.Add(method);
                        }
                    }
                    return callSites;
                });

        public class InitialStateHandler {
            public readonly MessageType MessageType;
            public readonly Action<IObjectMessage> Invoke;

            public InitialStateHandler(MessageType messageType, Action<IObjectMessage> invoke) {
                MessageType = messageType;
                Invoke = invoke;
            }
        }
    }

    public interface IReplicationConstructor {
        void SerializeInitialState(NetBuffer writer);
    }

    public class ReplicationConstructor  : IReplicationConstructor {

        private readonly IReadOnlyDictionary<MessageType, IObjectMessage> _objectMessageCache; 
        private readonly IList<InitialStateLogic.InitialStateHandler> _constructors;

        public ReplicationConstructor(IList<InitialStateLogic.InitialStateHandler> constructors, 
            IReadOnlyDictionary<MessageType, IObjectMessage> objectMessageCache) {

            _constructors = constructors;
            _objectMessageCache = objectMessageCache;
        }

        public void SerializeInitialState(NetBuffer writer) {
            for (int i = 0; i < _constructors.Count; i++) {
                var constructor = _constructors[i];
                // Fill message
                var message = _objectMessageCache[constructor.MessageType];
                constructor.Invoke(message);

                // Serialize it
                writer.Write(constructor.MessageType);
                message.Serialize(writer);
            }
        }
    }
}
