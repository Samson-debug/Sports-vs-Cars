using System;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Transform camTransform;

    private void Awake()
    {
        camTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(camTransform.position);
        transform.Rotate(0, 180f, 0);
    }
}


