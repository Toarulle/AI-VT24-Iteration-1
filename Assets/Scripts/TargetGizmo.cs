using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGizmo : MonoBehaviour
{
    public Color color;
    public float size;

    private void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position,size);
    }
}
