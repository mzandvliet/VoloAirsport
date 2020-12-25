using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This is the database that contains all listeners. Listeners can be registered
    /// with the database and the database can broadcast messages to all interested
    /// listeners.
    /// </summary>
    public class MessageListenerDatabase : SingletonBase<MessageListenerDatabase>
    {
        #region Private Variables
        /// <summary>
        /// Maps a message type to all listeners that listen to that type of message.
        /// </summary>
        private Dictionary<MessageType, HashSet<IMessageListener>> _messageTypeToMessageListeners = new Dictionary<MessageType, HashSet<IMessageListener>>();
        #endregion

        #region Public Methods
        /// <summary>
        /// Sends the specified message to all listeners which are interested in that message.
        /// </summary>
        public void SendMessageToInterestedListeners(Message message)
        {
            // Get the list of listeners which listen to the message and send the message to them
            HashSet<IMessageListener> interestedListeners = null;
            if (TryGetListenersForMessage(message, out interestedListeners)) SendMessageToListeners(message, interestedListeners);
        }

        /// <summary>
        /// Registers the specified listener for the specified message type. If the
        /// specified listener already listens to the specified message type, the 
        /// method has no effect.
        /// </summary>
        public void RegisterListenerForMessage(MessageType messageType, IMessageListener messageListener)
        {
            // Already registered?
            if (DoesListenerListenToMessage(messageType, messageListener)) return;

            // Register the listener
            RegisterNewMessageTypeIfNecessary(messageType);
            _messageTypeToMessageListeners[messageType].Add(messageListener);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Checks if 'messageListener' listens to messages of type 'messageType'.
        /// </summary>
        private bool DoesListenerListenToMessage(MessageType messageType, IMessageListener messageListener)
        {
            // Get the list of listeners for 'messageType' and then check if the listener exists among them
            HashSet<IMessageListener> listenersForMessage = null;
            if (_messageTypeToMessageListeners.TryGetValue(messageType, out listenersForMessage))
            {
                return listenersForMessage.Contains(messageListener);
            }
            else return false;
        }

        /// <summary>
        /// Adds a new entry inside the dictionary for the specified message type if necessary.
        /// </summary>
        private void RegisterNewMessageTypeIfNecessary(MessageType messageType)
        {
            if (!_messageTypeToMessageListeners.ContainsKey(messageType)) _messageTypeToMessageListeners.Add(messageType, new HashSet<IMessageListener>());
        }

        /// <summary>
        /// Attempts to retrieve the list of listeners which listen to the specified message.
        /// The list is stored in the last parameter if there are any listeners or it will be 
        /// set to null if no listeners exist for the message. 
        /// </summary>
        /// <returns>
        /// True if there are any listeners for the specified message and false otherwise.
        /// </returns>
        private bool TryGetListenersForMessage(Message message, out HashSet<IMessageListener> listeners)
        {
            listeners = null;
            if (_messageTypeToMessageListeners.ContainsKey(message.Type))
            {
                listeners = _messageTypeToMessageListeners[message.Type];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sends the specified message to all listeners which reside in 'listeners'.
        /// </summary>
        private void SendMessageToListeners(Message message, HashSet<IMessageListener> listeners)
        {
            foreach (IMessageListener listener in listeners)
            {
                listener.RespondToMessage(message);
            }
        }
        #endregion
    }
}
