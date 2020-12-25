using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// Abstract base singleton class which can be derived by all non-monobehaviour classes
    /// in order to get access to singleton behaviour.
    /// </summary>
    /// <remarks>
    /// The implementation requires that the derived classes have a public parameterless
    /// constructor. That means that the client code can instantiate those classes thus
    /// breaking the singleton pattern. This can be solved using reflection, but it seems
    /// much cleaner to avoid reflection and just keep in mind of the limitation.
    /// </remarks>
    public abstract class SingletonBase<T> where T : class, new()
    {
        #region Private Static Variables
        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static T _instance = new T();
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Returns the singleton instance.
        /// </summary>
        public static T Instance { get { return _instance; } }
        #endregion
    }
}
