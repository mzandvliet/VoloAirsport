using UnityEngine;
using System.Collections;

public class RigidbodyMover : MonoBehaviour
{
    [SerializeField] private Rigidbody _otherBody;

    private Rigidbody _body;
    private Vector3 _offset;

    // Use this for initialization
    void Start()
    {
        _body = gameObject.GetComponent<Rigidbody>();
        _offset = _otherBody.position - _body.position;
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _body.position = Vector3.zero;
            _body.velocity = Vector3.zero;

            _otherBody.position = _offset;
            _otherBody.velocity = Vector3.zero;
        }
    }
}
