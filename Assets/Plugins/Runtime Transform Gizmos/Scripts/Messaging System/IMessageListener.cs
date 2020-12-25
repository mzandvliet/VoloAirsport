namespace RTEditor
{
    /// <summary>
    /// This is an interface which must be implemented by all classes that must listen
    /// and respond to messages.
    /// </summary>
    public interface IMessageListener
    {
        #region Interface Methods
        /// <summary>
        /// All listeners must implement this method to respond to different types of messages.
        /// </summary>
        void RespondToMessage(Message message);
        #endregion
    }
}
