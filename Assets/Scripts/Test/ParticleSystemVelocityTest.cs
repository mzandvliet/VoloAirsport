using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Volo {
    public class ParticleSystemVelocityTest : MonoBehaviour {
        [SerializeField] private ParticleSystem _ps;
        [SerializeField] private Vector3 _velocity;

        void Update() {
            if (UnityEngine.Input.GetKeyDown(KeyCode.H)) {
                var vs = _ps.velocityOverLifetime;
                vs.xMultiplier = _velocity.x;
                vs.yMultiplier = _velocity.y;
                vs.zMultiplier = _velocity.z;
                _ps.Play();
            }
        }
    }
}
