using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class LegTargetPosition : MonoBehaviour
{
    public bool startFront = false;
    public Transform nextTarget;
    public SpiderBehaviour spider;

    public float distanceUntilNextTarget = 4f;
    public float offset = 0.5f;
    public float liftHeight = 2f;
    public float speed = 2f;
    
    private Vector3 pos;
    private float time = 0f;
    private Vector3 bezMiddlePoint;
    private bool firstPosition = true;
    private bool movingLeg = false;

    private void Start()
    {
        pos = transform.position;
        StartCoroutine(SetStartPosition());
    }

    void FixedUpdate()
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
            Vector3 m2 = Vector3.Lerp(bezMiddlePoint, nextTarget.position + posOrNeg*distanceUntilNextTarget/2f, time);
            pos = Vector3.Lerp(m1, m2, time);
            yield return null;
        }
        movingLeg = false;
    }
    
    private IEnumerator SetStartPosition()
    {
        movingLeg = true;
        time = 0f;
        Vector3 startPos = pos;
        bezMiddlePoint = Vector3.Lerp(startPos, nextTarget.position, 0.5f);
        bezMiddlePoint.y += liftHeight;
        Vector3 posOrNeg = Vector3.zero;
        float mag = (startPos - nextTarget.position).x;
        posOrNeg = startFront ? Vector3.left : Vector3.right;
        firstPosition = false;
        while (time <= 1f)
        {
            time += Time.deltaTime*speed;
            
            Vector3 m1 = Vector3.Lerp(startPos, bezMiddlePoint, time);
            Vector3 m2 = Vector3.Lerp(bezMiddlePoint, nextTarget.position + posOrNeg*(distanceUntilNextTarget/2f), time);
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