using System;
using UnityEngine;

public class RbPlay : MonoBehaviour
{
    public float acceleration = 10f;
    public Vector3 forceDirection = Vector3.forward;
    public ForceMode forceMode = ForceMode.Force;

    private Rigidbody rb;
    
    private bool stopped;
    private Vector3 initialPosition;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if(stopped) return;
        
        Vector3 beforeVelocity = rb.linearVelocity;
        print($"Before Velocity: <color=green>{beforeVelocity}</color>");
        
        if(forceMode == ForceMode.VelocityChange)
            rb.linearVelocity += forceDirection * acceleration * Time.fixedDeltaTime;
        else
            rb.AddForce(forceDirection * acceleration, forceMode);
        
        print($"After Velocity: <color=green>{rb.linearVelocity}</color>");
        print($"Velocity Difference: <color=yellow>{rb.linearVelocity - beforeVelocity}</color>");
    }

    [ContextMenu("Stop")]
    public void Stop()
    {
        stopped = true;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
    }

    [ContextMenu("Activate")]
    public void Activate()
    {
        stopped = false;
        rb.isKinematic = false;
    }

    [ContextMenu("Reset")]
    public void ResetPosition()
    {
        stopped = true;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        
        transform.position = initialPosition;
    }
}
