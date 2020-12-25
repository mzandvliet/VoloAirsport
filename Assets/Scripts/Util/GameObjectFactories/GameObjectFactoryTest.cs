using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.GameObjectFactories;
using UnityEngine;

public class GameObjectFactoryTest : MonoBehaviour {

    [SerializeField] private GameObject _prefab;

    [SerializeField] private GameObject _ringMeshEasyPrefab;
    [SerializeField] private GameObject _ringMeshNormalPrefab;
    [SerializeField] private GameObject _ringMeshHardPrefab;

    void Awake() {
        var ringEasy = CreateRingFactory(_ringMeshEasyPrefab, ringScale: 4);
        var ringNormal = CreateRingFactory(_ringMeshNormalPrefab, ringScale: 3);
        var ringHard = CreateRingFactory(_ringMeshHardPrefab, ringScale: 1);

        ringEasy.Instantiate();
        ringNormal.Instantiate();
        ringHard.Instantiate();
    }

    private Func<GameObject> CreateRingFactory(GameObject _ringmeshPrefab, int ringScale) {
        return GameObjectFactory.FromPrefab(_prefab, turnOff: true)
            .Adapt(g => {
                g.ReplaceChild("RingMesh", Instantiate(_ringmeshPrefab));
                g.FindInChildren("RingInner").transform.localScale *= ringScale;
                var s = g.AddComponent<SphereCollider>();
                s.radius = 2.9f * ringScale;
            })
            .TurnOn();
    }

}
