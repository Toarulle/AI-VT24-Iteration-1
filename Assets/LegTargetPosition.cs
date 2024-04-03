using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegTargetPosition : MonoBehaviour
{
    public Vector3 pos;
    public Transform nextTarget;

    public float distanceUntilNextTarget = 4f;
    public float offset = 0.5f;
    public float liftHeight = 2f;
    public float speed = 2f;

    private float time = 0f;
    private Vector3 bezMiddlePoint;

    private bool movingLeg = false;

    private void Start()
    {
        pos += transform.parent.position;
    }

    void Update()
    {
        if (!movingLeg && ShouldUpdateTargetPosition())
        {
            StartCoroutine(SetNewTargetPosition());
        }
        transform.position = pos;
    }

    private IEnumerator SetNewTargetPosition()
    {
        movingLeg = true;
        time = 0f;
        Vector3 startPos = pos;
        bezMiddlePoint = Vector3.Lerp(startPos, nextTarget.position, 0.5f);
        bezMiddlePoint.y += liftHeight;
        Vector3 posOrNeg = Vector3.zero;
        float mag = (startPos - nextTarget.position).x;
        if (mag < 0)
        {
            posOrNeg = Vector3.right;
        }
        else if (mag > 0)
        {
            posOrNeg = Vector3.left;
        }
        while (time <= 1f)
        {
            time += Time.deltaTime*speed;
            
            Vector3 m1 = Vector3.Lerp(startPos, bezMiddlePoint, time);
            Vector3 m2 = Vector3.Lerp(bezMiddlePoint, nextTarget.position + posOrNeg*offset, time);
            pos = Vector3.Lerp(m1, m2, time);
            yield return null;
        }

        movingLeg = false;
    }
    
    private bool ShouldUpdateTargetPosition()
    {
        return Vector3.Distance(pos, nextTarget.position) > distanceUntilNextTarget;
    }
}