using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This class represents a message that can be sent to listeners. It is a base
    /// abstract class which must be derived by each type of message that can be sent.
    /// </summary>
    public abstract class Message
    {
        #region Private Variables
        /// <summary>
        /// The message type.
        /// </summary>
        private MessageType _type;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the message type.
        /// </summary>
        public MessageType Type { get { return _type; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        public Message(MessageType type)
        {
            _type = type;
        }
        #endregion
    }
}
