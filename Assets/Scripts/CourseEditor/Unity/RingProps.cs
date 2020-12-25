using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.CourseEditing;
using UnityEngine;

public class RingProps : MonoBehaviour {
    [SerializeField] private GameObject _ringEasy;
    [SerializeField] private GameObject _ringNormal;
    [SerializeField] private GameObject _ringHard;
    [SerializeField] private int _poolSize = 100;

    private IDictionary<PropType, IPrefabPool> _factory;

    void Awake() {
        _factory = new Dictionary<PropType, IPrefabPool> {
            {PropType.RingEasy, new PrefabPool("RingEasyPool", _ringEasy, _poolSize)},
            {PropType.RingNormal, new PrefabPool("RingNormalPool", _ringNormal, _poolSize)},
            {PropType.RingHard, new PrefabPool("RingHardPool", _ringHard, _poolSize)}
        };
    }

    public IDictionary<PropType, IPrefabPool> Factory {
        get { return _factory; }
    }

    void OnDestroy() {
        foreach (var prefabPool in _factory.Values) {
            prefabPool.Dispose();
        }
    }
}
