using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SpiderBehaviour : MonoBehaviour
{
    public int smoothing = 6;
    public float stepSize = 1f;
    public float stepHeight = 1f;
    public float legSpeed = 2f;
    public List<Transform> legTargets = new List<Transform>();

    private List<Vector3> defaultLegPositions;
    private List<Vector3> latestLegPositions;
    
    private Vector3 velocity;
    private Vector3 lastVelocity;
    [SerializeField] private float velocityMultiplier = 2f;
    private Vector3 lastBodyPosition;
    private Vector3 lastBodyUp;
    private float raycastRange = 3f;
    private bool movingLeg = false;
    private int legAmount = 0;
    private int legToMove = -1;
    private int secondLegToMove = -1;
    private bool haveResetLegs = false;
    private void Start()
    {
        legAmount = legTargets.Count;
        defaultLegPositions = new List<Vector3>(legAmount);
        latestLegPositions = new List<Vector3>(legAmount);
        foreach (var leg in legTargets)
        {
            defaultLegPositions.Add(leg.localPosition);
            latestLegPositions.Add(leg.position);
        }
        
        lastBodyUp = transform.up;
        lastBodyPosition = transform.position;
    }

    private void FixedUpdate()
    {
        velocity = transform.position - lastBodyPosition;
        velocity = (velocity + smoothing * lastVelocity) / (smoothing + 1f);
        if (velocity.magnitude < 0.000025f)
        {
            velocity = lastVelocity;
            StartCoroutine(ResetLegs());
        }
        lastVelocity = velocity;
        legToMove = -1;
        secondLegToMove = -1;
        float maxStep = stepSize;
        List<Vector3> newPosition = new List<Vector3>();

        for (int i = 0; i < legAmount; i++)
        {
            newPosition.Add(transform.TransformPoint(defaultLegPositions[i]));
            Ray ray = new Ray(newPosition[i] + ((raycastRange/2) * lastBodyUp) + velocity * velocityMultiplier, -lastBodyUp);
            Debug.DrawRay(ray.origin, ray.direction);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, raycastRange, layerMask:LayerMask.GetMask("Ground")))
            {
                float distance = Vector3.Distance(legTargets[i].position, hit.point);
                
                if (distance > maxStep)
                {
                    maxStep = distance;

                    legToMove = i;                        
                    newPosition[i] = hit.point;
                }
                else if(distance > stepSize && legToMove + 1 == i && legToMove != -1 && i % 2 == 0)
                {
                    secondLegToMove = i;
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

        if (legToMove != -1 && !movingLeg)
        {
            movingLeg = true;
            StartCoroutine(MoveLeg(legToMove, newPosition[legToMove]));
            if (secondLegToMove == -2)
            {
                StartCoroutine(MoveLeg(secondLegToMove, newPosition[secondLegToMove]));
            }
            haveResetLegs = false;
        }
        
        lastBodyPosition = transform.position;
        /*Vector3 v1 = legsList[0].position - legsList[1].position;
        Vector3 v2 = legsList[2].position - legsList[3].position;
        Vector3 normal = Vector3.Cross(v1, v2).normalized;
        Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f / (smoothing + 1));
        transform.up = up;
        lastBodyUp = up;*/
    }

    private IEnumerator ResetLegs()
    {
        if (haveResetLegs)
            yield return null;
        
        int currentLeg = 0;
        
        while (currentLeg < legAmount)
        {
            if (!movingLeg)
            {
                movingLeg = true;
                StartCoroutine(MoveLeg(currentLeg, transform.TransformPoint(defaultLegPositions[currentLeg])));
                currentLeg++;
                yield return new WaitForFixedUpdate();
            }
        }
        haveResetLegs = true;
    }
    
    private IEnumerator MoveLeg(int index, Vector3 newPoint)
    {
        float time = 0f;
        Vector3 bezMiddlePoint = Vector3.Lerp(legTargets[index].position, newPoint, 0.5f);
        bezMiddlePoint.y += stepHeight;
        while (time < 1f)
        {
            time += Time.deltaTime * legSpeed;

            Vector3 m1 = Vector3.Lerp(legTargets[index].position, bezMiddlePoint, time);
            Vector3 m2 = Vector3.Lerp(bezMiddlePoint, newPoint, time);
            legTargets[index].position = Vector3.Lerp(m1, m2, time);
            yield return new WaitForFixedUpdate();
        }

        legTargets[index].position = newPoint;
        latestLegPositions[index] = legTargets[index].position;
        movingLeg = false;
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