using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransformPosition : MonoBehaviour
{
    public Transform following;

    [ContextMenu("Update Position")]
    public void UpdatePosToFollowTransform()
    {
        Debug.Log($"Old Position: {transform.position}, New Position: {following.position}");
        transform.position = following.position;
    }
    
    private void OnValidate()
    {
        transform.position = following.position;
    }
}
