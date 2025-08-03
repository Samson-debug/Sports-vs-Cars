using UnityEngine;

public class Grenade : MonoBehaviour
{
    public ParticleSystem blastParticleEffect;
    bool canDamage;
    
    private void Start()
    {
        Destroy(gameObject, 10f);
    }

    private void OnTriggerEnter(Collider other)
    {
        print($"Granade collided with {other.name}");
        if(other.CompareTag("Truck")) return;
        
        if (canDamage && other.TryGetComponent(out Health health)){
            if(other.TryGetComponent(out Truck truck)) return;
            health.Gothit(1);
            canDamage = false;
            
            ParticleSystem boomParticle = Instantiate(blastParticleEffect, transform.position, Quaternion.identity);
            boomParticle.Play();
            
            Destroy(gameObject);
        }
    }

    public void GotHit()
    {
        canDamage = true;
    }
}