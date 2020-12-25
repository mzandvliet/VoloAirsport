using System;
using System.Collections;
using System.Collections.Generic;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class AnonymousUpdateBehaviour : SingletonComponent<AnonymousUpdateBehaviour>
{
    public struct UpdateBehaviour
    {
        private readonly Action _updateFn;
        private readonly Action _onDestroyFn;

        public UpdateBehaviour(Action updateFn, Action onDestroyFn)
        {
            _updateFn = updateFn;
            _onDestroyFn = onDestroyFn;
        }

        public Action UpdateFn
        {
            get { return _updateFn; }
        }

        public Action OnDestroyFn
        {
            get { return _onDestroyFn; }
        }
    }

    // Useful for debugging.
    [SerializeField] private int _updateCount;

    public int UpdateCount {
        get { return _updateCount; }
    }

    private IList<UpdateBehaviour> _addUpdateQueue;
    private IList<UpdateBehaviour> _removeUpdateQueue;
    private IList<UpdateBehaviour> _activeUpdates;

    protected override void OnAwake()
    {
        _addUpdateQueue = new List<UpdateBehaviour>();
        _removeUpdateQueue = new List<UpdateBehaviour>();
        _activeUpdates = new List<UpdateBehaviour>();
        _updateCount = 0;
    }

    public IDisposable StartDisposableCoroutine(IEnumerator routine) {
        var disposable = new BooleanDisposable();
        StartCoroutine(CreateDisposableCoroutine(disposable, routine));
        return disposable;
    }

    private IEnumerator CreateDisposableCoroutine(ICancelable cancelable, IEnumerator coroutine) {
        while (coroutine.MoveNext() && !cancelable.IsDisposed) yield return coroutine.Current;
    }

    public void AddFn(UpdateBehaviour update)
    {
        _addUpdateQueue.Add(update);
    }

    public void RemoveFn(UpdateBehaviour update)
    {
        _removeUpdateQueue.Add(update);
    }

    void Update()
    {
        ProcessQueuedChanges();

        for (int i = 0; i < _activeUpdates.Count; i++)
        {
            _activeUpdates[i].UpdateFn();
        }
    }

    void OnDestroy()
    {
        for (int i = 0; i < _activeUpdates.Count; i++)
        {
            _activeUpdates[i].OnDestroyFn();
        }
    }

    void ProcessQueuedChanges()
    {
        for (int i = 0; i < _addUpdateQueue.Count; i++)
        {
            _activeUpdates.Add(_addUpdateQueue[i]);
        }
        _addUpdateQueue.Clear();

        for (int i = 0; i < _removeUpdateQueue.Count; i++)
        {
            _activeUpdates.Remove(_removeUpdateQueue[i]);
        }
        _removeUpdateQueue.Clear();

        _updateCount = _activeUpdates.Count;
    }
}