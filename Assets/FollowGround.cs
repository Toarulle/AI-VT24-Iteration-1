using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowGround : MonoBehaviour
{
    void Update()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 2, Vector3.down, out RaycastHit hit, 4f, LayerMask.NameToLayer("Ground")))
        {
            transform.position = hit.point;
        }
    }
}
