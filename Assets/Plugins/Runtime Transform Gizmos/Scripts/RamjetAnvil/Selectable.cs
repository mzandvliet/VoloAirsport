using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RTEditor {
    public class Selectable : MonoBehaviour {
        [SerializeField] private string _id;

        public string Id {
            get { return _id; }
        }
    }
}
