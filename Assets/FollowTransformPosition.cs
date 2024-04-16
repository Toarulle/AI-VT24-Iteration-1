using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransformPosition : MonoBehaviour
{
    public Transform following;
    public float offset = 0;

    [ContextMenu("Update Position")]
    public void UpdatePosToFollowTransform()
    {
        transform.position = following.position; // + following.InverseTransformDirection(Vector3.forward)*offset;
    }
    
    private void OnValidate()
    {
        UpdatePosToFollowTransform();
    }
}
