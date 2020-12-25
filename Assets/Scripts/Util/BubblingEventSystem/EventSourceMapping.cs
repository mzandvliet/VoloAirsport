using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EventSourceMapping : MonoBehaviour {

    [SerializeField] private GameObject _target;

    public GameObject Target {
        get { return _target; }
    }
}
