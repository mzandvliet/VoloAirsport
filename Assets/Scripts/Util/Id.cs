using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Id : MonoBehaviour
{
    [SerializeField] private string _value;

    public string Value
    {
        get { return _value; }
        set { _value = value; }
    }
}
