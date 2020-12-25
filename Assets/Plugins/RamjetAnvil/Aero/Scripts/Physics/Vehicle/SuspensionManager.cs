using UnityEngine;
using System.Collections.Generic;

public class SuspensionManager : MonoBehaviour
{
    [SerializeField]
    float rideHeight = 5f;
    [SerializeField]
    float springRate = 20f;
    [SerializeField]
    float dampeningFactor = 3f;

    List<Suspension> suspensions;
    bool isGrounded;

    public bool IsGrounded
    {
        get { return isGrounded; }
    }

    void Update()
    {
        isGrounded = false;
        for (int i = 0; i < suspensions.Count; i++)
        {
            isGrounded = isGrounded | suspensions[i].IsGrounded;
        }
    }

    void Awake()
    {
        suspensions = new List<Suspension>(GetComponentsInChildren<Suspension>());
        foreach (Suspension s in suspensions)
        {
            s.SpringLength = rideHeight;
            s.SpringRate = springRate;
            s.DampeningFactor = dampeningFactor;
        }
    }
}