using System;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility
{
    public static class AnonymousMonoBehaviours
    {
        private static readonly Action EmptyAction = () => { };

        /// <summary>
        /// Creates an anonymous monobehaviour instance from the given update Action
        /// The instance can be destroyed by calling Dispose on the returned IDisposable.
        /// </summary>
        /// <param name="updateFn">Called whenever AnonymousMonobehaviour.Update() is called.</param>
        /// <param name="onDestroyFn">Called whenever AnonymousMonobehaviour is destroyed, useful for cleaning up.</param>
        /// <returns>A disposable allowing the caller to destroy this anonymous instance.</returns>
        public static IDisposable Update(Action updateFn, Action onDestroyFn = null)
        {
            // We capture the instance that we want to add and remove this update behaviour to
            // If we wouldn't capture it, it might be another instance when we remove the update behaviour than when we added it.
            var anonymousUpdateBehaviourInstance = AnonymousUpdateBehaviour.Instance;

            onDestroyFn = onDestroyFn ?? EmptyAction;
            var updateBehaviour = new AnonymousUpdateBehaviour.UpdateBehaviour(updateFn, onDestroyFn);
            anonymousUpdateBehaviourInstance.AddFn(updateBehaviour);
            return Disposables.Create(() =>
            {
                if (anonymousUpdateBehaviourInstance != null)
                {
                    anonymousUpdateBehaviourInstance.RemoveFn(updateBehaviour);
                }
            });
        }

        /// <summary>
        /// Adds an anonymous update behaviour to the given game object.
        /// Allows the behaviour to adhere to the lifecycle of the subject, 
        /// e.g. it's destroyed when the object gets destroyed and it can be removed with GameObject.RemoveComponent.
        /// </summary>
        /// <param name="subject">where the update function is bound to</param>
        /// <param name="updateFn">the logic to run every frame</param>
        public static void UpdateLocal(GameObject subject, Action updateFn) {
            var updateBehaviour = subject.AddComponent<LocalAnonymousUpdateBehaviour>();
            updateBehaviour.UpdateAction = updateFn;
        }
    }
}
