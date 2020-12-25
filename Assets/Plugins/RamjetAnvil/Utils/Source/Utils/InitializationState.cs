using UnityEngine;

/// <summary>
/// Indicates whether a GameObject has been awoken (i.g. Awake was called on all of its components)
/// and whether a GameObject is started (i.g. Start was called on all of its components)
/// </summary>
public class InitializationState : MonoBehaviour
{
    private bool _isAwake;
    private bool _isStarted;

    void Awake()
    {
        _isAwake = false;
        _isStarted = false;
    }

    void Start()
    {
        _isAwake = true;
    }

    void Update()
    {
        if (!_isStarted)
        {
            _isStarted = true;
        }
    }

    public bool IsAwake
    {
        get { return _isAwake; }
    }

    public bool IsStarted
    {
        get { return _isStarted; }
    }
}
