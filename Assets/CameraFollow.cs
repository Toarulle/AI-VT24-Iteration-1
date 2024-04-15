using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform toFollow;
    [SerializeField] private Vector3 offset;
    
    void Update()
    {
        transform.position = toFollow.position - offset;
    }

    [ContextMenu("SetOffset")]
    private void SetOffset()
    {
        Debug.Log($"Set Offset: {offset}");
        offset = toFollow.position - transform.position;
    }
}
