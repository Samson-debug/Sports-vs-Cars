using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    //events
    public Action<GameObject> OnDeath;
    public Action<GameObject> OnDestroyed;
    
    [Header("Vfx")]
    public ParticleSystem blastParticlePrefab;
    public ParticleSystem smokeParticlePrefab;
    
    [Header("Health")]
    public int maxHealthPoints = 5;
    public float destroyTime = 3f;
    int healthPoints;
    
    [Header("UI")]
    public GameObject heatlthUIParent;
    public List<Image> healthPointImages = new List<Image>();
    
    public bool Dead => healthPoints <= 0;

    private void Start()
    {
        healthPoints = maxHealthPoints;
        heatlthUIParent.SetActive(true);
    }

    public void Gothit(int _healthPointsToLoose)
    {
        if(healthPoints <= 0) return;

        for (int i = _healthPointsToLoose; i > 0; i--){
            healthPoints--;
            healthPointImages[healthPoints].gameObject.SetActive(false);
        }

        if (healthPoints <= 0){
            Collider col = GetComponent<Collider>();
            col.enabled = false;
            
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null){
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            
            //disable ui
            heatlthUIParent.SetActive(false);
            
            OnDeath?.Invoke(gameObject);
            StartCoroutine(StartDieProcess());
        }
    }

    private IEnumerator StartDieProcess()
    {
        ParticleSystem blastPS = Instantiate(blastParticlePrefab, transform.position, Quaternion.identity, transform);
        blastPS.Play();
        
        yield return new WaitForSeconds(0.5f);
        
        Vector3 psPos = new Vector3(-0.16f, 1.36f, 1.96f);
        ParticleSystem smokePS = Instantiate(smokeParticlePrefab, transform.position, Quaternion.identity, transform);
        smokePS.transform.localPosition = psPos;
        smokePS.Play();
        
        yield return new WaitForSeconds(destroyTime);
        
        OnDestroyed?.Invoke(gameObject);
        Destroy(gameObject);
    }

    [ContextMenu("Die")]
    public void Die()
    {
        if(healthPoints <= 0) return;
        Gothit(healthPoints);
    }
}
