using System;
using UnityEngine;

public class Follower : MonoBehaviour
{
    public Transform followTarget;

    private Vector3 offset;
    private void Awake()
    {
        if (followTarget == null)
            followTarget = FindFirstObjectByType<Truck>().transform;
        
        offset = transform.position - followTarget.position;
    }

    private void LateUpdate()
    {
        if(GameManager.Instance.Paused) return;
        
        transform.position = followTarget.position + offset;
    }
}