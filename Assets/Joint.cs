using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joint : MonoBehaviour
{
    public Joint child;
    public Axis axisToRotateAround;
    
    public enum Axis
    {
        X,
        Y,
        Z
    };

    public Joint GetChild()
    {
        return child;
    }

    public void Rotate(float angle)
    {
        switch (axisToRotateAround)
        {
            case Axis.X:
                transform.Rotate(Vector3.right * angle);
                break;
            case Axis.Y:
                transform.Rotate(Vector3.up * angle);
                break;
            case Axis.Z:
                transform.Rotate(Vector3.forward * angle);
                break;

        }
    }
}
