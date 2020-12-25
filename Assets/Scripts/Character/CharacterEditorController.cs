using System.Collections.Generic;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.IK;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public class CharacterEditorController : MonoBehaviour {
    

    [SerializeField] private EditorOrbitCamera _camera;
    [Dependency, SerializeField] private AbstractUnityClock _clock;
    [SerializeField] private float _sensitivity = 50f;

    private bool _isDragging;
    private float _dragDepth;
    private Vector3 _dragOffset;

    private IList<MuscleJoint> _jointHierarchy;
    private IList<MuscleJoint> _ikHierarchy;
    private Transform _ikTarget;

    private void Awake()
    {
        _ikTarget = new GameObject("IKTarget").transform;
    }

    private void OnDestroy()
    {
        Destroy(_ikTarget);

        if (_ikHierarchy != null)
            DestroyHierarchy(_ikHierarchy);
    }

    // Todo: Locking joints by clicking on them
    private void Update()
    {
        // TODO Buttons don't work anymore
        bool mouseLeftDown = Input.GetButtonDown("unity_mouse_0");
        bool mouseLeftUp = Input.GetButtonUp("unity_mouse_0");
        bool mouseLeft = Input.GetButton("unity_mouse_0");
        
        Vector3 cursorPosition = Input.mousePosition;

        if (mouseLeftDown)
        {
            GameObject selectedObject = QueryCursor(cursorPosition);
            if (selectedObject)
            {
                MuscleJoint joint = selectedObject.GetComponent<MuscleJoint>();
                if (joint)
                {
                    StartDragging(joint);
                }
                else
                {
                    _camera.Center = selectedObject.transform.position;
                }
            }
        }
        if (mouseLeftUp)
        {
            StopDragging();
        }

        if (mouseLeft)
        {
            if (_isDragging)
            {
                Drag(cursorPosition);
            }
            else
            {
                Look();
            }
        }

        if (_jointHierarchy != null && _ikHierarchy != null)
        {
            IKSolver.Slerp(_ikHierarchy, _jointHierarchy, 10f * _clock.DeltaTime);
        }
    }

    private GameObject QueryCursor(Vector3 cursorPosition)
    {
        Ray cursorRay = _camera.GetComponent<Camera>().ScreenPointToRay(cursorPosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(cursorRay, out hitInfo))
        {
            return hitInfo.collider.gameObject;
        }
        return null;
    }

    private void StartDragging(MuscleJoint joint)
    {
        _isDragging = true;

        if (_ikHierarchy != null)
            DestroyHierarchy(_ikHierarchy);

        _jointHierarchy = IKSolver.GetHierarchy(joint);
        _ikHierarchy = IKSolver.CopyHierarchy(_jointHierarchy);
        _ikTarget.position = joint.transform.position;
        _ikTarget.rotation = joint.transform.rotation;

        Vector3 screenSpacePosition = _camera.GetComponent<Camera>().WorldToScreenPoint(joint.transform.position);
        _dragDepth = screenSpacePosition.z;
        Vector3 cursorPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpacePosition.z);
        _dragOffset = joint.transform.position - _camera.GetComponent<Camera>().ScreenToWorldPoint(cursorPosition);
    }

    private void DestroyHierarchy(IEnumerable<MuscleJoint> joints)
    {
        foreach (MuscleJoint joint in joints)
        {
            if (joint)
                Destroy(joint.gameObject);
        }
    }

    private void StopDragging()
    {
        _isDragging = false;
    }

    // Drag the selected object in screenspace
    private void Drag(Vector3 cursorPosition)
    {
        Vector3 projectedCursorPosition = new Vector3(cursorPosition.x, cursorPosition.y, _dragDepth);
        Vector3 cursorWorldPosition = _camera.GetComponent<Camera>().ScreenToWorldPoint(projectedCursorPosition);
        cursorWorldPosition += _dragOffset;
        
        _ikTarget.position = cursorWorldPosition;
        IKSolver.Evaluate(_ikHierarchy, _ikTarget);
    }
    
    private void Look()
    {
        _camera.Move(
            Input.GetAxis("MouseX") * _sensitivity * _clock.DeltaTime,
            -Input.GetAxis("MouseY") * _sensitivity * _clock.DeltaTime);
    }

    private void OnDrawGizmos()
    {
        if (_ikHierarchy != null)
        {
            IKSolver.DrawGizmos(_ikHierarchy, _ikTarget);
        }
    }
}
