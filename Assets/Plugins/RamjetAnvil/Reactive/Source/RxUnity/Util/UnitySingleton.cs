using UnityEngine;

namespace RxUnity.Util
{
    /// <summary>
    /// Will create a new instance of the specified MonoBehaviour lazily.
    /// </summary>
    /// <typeparam name="T">The type to instantiate</typeparam>
    public class UnitySingleton<T> where T : MonoBehaviour
    {
        private readonly string _name;
        private readonly object _gate = new object();
        private volatile bool _isInitialized = false;
        private volatile T _instance;

        public UnitySingleton(string name)
        {
            _name = name;
        }

        public T Instance
        {
            get
            {
                if (!_isInitialized)
                {
                    lock (_gate)
                    {
                        if (!_isInitialized)
                        {
                            // TODO Run in unity thread. Is that even possible?
                            var go = new GameObject(_name);
                            _instance = go.AddComponent<T>();
                            Object.DontDestroyOnLoad(go);
                            _isInitialized = true;
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
