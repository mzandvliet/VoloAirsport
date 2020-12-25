using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using RamjetAnvil.RamNet;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using UnityEngine;
using UnityEngine.Networking;
using Whathecode.System;
using Object = UnityEngine.Object;

namespace RamjetAnvil.RamNet {

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MessageHandler : Attribute {
        public readonly ObjectRole AllowedSenders;

        public MessageHandler(ObjectRole allowedSenders = ObjectRoles.Everyone) {
            AllowedSenders = allowedSenders;
        }
    }

    public struct ObjectMessageMetadata {
        public readonly ObjectRole SenderRole;
        public readonly float Latency;

        public ObjectMessageMetadata(ObjectRole senderRole, float latency) {
            SenderRole = senderRole;
            Latency = latency;
        }
    }

    public struct Sender {
        public readonly ConnectionId ConnectionId;
        public readonly ObjectRole Role;
        public readonly SequenceNumber SequenceNumber;
        public readonly float Latency;

        public Sender(ConnectionId connectionId, ObjectRole role, SequenceNumber sequenceNumber, float latency) {
            ConnectionId = connectionId;
            Role = role;
            SequenceNumber = sequenceNumber;
            Latency = latency;
        }
    }

    public class ObjectMessageRouter {

        // TODO Consider using a global router for all objects
        // to more efficiently store handlers

        private readonly IReadOnlyDictionary<Type, MessageType> _messageTypeIds; 
        private readonly IDictionary<MessageType, IList<ObjectMessageHandler>> _registeredHandlers;
        private readonly IDictionary<MessageType, SequenceNumber> _lastReceivedFrames; 

        public ObjectMessageRouter(IReadOnlyDictionary<Type, MessageType> messageTypeIds, 
            GameObject gameObject) {

            _messageTypeIds = messageTypeIds;
            //Debug.Log("message types " + messageTypeIds.Keys.Aggregate("", (acc, t) => acc + ", " + t));
            _registeredHandlers = new ArrayDictionary<MessageType, IList<ObjectMessageHandler>>(_messageTypeIds.Count);
            _lastReceivedFrames = new ArrayDictionary<MessageType, SequenceNumber>(_messageTypeIds.Count);

            this.RegisterGameObject(gameObject);
        }

        public void Dispatch<TMessage>(Sender sender, INetworkMessage<TMessage> networkMessage) where TMessage : IObjectMessage {

            //Debug.Log("routing message of type " + message.Content.GetType());
            IList<ObjectMessageHandler> handlers;
            if (_registeredHandlers.TryGetValue(networkMessage.MessageType, out handlers)) {
                if (networkMessage.Content is IOrderedObjectMessage) {
                    //Debug.Log("received ordered message " + typeof(TMessage));
                    
                    SequenceNumber lastReceivedFrameId;
                    if (!_lastReceivedFrames.TryGetValue(networkMessage.MessageType, out lastReceivedFrameId) ||
                        sender.SequenceNumber > lastReceivedFrameId) {

                        InvokeHandlers(handlers, ref sender, networkMessage.Content);
                        _lastReceivedFrames[networkMessage.MessageType] = sender.SequenceNumber;
                    } else {
                        Debug.Log("Received a message of type " + networkMessage.Content.GetType() + 
                            " that was older than what we previously got");
                    }
                } else {
                    InvokeHandlers(handlers, ref sender, networkMessage.Content);
                }
            }
        }

        private void InvokeHandlers(IList<ObjectMessageHandler> handlers, ref Sender sender, IObjectMessage message) {
            //Debug.Log("invoking handlers for message " + message.GetType() + " handler count " + handlers.Count + " sender role " + sender.Role);
            for (int i = 0; i < handlers.Count; i++) {
                var handler = handlers[i];
                if ((handler.MetaData.AllowedSenders & sender.Role) != 0) {
                    handler.Invoke(message, new ObjectMessageMetadata(sender.Role, sender.Latency));
                } else {
                    Debug.Log(sender + " is not allowed to send message of type " + message.GetType());
                }
            }
        }

        public ObjectMessageRouter RegisterHandler(Type messageType, ObjectMessageHandler handler) {
            if (!_messageTypeIds.ContainsKey(messageType)) {
                throw new Exception("Message type " + messageType + " is not registered, did you forget to import the message from the assembly?");
            }
            var messageTypeId = _messageTypeIds[messageType];
            IList<ObjectMessageHandler> existingHandlers;
            if (!_registeredHandlers.TryGetValue(messageTypeId, out existingHandlers)) {
                existingHandlers = new List<ObjectMessageHandler>();
                _registeredHandlers[messageTypeId] = existingHandlers;
            }
            existingHandlers.Add(handler);
            return this;
        }

        public void ClearLastReceivedMessages() {
            _lastReceivedFrames.Clear();
        }
    }

    //public delegate void ObjectMessageHandler(IObjectMessage message, MessageMetadata connectionId);

    public class ObjectMessageHandler {
        public readonly MessageHandler MetaData;
        public readonly Action<IObjectMessage, ObjectMessageMetadata> Invoke;

        public ObjectMessageHandler(MessageHandler metaData, Action<IObjectMessage, ObjectMessageMetadata> invoke) {
            MetaData = metaData;
            Invoke = invoke;
        }
    }

    public static class ObjectMessageDispatcherExtensions {

        private static readonly List<MonoBehaviour> ComponentCache = new List<MonoBehaviour>(); 

        public static void RegisterGameObject(
            this ObjectMessageRouter router, 
            GameObject g) {

            ComponentCache.Clear();
            g.GetComponentsInChildren(ComponentCache);

            for (int componentIndex = 0; componentIndex < ComponentCache.Count; componentIndex++) {
                var component = ComponentCache[componentIndex];
                if (component == null) {
                    throw new ArgumentException("One or more components found in passed GameObject are null");
                }
                var componentType = component.GetType();
                var callSites = GetMessageHandlerCallSite(componentType);
                for (int i = 0; i < callSites.Count; i++) {
                    var callSite = callSites[i];

                    if (VerifySignature(callSite.MethodInfo, new[] {typeof (IObjectMessage), typeof (ObjectMessageMetadata)}) ||
                        VerifySignature(callSite.MethodInfo, new[] {typeof (IObjectMessage)})) {

                        var handlerParameters = callSite.MethodInfo.GetParameters();
                        var messageParameter = handlerParameters[0];
                        var handlerType = messageParameter.ParameterType;
                        // TODO Better error handling
                        if (handlerParameters.Length == 1) {
                            var messageHandleCallSite = DelegateHelper.CreateDelegate<Action<IObjectMessage>>(
                                callSite.MethodInfo,
                                component);
                            router.RegisterHandler(handlerType, new ObjectMessageHandler(
                                metaData: callSite.MetaData,
                                invoke: (message, metadata) => {
                                    if (component.enabled) {
                                        messageHandleCallSite(message);
                                    }
                                }));
                        } else if (handlerParameters.Length == 2) {
                            var messageHandleCallSite = DelegateHelper.CreateDelegate<Action<IObjectMessage, ObjectMessageMetadata>>(
                                callSite.MethodInfo,
                                component);
                            router.RegisterHandler(handlerType, new ObjectMessageHandler(
                                metaData: callSite.MetaData,
                                invoke: (message, metadata) => {
                                    if (component.enabled) {
                                        messageHandleCallSite(message, metadata);
                                    }
                                }));
                        }
                    } else {
                        throw new Exception("Message handler has incorrect signature: " +
                                            "use (IObjectMessage, MessageMetadata) or (IObjectMessage) instead");
                    }
                }
            }
        }

        private static bool VerifySignature(MethodInfo method, Type[] requiredParams) {
            var parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++) {
                var parameter = parameters[i];
                if (i >= requiredParams.Length) {
                    return false;
                }
                if (!requiredParams[i].IsAssignableFrom(parameter.ParameterType)) {
                    return false;
                }
            }
            return true;
        }

        public static readonly Func<Type, IList<MessageHandlerReference>> GetMessageHandlerCallSite = Memoization
            .Memoize<Type, IList<MessageHandlerReference>>(
                componentType => {
                    var callSites = new List<MessageHandlerReference>();
                    var methods = componentType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    for (int methodIndex = 0; methodIndex < methods.Length; methodIndex++) {
                        var method = methods[methodIndex];
                        var attributes = method.GetCustomAttributes(typeof (MessageHandler), inherit: true);
                        var isMessageHandler = attributes.Length > 0;
                        if (isMessageHandler) {
                            callSites.Add(new MessageHandlerReference(attributes[0] as MessageHandler, method));
                        }
                    }
                    return callSites;
                });

        public static void ApplyTo(this ObjectRole objectRole, GameObject instance) {
            if (!instance.IsDestroyed()) {
                ComponentCache.Clear();
                // TODO Optimize this by caching the INetworkBehaviors in the ReplicatedObject
                instance.GetComponentsInChildren(ComponentCache);
                for (int i = 0; i < ComponentCache.Count; i++) {
                    var component = ComponentCache[i];
                    if (component is INetworkBehavior) {
                        var networkBehavior = component as INetworkBehavior;
                        // TODO Check if the role was already disabled/enabled
                        if (networkBehavior.Role.Suits(objectRole)) {
                            networkBehavior.OnRoleEnabled(objectRole);
                            component.enabled = true;
                        } else {
                            networkBehavior.OnRoleDisabled(objectRole);
                            component.enabled = false;
                        }   
                    }
                }
            }
        }

        public class MessageHandlerReference {
            public readonly MessageHandler MetaData;
            public readonly MethodInfo MethodInfo;

            public MessageHandlerReference(MessageHandler metaData, MethodInfo methodInfo) {
                MetaData = metaData;
                MethodInfo = methodInfo;
            }
        }

    }
}

