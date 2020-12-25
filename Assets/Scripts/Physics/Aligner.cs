using System;
using UnityEngine;

/* 
 * Aligns a game object to be either an arm or a leg wing.
 * 
 * Note: alignment is both for visuals and physics, hence the hook to a wing's OnPreFixedUpdate
 */

[ExecuteInEditMode]
public class Aligner : MonoBehaviour {
    [SerializeField] private Airfoil1D _airfoil;
    /* The points used to align the wing. */
    [SerializeField] private Transform[] _anchorPoints;
    /* Side of the wing relative to the player. */
    [SerializeField] private Side _orientation;

    public event Action OnAlign;

    private void OnEnable() {
        _airfoil.OnPreUpdate += Align;
        _airfoil.OnPreFixedUpdate += Align;
    }

    private void OnDisable() {
        if (_airfoil) {
            _airfoil.OnPreUpdate -= Align;
            _airfoil.OnPreFixedUpdate -= Align;
        }
    }
    
    public void Align(IAerodynamicSurface surface) {
        if (_anchorPoints == null || _anchorPoints.Length != 3) {
            return;
        }

        if (OnAlign != null) {
            OnAlign();
        }

        transform.position = (_anchorPoints[0].position + _anchorPoints[1].position + _anchorPoints[2].position) / 3f;

        Vector3 normal = Vector3.zero;
        switch (_orientation)
        {
            case Side.Left:
                normal = Vector3.Cross(_anchorPoints[2].position - _anchorPoints[1].position, _anchorPoints[1].position - _anchorPoints[0].position);
                break;
            case Side.Right:
                normal = -Vector3.Cross(_anchorPoints[2].position - _anchorPoints[1].position, _anchorPoints[1].position - _anchorPoints[0].position);
                break;
            case Side.Center:
                normal = Vector3.Cross(_anchorPoints[0].position - _anchorPoints[1].position, _anchorPoints[2].position - _anchorPoints[0].position);
                break;
        }

        Vector3 forward = Vector3.zero;
        switch (_orientation)
        {
            case Side.Left:
            case Side.Right:
                forward = _anchorPoints[1].position - _anchorPoints[0].position;
                transform.rotation = Quaternion.LookRotation(forward, normal);
                break;
            case Side.Center:
                forward = _anchorPoints[0].position - transform.position;
                transform.rotation = Quaternion.LookRotation(forward, normal);
                break;
        }
    }

    public enum Side {
        Left = -1,
        Center = 0,
        Right = 1
    }
}
