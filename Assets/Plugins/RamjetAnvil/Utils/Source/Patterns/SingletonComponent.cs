using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

//#define DEBUG

namespace RamjetAnvil.Unity.Utility
{
    public interface ISingletonComponent
    {
        bool IsInitialized { get; }
        void Initialize();
    }

    /// <summary>
    ///  Base class for all singletons used in Unity projects.
    /// </summary>
    /// <typeparam name="T">Type of the subclass inheriting from SingletonComponent</typeparam>
    public abstract class SingletonComponent<T> : MonoBehaviour, ISingletonComponent where T : MonoBehaviour, ISingletonComponent {
        private static T _instance;
        private static bool _isQuitting;

        public static bool IsQuitting
        {
            get { return _isQuitting; }
        }

        private bool _isInitialized;

        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        public static T Instance
        {
            get
            {
                if (!_isQuitting)
                {
                    if (!_instance)
                    {
                        _instance = TryGetInstance();
                    }
                    if (_instance && Application.isPlaying && !_instance.IsInitialized)
                    {
                        _instance.Initialize();
                    }
                    return _instance;
                }

                return null;
            }
        }

        static SingletonComponent()
        {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
#endif
        }

        private static T TryGetInstance()
        {
            DebugLog("Attempting to find existing instance scene");

            T instance = FindObjectOfType(typeof(T)) as T;
            if (instance)
            {
                DebugLog("Found existing instance in scene");
            }
            else
            {
                DebugLog("Creating new instance in scene");

                var go = new GameObject("_" + typeof(T).Name);
                instance = go.AddComponent<T>();
                DontDestroyOnLoad(go);
            }
            if (!instance)
            {
                DebugLog("Failed to find or create instance");
            }
            return instance;
        }

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (!_isInitialized)
            {
                DebugLog("Initialize");
                OnAwake();
                _isInitialized = true;
            }
        }

        protected abstract void OnAwake();

        protected virtual void OnApplicationQuit()
        {
            DebugLog("OnApplicationQuit");
            _isQuitting = true;
        }

        private static void OnPlayModeStateChanged()
        {
            if (!Application.isPlaying)
            {
                //DebugLog("OnPlayModeStateChanged");
                _isQuitting = false;
            }
        }


        [Conditional("DEBUG")]
        private static void DebugLog(string message, params object[] args)
        {
            Debug.Log(string.Format("[" + typeof (T).Name + "]: " + message, args));
        }
    }
}