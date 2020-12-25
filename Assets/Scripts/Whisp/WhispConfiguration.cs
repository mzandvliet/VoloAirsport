using UnityEngine;
using System.Collections;
using RamjetAnvil.Unity.Utility;

public class WhispConfiguration : MonoBehaviour {
    [SerializeField] private float _speed = 10f;

    public float Speed {
        get { return _speed; }
        set { _speed = value; }
    }
}

public static class WhispSimulation {
    public static Vector3 Simulate(Vector3 position, Vector3 direction, WhispConfiguration cfg, float deltaTime) {
        return position + direction * cfg.Speed * deltaTime;
    }
}
