using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    public Joint root;
    public Joint end;

    public GameObject target;

    public float threshold = 0.05f;
    public float rate = 5.0f;
    public int steps = 20;

    private void Update()
    {
        for (int i = 0; i < steps; i++)
        {
            if (GetDistance(end.transform.position, target.transform.position) > threshold)
            {
                Joint current = root;
                while (current != null)
                {
                    float slope = CalculateSlope(current);
                    current.Rotate(-slope * rate);
                    current = current.GetChild();
                }
            }
        }
    }

    float CalculateSlope(Joint joint)
    {
        float deltaTheta = 0.01f;
        float distance1 = GetDistance(end.transform.position, target.transform.position);
        
        joint.Rotate(deltaTheta);
        
        float distance2 = GetDistance(end.transform.position, target.transform.position);
        
        joint.Rotate(-deltaTheta);

        return (distance2 - distance1) / deltaTheta;
    }

    float GetDistance(Vector3 point1, Vector3 point2)
    {
        return Vector3.Distance(point1, point2);
    }
}