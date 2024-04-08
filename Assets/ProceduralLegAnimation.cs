using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralLegAnimation : MonoBehaviour
{
    public float stepSize = 1f;
    public float stepHeight = 0.1f;
    public SpiderBehaviour spider;
    
    private float raycastRange = 6f;
    private Vector3 originalLegPosition;
    private Vector3 lastLegPosition;
    private bool shouldMoveLeg = false;
    private bool movingLeg = false;

    private void Start()
    {
        originalLegPosition = transform.localPosition;
        lastLegPosition = transform.position;
        movingLeg = false;
    }

    private void FixedUpdate()
    {
        Vector3 newPosition = new Vector3();
        shouldMoveLeg = false;
        float maxDistance = stepSize;

        newPosition = spider.transform.TransformPoint(originalLegPosition);
        float distance = Vector3.ProjectOnPlane(newPosition + spider.Velocity*spider.VelocityMultiplier - lastLegPosition, spider.LastBodyUp).magnitude;
        if (distance > maxDistance)
        {
            maxDistance = distance;
            shouldMoveLeg = true;
        }

        if (!shouldMoveLeg)
        {
            transform.position = lastLegPosition;
        }

        if (shouldMoveLeg && !movingLeg)
        {
            Vector3 targetPoint = newPosition +
                                  Mathf.Clamp(spider.Velocity.magnitude * spider.VelocityMultiplier, 0.0f, 1.5f) *
                                  (newPosition - transform.position) + spider.Velocity * spider.VelocityMultiplier;
            List<Vector3> positionAndNormal = new List<Vector3>(MatchToSurfaceFromAbove(targetPoint, raycastRange, spider.LastBodyUp));
            movingLeg = true;
            StartCoroutine(PerformStep(positionAndNormal[0]));
        }
    }

    IEnumerator PerformStep(Vector3 targetPoint)
    {
        float time = 0f;
        Vector3 startPos = lastLegPosition;

        for (int i = 0; i < 4; i++)
        {
            transform.position = Vector3.Lerp(startPos, targetPoint, i/5f);
            transform.position += spider.LastBodyUp * (Mathf.Sin(i/5f * Mathf.PI) * stepHeight);
            yield return new WaitForFixedUpdate();
        }
        
        /*Vector3 bezMiddlePoint = Vector3.Lerp(startPos, targetPoint, 0.5f);
        bezMiddlePoint.y += stepHeight;
        Vector3 posOrNeg = Vector3.zero;
        while (time <= 1f)
        {
            time += Time.deltaTime;
            
            Vector3 m1 = Vector3.Lerp(startPos, bezMiddlePoint, time);
            Vector3 m2 = Vector3.Lerp(bezMiddlePoint, targetPoint, time);
            transform.position = Vector3.Lerp(m1, m2, time);
            yield return new WaitForFixedUpdate();
        }*/

        transform.position = targetPoint;
        lastLegPosition = transform.position;
        movingLeg = false;
    }
    
    List<Vector3> MatchToSurfaceFromAbove(Vector3 point, float halfRange, Vector3 up)
    {
        List<Vector3> res = new List<Vector3>(2);
        RaycastHit hit;
        Ray ray = new Ray(point + halfRange * up, -up);

        if (Physics.Raycast(ray, out hit, 2f * halfRange))
        {
            res.Insert(0, hit.point);
            res.Insert(1, hit.normal);
        }
        else
        {
            res.Insert(0, point);
        }

        return res;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spider.transform.TransformPoint(originalLegPosition), stepSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
    }
}
