using System;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Turret Settings")]
    public Transform mouthTransform;
    public float startDelay = 1f;
    private bool firstTime = true;
    
    [Header("Grenade Settings")]
    public GameObject grenadePrefab;
    public float shootForce = 100f;
    public float shootInterval = 2f;
    
    private float lastShootTime;
    
    Truck truck;

    private void Awake()
    {
        truck = FindFirstObjectByType<Truck>();
    }

    void Start()
    {
        // Validate required components
        if (mouthTransform == null)
        {
            Debug.LogError("Turret: mouthTransform is not assigned!");
            enabled = false;
            return;
        }
        
        if (grenadePrefab == null)
        {
            Debug.LogError("Turret: grenadePrefab is not assigned!");
            enabled = false;
            return;
        }
        
        // Check if grenade prefab has rigidbody
        if (grenadePrefab.GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("Turret: grenadePrefab must have a Rigidbody component!");
            enabled = false;
            return;
        }
    }
    
    void Update()
    {
        if(GameManager.Instance.Paused) return;
        
        float nextShotTime = (lastShootTime + shootInterval) + (firstTime ? startDelay : 0f); 
        // Auto shooting at intervals
        if (Time.time >= nextShotTime)
        {
            ShootGrenade();
        }
    }
    
    void ShootGrenade()
    {
        // Instantiate grenade at spawn point
        GameObject grenade = Instantiate(grenadePrefab, mouthTransform.position, mouthTransform.rotation);
        
        // Get rigidbody component
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
        
        if (grenadeRb != null){
            grenadeRb.linearVelocity = truck.Velocity;
            // Apply force in the mouth's forward direction
            Vector3 shootDirection = mouthTransform.forward;
            grenadeRb.AddForce(shootDirection * shootForce);
        }
        else
        {
            Debug.LogError("Turret: Instantiated grenade doesn't have a Rigidbody component!");
        }
        
        // Update last shoot time
        lastShootTime = Time.time;
        
        // Optional: Add shooting effects here
        OnGrenadeShot();
    }
    
    // Override this in derived classes or use events for effects
    protected virtual void OnGrenadeShot()
    {
        // Add muzzle flash, sound, recoil, etc.
        Debug.Log("Turret shot Granade");
    }
    
    [ContextMenu("Start Shooting")]
    // Public methods for external control
    public void StartShooting()
    {
        enabled = true;
    }
    
    [ContextMenu("Stop Shooting")]
    public void StopShooting()
    {
        enabled = false;
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualize shooting direction in Scene view
        if (mouthTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(mouthTransform.position, mouthTransform.forward * 5f);
            
            if (mouthTransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(mouthTransform.position, 0.1f);
            }
        }
    }
}