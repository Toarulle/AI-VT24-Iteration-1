using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;
    void FixedUpdate()
    {
        transform.Rotate(Vector3.up, rotateSpeed);
    }
}
