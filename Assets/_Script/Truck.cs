using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Truck : MonoBehaviour
{
    public Transform backPoint;
    public float speed = 10f;

    Rigidbody rb;
    
    Vector3 targetVelocity;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        StartMoving();
    }

    private void Update()
    {
        rb.linearVelocity = targetVelocity;
    }

    public Vector3 Position => backPoint == null ? transform.position : backPoint.position;
    public Vector3 Velocity => rb.linearVelocity;
    private void StartMoving()
    {
        targetVelocity = speed * transform.forward;
    }

    private void StopMoving()
    {
        targetVelocity = Vector3.zero;
    }

    public void Hit()
    {
        print($"truck was hit");
    }
}
