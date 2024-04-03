using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowGround : MonoBehaviour
{
    void Update()
    {
        Physics.Raycast(transform.position + Vector3.up * 2, Vector3.down, out RaycastHit hit, 4f);
        if (hit.collider.CompareTag("Ground"))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y;
            transform.position = pos;
        }
    }
}
