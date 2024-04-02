using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joint : MonoBehaviour
{
    public Joint child;

    public Joint GetChild()
    {
        return child;
    }

    public void Rotate(float angle)
    {
        transform.Rotate(Vector3.right * angle);
    }
}
