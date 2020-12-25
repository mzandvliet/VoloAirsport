using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RxUnity.Util;
using UnityEngine;

namespace RxUnity.Schedulers
{
    public class UnityThreadDispatcher : MonoBehaviour
    {
        readonly IProducerConsumerCollection<Action> _actions = new ConcurrentQueue<Action>();
        readonly IProducerConsumerCollection<IEnumerator> _newCoroutines = new ConcurrentQueue<IEnumerator>();

        static readonly UnitySingleton<UnityThreadDispatcher> MainThreadDispatcherSingleton = 
            new UnitySingleton<UnityThreadDispatcher>("UnityThreadDispatcher");

        private UnityThreadDispatcher() {}

        public static UnityThreadDispatcher Instance
        {
            get
            {
                return MainThreadDispatcherSingleton.Instance;
            }
        }

        void Update()
        {
            ProcessActions();
            StartNewCoroutines();
        }

        void ProcessActions()
        {
            Action action;
            while (_actions.TryTake(out action))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex); // Is log can't handle...?
                }
            }
        }

        void StartNewCoroutines()
        {
            IEnumerator newCoroutine;
            while (_newCoroutines.TryTake(out newCoroutine))
            {
                StartCoroutine(newCoroutine);
            }
        }

        public void Post(Action item)
        {
            _actions.TryAdd(item);
        }

        public void QueueCoroutine(IEnumerator routine)
        {
            _newCoroutines.TryAdd(routine);
        }
    }
}