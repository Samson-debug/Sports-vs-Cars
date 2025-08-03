using System;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

/*public class Chaser : MonoBehaviour
{
    public Action OnRamComplete;
    
    [Header("Follow")]
    public float chaseDistance = 10f;
    public float minSpeed = 10f;
    public float maxSpeed = 10f;
    public float acceleration = 5f;
    public float deceleration = 40f;
    public float followRotationSpeed = 180f;

    [Header("Ram")]
    public float maxRamSpeed = 150f;
    public float ramAcceleration = 10f;
    public float ramRotationSpeed = 360f;
    
    Truck truck;
    Rigidbody rb;
    Health health;
    CarState currentState;
    
    private Vector3 currentVelocity;
    private Vector3 lanePosition;
    private bool ramming;
    private bool returningToLane;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        truck = FindFirstObjectByType<Truck>();
        
        lanePosition = transform.position;
    }

    private void OnEnable()
    {
        health.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        health.OnDeath -= OnDeath;
    }

    private void Start()
    {
        currentState = CarState.Following;
    }

    private void FixedUpdate()
    {
        /*if(ramming) RamTowardsTarget();
        else Follow();#1#

        switch (currentState){
            case CarState.Following:
                Follow();
                break;
            case CarState.Ramming:
                RamTowardsTarget();
                break;
            case CarState.ReturningToLane:
                Follow();
                if(IsOnLane()){
                    currentState = CarState.Following;
                    OnRamComplete?.Invoke();
                }
                break;
        }
    }

    private void Follow()
    {
        if(ramming) return;
        
        Vector3 toTarget = new Vector3(lanePosition.x, transform.position.y, truck.Position.z) - transform.position;
        
        //Debug
        print($"{name} Velocity: {rb.linearVelocity}");
        print($"{name} truck position: {truck.Position}");
        print($"{name} Chaser position: {transform.position}");
        print($"{name} Distance : {toTarget.magnitude}");

        // Remove any vertical component (if cars are on a flat plane)
        toTarget.y = 0f;

        //Rotation
        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, followRotationSpeed * Time.fixedDeltaTime); 
        
        float currentDistance = toTarget.magnitude;

        // ---- SPEED CONTROL ----
        float distanceError = currentDistance - chaseDistance;
        float targetSpeed = Mathf.Clamp(distanceError, minSpeed, maxSpeed);

        // Convert target speed to desired velocity
        Vector3 desiredVelocity = transform.forward * targetSpeed;

        // Calculate the force needed to reach desired velocity
        Vector3 force = (desiredVelocity - rb.linearVelocity) * (distanceError > 0 ? acceleration : deceleration);

        print($"{name} velocity difference : {(desiredVelocity - rb.linearVelocity).magnitude}");
        rb.AddForce(force, ForceMode.Acceleration);
    }

    [ContextMenu("Ram")]
    public void StartRam()
    {
        /*ramming = true;
        returningToLane = false;#1#
        currentState = CarState.Ramming;
    }

    private void RamTowardsTarget()
    {
        
        Vector3 toTarget = truck.Position - transform.position;
        toTarget.y = 0f;

        float currentDistance = toTarget.magnitude;

        // Look at the target quickly
        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, ramRotationSpeed * Time.fixedDeltaTime);

        // Full acceleration toward the target
        Vector3 desiredVelocity = transform.forward * maxRamSpeed;
        Vector3 force = (desiredVelocity - rb.linearVelocity) * ramAcceleration * 0.1f;

        rb.AddForce(force, ForceMode.Acceleration);
    }

    private void OnCollisionEnter(Collision collision)
    {

        // Check if we hit the truck (target)
        if (collision.transform == truck.transform)
        {
            truck.GetComponent<Health>()?.Gothit(1);
            currentState = CarState.ReturningToLane;
        }
    }

    private bool IsOnLane()
    {
        return Mathf.Abs(lanePosition.x - transform.position.x) < 0.3;
    }

    private void OnDeath(GameObject obj)
    {
        if(currentState == CarState.ReturningToLane || currentState == CarState.Ramming) 
            OnRamComplete?.Invoke();
        
        GameManager.Instance.CarDestroyed();
    }
}

public enum CarState
{
    Following,
    Ramming,
    ReturningToLane
}*/

public class Chaser : MonoBehaviour
{
    public Action OnRamComplete;
    
    [Header("Follow")]
    public float chaseDistance = 10f;
    public float minSpeed = 10f;
    public float maxSpeed = 10f;
    public float acceleration = 5f;
    public float followRotationSpeed = 180f;

    [Header("Ram")]
    public float maxRamSpeed = 150f;
    public float ramAcceleration = 10f;
    public float ramRotationSpeed = 360f;
    
    Truck truck;
    Rigidbody rb;
    Health health;
    CarState currentState;
    
    private Vector3 currentVelocity;
    private Vector3 lanePosition;
    private bool ramming;
    private bool returningToLane;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        truck = FindFirstObjectByType<Truck>();
        
        lanePosition = transform.position;
    }

    private void OnEnable()
    {
        health.OnDestroyed += OnDeath;
    }

    private void OnDisable()
    {
        health.OnDestroyed -= OnDeath;
    }

    private void Start()
    {
        currentState = CarState.Following;
    }

    private void FixedUpdate()
    {
        switch (currentState){
            case CarState.Following:
                Follow();
                break;
            case CarState.Ramming:
                RamTowardsTarget();
                break;
            case CarState.ReturningToLane:
                Follow();
                if(IsOnLane()){
                    currentState = CarState.Following;
                    OnRamComplete?.Invoke();
                }
                break;
        }
    }

    public float positionGain = 2f;
    public float velocityGain = 2f;
    private void Follow()
    {
        // Predict where target will be in the future
        float predictionTime = 0.2f; // Adjust based on how far ahead to look
        Vector3 predictedPosition = truck.Position + (truck.Velocity * predictionTime);
        predictedPosition = new Vector3(lanePosition.x, transform.position.y, predictedPosition.z - chaseDistance);

        //Rotation
        Vector3 toTarget = new Vector3(lanePosition.x, transform.position.y, truck.Position.z) - transform.position;
        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized);
        float rotSpeedMultiplier = Mathf.InverseLerp(minSpeed, maxSpeed, rb.linearVelocity.magnitude);
        transform.rotation = Quaternion.RotateTowards
        (transform.rotation,
            targetRot,
            followRotationSpeed * Time.fixedDeltaTime
        );        
        
        // Use predicted position instead of current position
        Vector3 positionError = predictedPosition - transform.position;
        Vector3 velocityError = truck.Velocity - rb.linearVelocity;
    
        Vector3 desiredAcceleration = 
            (positionError * positionGain) + 
            (velocityError * velocityGain);
    
        desiredAcceleration = Vector3.ClampMagnitude(desiredAcceleration, acceleration);
        //rb.AddForce(desiredAcceleration * rb.mass, ForceMode.Force);
        rb.linearVelocity += desiredAcceleration * Time.fixedDeltaTime;
        
        // Clamp minimum velocity while preserving direction
        float currentSpeed = rb.linearVelocity.magnitude;
        if (currentSpeed < 0.01f || Vector3.Dot(rb.linearVelocity, transform.forward) < 0) // Avoid division by zero
        {
            rb.linearVelocity = transform.forward * minSpeed;
        }
        else if (currentSpeed < minSpeed){
            rb.linearVelocity = rb.linearVelocity.normalized * minSpeed;
        }

        print($"Pos Error: <color=yellow>{positionError}</color> && velocity Error: <color=yellow>{velocityError}</color>");
        print($"Acceleration: <color=green>{desiredAcceleration.magnitude}</color> & Accel Dir: <color=green>{desiredAcceleration.normalized}</color>");
        print($"Velocity: <color=red>{rb.linearVelocity}</color>");
    }

    [ContextMenu("Ram")]
    public void StartRam()
    {
        /*ramming = true;
        returningToLane = false;*/
        currentState = CarState.Ramming;
    }

    private void RamTowardsTarget()
    {
        
        Vector3 toTarget = truck.Position - transform.position;
        toTarget.y = 0f;

        float currentDistance = toTarget.magnitude;

        // Look at the target quickly
        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, ramRotationSpeed * Time.fixedDeltaTime);

        // Full acceleration toward the target
        Vector3 desiredVelocity = transform.forward * maxRamSpeed;
        Vector3 force = (desiredVelocity - rb.linearVelocity) * ramAcceleration * 0.1f;

        rb.AddForce(force, ForceMode.Acceleration);
    }

    private void OnCollisionEnter(Collision collision)
    {

        // Check if we hit the truck (target)
        if (collision.transform == truck.transform)
        {
            truck.GetComponent<Health>()?.Gothit(1);
            currentState = CarState.ReturningToLane;
        }
    }

    private bool IsOnLane()
    {
        return Mathf.Abs(lanePosition.x - transform.position.x) < 0.3;
    }

    private void OnDeath(GameObject obj)
    {
        if(currentState == CarState.ReturningToLane || currentState == CarState.Ramming) 
            OnRamComplete?.Invoke();
        
        GameManager.Instance.CarDestroyed();
    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying) return;
        
        Vector3 position = transform.position + Vector3.up * 0.3f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(position, transform.forward * 2f);
        Gizmos.color = Color.red;
        Vector3 targetPos = new Vector3(lanePosition.x, transform.position.y, truck.Position.z);
        targetPos.y += 0.3f;
        Gizmos.DrawSphere(targetPos, 0.1f);
        Gizmos.DrawLine(position, targetPos);
    }
}

public enum CarState
{
    Following,
    Ramming,
    ReturningToLane
}