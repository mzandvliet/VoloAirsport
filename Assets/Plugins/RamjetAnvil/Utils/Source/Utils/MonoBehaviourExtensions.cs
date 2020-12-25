using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RamjetAnvil.Unity.Utility
{
    public static class MonoBehaviourExtensions
    {
        #region extention methods

        public static bool IsDestroyed(this Object me) {
            return me == null;
        }

        public static T GetComponentInHierarchy<T>(this GameObject me, string name = null) where T : Component {
            var c = me.GetComponent<T>();
            if (c != null) {
                return c;
            }

            for (int i = 0; i < me.transform.childCount; i++) {
                var child = me.transform.GetChild(i);

                var component = GetComponentInHierarchy<T>(child.gameObject, name);
                if (component != null && (name == null || component.name.Equals(name))) {
                    return component;
                }

            }

            return null;
        }
		
        public static T GetComponentOfInterface<T>(this GameObject me) where T : class {
            return me.GetComponent(typeof (T)) as T;
        }

		public static List<T> GetComponentsOfInterface<T>(this GameObject me) where T : class
        {
            var behaviours = me.GetComponents<MonoBehaviour>();
            return FilterForInterface<T>(behaviours);
        }

        private static readonly List<Component> ComponentCache = new List<Component>(256);

        public static void GetComponentsOfInterface<T>(this GameObject me, IList<object> components) where T : class {
            ComponentCache.Clear();
            me.GetComponents(ComponentCache);
            for (int i = 0; i < ComponentCache.Count; i++) {
                var component = ComponentCache[i];
                if (component is T) {
                    components.Add(component);
                }
            }
        }

        public static List<T> GetComponentsOfInterfaceInChildren<T>(this GameObject me) where T : class
        {
            var behaviours = me.GetComponentsInChildren<MonoBehaviour>();
            return FilterForInterface<T>(behaviours);
        }

        public static void GetComponentsOfInterfaceInChildren<T>(this GameObject me, IList<object> components) where T : class {
            ComponentCache.Clear();
            me.GetComponentsInChildren(ComponentCache);
            for (int i = 0; i < ComponentCache.Count; i++) {
                var component = ComponentCache[i];
                if (component is T) {
                    components.Add(component);
                }
            }
        }

        public static List<T> GetComponentsOfInterface<T>(this Behaviour me) where T : class
        {
            var behaviours = me.GetComponents<MonoBehaviour>();
            return FilterForInterface<T>(behaviours);
        }

        public static List<T> GetComponentsOfInterfaceInChildren<T>(this Behaviour me) where T : class
        {
            var behaviours = me.GetComponentsInChildren<MonoBehaviour>();
            return FilterForInterface<T>(behaviours);
        }

        private static List<T> FilterForInterface<T>(IEnumerable<MonoBehaviour> behaviours) where T : class
        {
            var components = new List<T>();

            foreach (var behaviour in behaviours)
            {
                T component = behaviour as T;
                if (component != null)
                    components.Add(component);
            }

            return components;
        }

        public static void RemoveComponents<T>(this GameObject g) {
            ComponentCache.Clear();
            g.GetComponents(typeof(T), ComponentCache);
            for (int i = 0; i < ComponentCache.Count; i++) {
                var component = ComponentCache[i];
                Object.Destroy(component);
            }
        }

        public static void RemoveComponentsInChildren<T>(this GameObject g) {
            var components = g.GetComponentsInChildren(typeof(T), includeInactive: true);
            for (int i = 0; i < components.Length; i++) {
                var component = components[i];
                Object.Destroy(component);
            }
        }

        #endregion

        #region Static utility methods

        public static T GetComponentInParents<T>(Transform transform) where T : Behaviour
        {
            T component = transform.GetComponent<T>();
            if (component)
                return component;

            if (transform.parent)
                return GetComponentInParents<T>(transform.parent);

            return null;
        }

        public static List<T> FindComponentsOfInterface<T>() where T : class
        {
            var monoBehaviours = Object.FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];
            return FilterForInterface<T>(monoBehaviours);
        }

        public static List<T> FindComponentsOfInterfaceIncludingAssets<T>() where T : class
        {
            var monoBehaviours = Resources.FindObjectsOfTypeAll(typeof (MonoBehaviour)) as MonoBehaviour[];
            return FilterForInterface<T>(monoBehaviours);
        }

        /// <summary>
        /// This methods automatically chooses the proper way to Destroy, regardless of whether it
        /// is called in Editor or Game code.
        /// </summary>
        /// <param name="obj">The Unity Object to be destroyed.</param>
        public static void DestroyAgnostic(Object obj)
        {
            DestroyAgnostic(obj, false);
        }

        /// <summary>
        /// This methods automatically chooses the proper way to Destroy, regardless of whether it
        /// is called in Editor or Game code.
        /// </summary>
        /// <param name="obj">The Unity Object to be destroyed.</param>
        public static void DestroyAgnostic(Object obj, bool allowDestroyingAssets)
        {
            if (obj == null)
            {
                Debug.LogWarning("DestroyAgnostic: Object already destroyed.");
                return;
            }

            Debug.Log(String.Format("DestroyAgnostic: {0} [{1}]", obj.name.ToString(), obj.GetType().Name));

            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj, allowDestroyingAssets);
        }
    }

    #endregion
}
