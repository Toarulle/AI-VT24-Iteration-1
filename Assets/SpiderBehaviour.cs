using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBehaviour : MonoBehaviour
{
    public int smoothing = 1;
    public List<Vector3> legsList = new List<Vector3>();
    
    private Vector3 velocity;
    private Vector3 lastVelocity;
    [SerializeField] private float velocityMultiplier = 2f;
    private Vector3 lastBodyPosition;
    private Vector3 lastBodyUp;
    
    public Vector3 Velocity => velocity;
    public float VelocityMultiplier => velocityMultiplier;
    public Vector3 LastBodyUp => lastBodyUp;

    private void Start()
    {
        lastBodyUp = transform.up;
        lastBodyPosition = transform.position;
    }

    private void FixedUpdate()
    {
        velocity = transform.position - lastBodyPosition;
        velocity = (velocity + smoothing * lastVelocity) / (smoothing + 1f);
        if (velocity.magnitude < 0.000025f)
            velocity = lastVelocity;
        else
            lastVelocity = velocity;
        
        lastBodyPosition = transform.position;

        if (legsList.Count < 4)
            return;
        Vector3 v1 = legsList[0] - legsList[1];
        Vector3 v2 = legsList[2] - legsList[3];
        Vector3 normal = Vector3.Cross(v1, v2).normalized;
        Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f / (float)(smoothing + 1));
        transform.up = up;
        lastBodyUp = up;
    }
}
