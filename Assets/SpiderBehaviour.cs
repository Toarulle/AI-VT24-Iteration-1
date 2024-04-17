using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SpiderBehaviour : MonoBehaviour
{
    [SerializeField] private int smoothing = 6;
    [SerializeField] private float stepSize = 1f;
    [SerializeField] private float stepHeight = 1f;
    [SerializeField] private float legSpeed = 2f;
    [SerializeField][Range(0.05f, 1f)] private float tiltMagnitude = 1f;
    [SerializeField] private List<Transform> legTargets = new List<Transform>();
    [SerializeField] private Transform body;
    [SerializeField][Range(0.2f, 5f)] private float bodyHeightOffset = 2f;
    
    private List<Vector3> defaultLegPositions;
    private List<Vector3> latestLegPositions;
    
    private Vector3 velocity;
    private Vector3 lastVelocity;
    [SerializeField] private float velocityMultiplier = 2f;
    private Vector3 lastBodyPosition;
    private Vector3 lastUpVector;
    private float raycastRange = 5f;
    private bool movingLeg = false;
    private int legAmount = 0;
    private int legToMove = -1;
    private bool haveResetAllLegs = true;
    private bool shouldResetLegs = false;
    private int legToReset = 0;
    
    private void Start()
    {
        legAmount = legTargets.Count;
        defaultLegPositions = new List<Vector3>(legAmount);
        latestLegPositions = new List<Vector3>(legAmount);
        lastUpVector = transform.up;
        lastBodyPosition = transform.position;
        foreach (var leg in legTargets)
        {
            defaultLegPositions.Add(leg.localPosition);
            latestLegPositions.Add(leg.position);
        }
    }

    private Vector3 GetNewFootPosition()
    {
        Vector3 pos = new Vector3();
        int hitAmount = 0;

        return pos;
    }
    
    private void FixedUpdate()
    {
        velocity = transform.position - lastBodyPosition;
        velocity = (velocity + smoothing * lastVelocity) / (smoothing + 1f);
        if (velocity.magnitude < 0.000025f)
        {
            velocity = lastVelocity;
            if (!haveResetAllLegs && !shouldResetLegs)
            {
                //shouldResetLegs = true;
            }
        }
        
        lastVelocity = velocity;
        legToMove = -1;
        float maxStep = stepSize;
        List<Vector3> newPosition = new List<Vector3>();

        for (int i = 0; i < legAmount; i++)
        {
            newPosition.Add(transform.TransformPoint(defaultLegPositions[i]));
            Ray ray = new Ray(newPosition[i] + ((raycastRange/2) * lastUpVector) + (velocity.magnitude * velocityMultiplier)*(newPosition[i] - legTargets[i].position), -transform.up);
            Debug.DrawRay(ray.origin, ray.direction*raycastRange, Color.red);
            RaycastHit hit;
            if (Physics.SphereCast(ray, stepSize, out hit, raycastRange, layerMask:LayerMask.GetMask("Ground")))
            {
                float distance = Vector3.Distance(latestLegPositions[i], hit.point);
                if (shouldResetLegs)
                {
                        newPosition[i] = hit.point;
                }
                else
                {
                    if (distance > maxStep)
                    {
                        maxStep = distance;

                        legToMove = i;                        
                        newPosition[i] = hit.point;
                    }
                }
            }
        }

        for (int i = 0; i < legAmount; i++)
        {
            if (i != legToMove)
            {
                legTargets[i].position = latestLegPositions[i];
            }
        }

        if (legToMove != -1 && !movingLeg && !shouldResetLegs)
        {
            movingLeg = true;
            StartCoroutine(MoveLeg(legToMove, newPosition[legToMove]));

            haveResetAllLegs = false;
        }
        else if (shouldResetLegs && !movingLeg)
        {
            movingLeg = true;
            StartCoroutine(MoveLeg(legToReset, newPosition[legToReset]));
        }
        
        float averageFootHeight = 0;
        foreach (var leg in legTargets)
        {
            averageFootHeight += leg.position.y;
        }
        averageFootHeight = (averageFootHeight / legAmount)+bodyHeightOffset;
        float diff = averageFootHeight - lastBodyPosition.y;
        //transform.position += lastUpVector*(diff/(smoothing + 1));
        lastBodyPosition = transform.position;
        
        //Vector3 v2 = legTargets[0].position - legTargets[1].position;
        //Vector3 v1 = legTargets[2].position - legTargets[3].position;
        Vector3 v1 = legTargets[0].position - legTargets[3].position;
        Vector3 v2 = legTargets[0].position - legTargets[2].position;
        Vector3 normal = Vector3.Cross(v1, v2).normalized;
        Debug.DrawRay(transform.position+transform.up, normal*2f, Color.black);
        Debug.DrawRay(transform.position+transform.up, transform.forward*2f, Color.black);
        Vector3 up = Vector3.Lerp(lastUpVector, normal, 1f/(smoothing + 1));
        transform.up = up;
        transform.rotation = Quaternion.LookRotation(transform.parent.forward, up);
        lastUpVector = up;
    }

    public Vector3 GetUpVector()
    {
        // Vector3 v2 = legTargets[0].position - legTargets[1].position;
        // Vector3 v1 = legTargets[2].position - legTargets[3].position;
        // Vector3 normal = Vector3.Cross(v1, v2).normalized;
        // Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f/(smoothing + 1));
        // transform.up = up;
        // transform.rotation = Quaternion.LookRotation(transform.forward, up);
        // lastBodyUp = up;
        return lastUpVector;
    }
    
    private IEnumerator MoveLeg(int index, Vector3 newPoint)
    {
        float time = 0f;
        Vector3 lastLegPos = legTargets[index].position;
        Vector3 bezMiddlePoint = Vector3.Lerp(lastLegPos, newPoint, 0.5f);
        bezMiddlePoint.y += stepHeight;
        while (time < 1f)
        {
            time += Time.deltaTime * legSpeed;

            Vector3 m1 = Vector3.Lerp(lastLegPos, bezMiddlePoint, time);
            Vector3 m2 = Vector3.Lerp(bezMiddlePoint, newPoint, time);
            legTargets[index].position = Vector3.Lerp(m1, m2, time);
            yield return new WaitForFixedUpdate();
        }

        legTargets[index].position = newPoint;
        latestLegPositions[index] = legTargets[index].position;
        movingLeg = false;
        if (shouldResetLegs)
        {
            legToReset++;
            if (legToReset >= legAmount)
            {
                legToReset = 0;
                haveResetAllLegs = true;
                shouldResetLegs = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        for (var i = 0; i < legAmount; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(legTargets[i].position, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(defaultLegPositions[i]),stepSize);
        }
    }
}