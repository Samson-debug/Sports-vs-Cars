using System;
using UnityEngine;
using DG.Tweening;
using Unity.Android.Gradle.Manifest;
using Random = UnityEngine.Random;

public class BatController : MonoBehaviour
{
    [Header("Swing")]
    public float waitTimeAfterSwing = 0.5f;
    public float finalYRotation = -100f;
    public KeyCode swingKey = KeyCode.Space;
    
    [Header("Swing Animation")]
    public float swingDuration = 0.2f;
    public float returnDuration = 0.3f;
    public Ease swingEase = Ease.OutQuart;
    public Ease returnEase = Ease.InOutQuart;
    
    [Header("Tennis Shot")]
    public Transform carTransform;
    public float minThrowDistance = 5f;
    public float predictionTime = 0.2f;
    public float shotForce = 1500f;
    public float defaultZShootPos = 25f;
    public float defaultXShootPos = 3f;
    
    private Rigidbody carRigidbody;
    
    private Vector3 defaultRotation;
    private bool canSwinging;
    private bool isSwinging;
    private Sequence swingSequence;
    
    void Start()
    {
        // Store the initial rotation as default
        defaultRotation = transform.eulerAngles;
        
        // Cache car rigidbody for velocity calculations
        if (carTransform != null)
        {
            carRigidbody = carTransform.GetComponent<Rigidbody>();
            if (carRigidbody == null)
            {
                Debug.LogWarning("BatController: Car doesn't have a Rigidbody component! Trajectory prediction disabled.");
            }
        }
    }
    
    void Update()
    {
        if(GameManager.Instance.Paused) return;
        
        // Check for swing input
        if (Input.GetKeyDown(swingKey) && !canSwinging)
        {
            PerformSwing();
        }
    }
    
    void PerformSwing()
    {
        // Prevent multiple swings
        canSwinging = true;
        isSwinging = true;
        
        // Kill any existing sequence to prevent conflicts
        swingSequence?.Kill();
        
        // Create swing sequence
        swingSequence = DOTween.Sequence();
        
        // Calculate target rotation
        Vector3 targetRotation = new Vector3(
            defaultRotation.x, 
            finalYRotation, 
            defaultRotation.z
        );
        
        // Swing forward with force
        swingSequence.Append(
            transform.DORotate(targetRotation, swingDuration)
                .SetEase(swingEase).OnComplete(() => isSwinging = false)
        );
        
        // Wait at the swing position
        swingSequence.AppendInterval(waitTimeAfterSwing);
        
        // Return to default position
        swingSequence.Append(
            transform.DORotate(defaultRotation, returnDuration)
                .SetEase(returnEase)
        );
        
        // Reset swinging flag when complete
        swingSequence.OnComplete(() => {
            canSwinging = false;
        });
        
        // Play the sequence
        swingSequence.Play();
    }
    
    // Public method to trigger swing from other scripts
    public void TriggerSwing()
    {
        if (!canSwinging)
        {
            PerformSwing();
        }
    }
    
    // Check if bat is currently swinging
    public bool IsSwinging()
    {
        return canSwinging;
    }
    
    // Reset bat to default position (useful for debugging)
    public void ResetToDefault()
    {
        swingSequence?.Kill();
        transform.eulerAngles = defaultRotation;
        canSwinging = false;
    }
    
    void OnDestroy()
    {
        // Clean up DOTween sequences
        swingSequence?.Kill();
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Only hit grenades while swinging
        if (!isSwinging) return;
        
        // Check if it's a grenade (you can use tags or layer instead)
        if (other.CompareTag("Grenade"))
        {
            HitGrenade(other);
        }
    }
    
    void HitGrenade(Collider grenadeCollider)
    {
        Vector3 targetPosition;
        if (carTransform == null)
            targetPosition = new Vector3(defaultXShootPos, 0f, transform.position.z - defaultZShootPos);
        else
            targetPosition = carTransform.position;
        
        if(carRigidbody != null)
            targetPosition += carRigidbody.linearVelocity * predictionTime;
        
        Rigidbody grenadeRb = grenadeCollider.GetComponent<Rigidbody>();
        if (grenadeRb == null)
        {
            Debug.LogWarning("BatController: Grenade doesn't have a Rigidbody component!");
            return;
        }
        
        // Calculate predictive trajectory
        //Vector3 targetPosition = carTransform.position + carRigidbody.linearVelocity * predictionTime;
        Vector3 toTarget = targetPosition - grenadeCollider.transform.position;
        if (toTarget.magnitude < minThrowDistance){
            targetPosition= new Vector3(targetPosition.x, targetPosition.y, targetPosition.z - minThrowDistance);
            toTarget = targetPosition - grenadeCollider.transform.position;
        }
        Vector3 directionToTarget = toTarget.normalized;
        
        // Add upward arc for realistic projectile trajectory
        directionToTarget.y += 0.3f;
        directionToTarget = directionToTarget.normalized;
        
        // Reset grenade velocity and apply new force
        grenadeRb.linearVelocity = Vector3.zero;
        grenadeRb.angularVelocity = Vector3.zero;
        grenadeRb.AddForce(directionToTarget * shotForce);
        
        // Optional: Add some spin for realism
        grenadeRb.AddTorque(Random.insideUnitSphere * 10f);
        
        Debug.Log($"Grenade hit! Redirecting toward predicted car position: {targetPosition}");
        
        //Notify Grenade
        Grenade grenade = grenadeCollider.GetComponent<Grenade>();
        grenade.GotHit();
    }
    
    Vector3 CalculatePredictiveTarget(Vector3 grenadePosition)
    {
        Vector3 carCurrentPosition = carTransform.position;
        
        // If no rigidbody, just aim at current position
        if (carRigidbody == null)
        {
            return carCurrentPosition;
        }
        
        Vector3 carVelocity = carRigidbody.linearVelocity;
        
        // Simple trajectory prediction
        // Calculate approximate flight time based on distance and shot force
        float distance = Vector3.Distance(grenadePosition, carCurrentPosition);
        float grenadeSpeed = shotForce / 100f; // Rough estimate of grenade speed
        float flightTime = distance / grenadeSpeed;
        
        // Predict where car will be when grenade arrives
        Vector3 predictedPosition = carCurrentPosition + (carVelocity * flightTime);
        
        // Optional: Add some lead time for better accuracy
        predictedPosition += carVelocity * 0.1f;
        
        return predictedPosition;
    }
    
    public void SetTargetCar(GameObject car)
    {
        carTransform = car.transform;
        carRigidbody = car.GetComponent<Rigidbody>();
    }

    public void RemoveTargetCar()
    {
        carTransform = null;
        carRigidbody = null;
    }

    public void SetDefaultXPos(float pos)
    {
        defaultXShootPos = pos;
    }
}