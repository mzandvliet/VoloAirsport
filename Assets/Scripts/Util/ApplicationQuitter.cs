using System;
using RamjetAnvil.Volo.Util;
using UnityEngine;

public class ApplicationQuitter : MonoBehaviour, ILock {

    private volatile bool _isQuitRequested;

    private readonly object _lock = new object();
    private volatile bool _isLockInvalid;
    private volatile int _lockCount;

    public void AcquireLock() {
        lock (_lock) {
            if (_isLockInvalid) {
                throw new Exception("Lock is no longer valid");
            }
            _lockCount++;    
        }
    }

    public void ReleaseLock() {
        lock (_lock) {
            _lockCount = Math.Max(0, _lockCount - 1);    
        }
    }

    void Update() {
        if (_isQuitRequested) {
            lock (_lock) {
                if (_lockCount > 0) {
                    Debug.Log("Cannot quit while lock is being held");
                } else {
                    Debug.Log("Quitting...");
                    _isLockInvalid = true;
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
            }
        }    
    }

    public void RequestQuit() {
        lock (_lock) {
            _isQuitRequested = true;    
        }
    }

}
