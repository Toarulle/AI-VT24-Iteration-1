using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class SpiderProceduralLegAnimation : MonoBehaviour
{
    public List<Transform> legTransforms;
    public float stepSize = 1f;
    public float stepHeight = 1.5f;
    public int smoothing = 5;
    public bool tiltBody = true;

    private float raycastRange = 5f;
    private List<Vector3> originalLegPositions;
    private List<Vector3> lastLegPositions;
    private Vector3 lastBodyUp;
    private bool isALegMoving = false;
    private int legAmount;

    private Vector3 velocity;
    private Vector3 lastVelocity;
    private Vector3 lastBodyPosition;

    private float velocityMultiplier = 5f;

    private List<Vector3> hitPoints = new List<Vector3>();
    private List<Vector3> startPoints = new List<Vector3>();
    
    private void Start()
    {
        lastBodyUp = transform.up;

        legAmount = legTransforms.Count;
        originalLegPositions = new List<Vector3>(legAmount);
        lastLegPositions = new List<Vector3>(legAmount);
        isALegMoving = false;

        for (int i = 0; i < legAmount; i++)
        {
            originalLegPositions.Add(legTransforms[i].localPosition);
            lastLegPositions.Add(legTransforms[i].position);
        }

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

        List<Vector3> newPositions = new List<Vector3>(legAmount);
        int indexToMove = -1;
        float maxDistance = stepSize;
        for (int i = 0; i < legAmount; i++)
        {
            newPositions.Add(transform.TransformPoint(originalLegPositions[i]));

            float distance = Vector3
                .ProjectOnPlane(newPositions[i] + velocity * velocityMultiplier - lastLegPositions[i], transform.up)
                .magnitude;
            if (distance > maxDistance)
            {
                maxDistance = distance;
                indexToMove = i;
            }
        }

        for (int i = 0; i < legAmount; i++)
        {
            if (i != indexToMove)
            {
                legTransforms[i].position = lastLegPositions[i];
            }
        }

        if (indexToMove != -1 && !isALegMoving)
        {
            Vector3 targetPoint = newPositions[indexToMove] +
                                  Mathf.Clamp(velocity.magnitude * velocityMultiplier, 0f, 1.5f) *
                                  (newPositions[indexToMove] - legTransforms[indexToMove].position) +
                                  velocity * velocityMultiplier;
            List<Vector3> positionAndNormalFwd = new List<Vector3>(MatchToSurfaceFromAbove(targetPoint+velocity*velocityMultiplier, raycastRange, (transform.up - velocity * 100).normalized));
            List<Vector3> positionAndNormalBwd = new List<Vector3>(MatchToSurfaceFromAbove(targetPoint+velocity*velocityMultiplier, raycastRange*(1f+velocity.magnitude), (transform.up + velocity * 75).normalized));
            isALegMoving = true;

            if (positionAndNormalFwd[1] == Vector3.zero)
            {
                StartCoroutine(PerformStep(indexToMove, positionAndNormalBwd[0]));    
            }
            else
            {
                StartCoroutine(PerformStep(indexToMove, positionAndNormalFwd[0]));    
            }
            
        }

        lastBodyPosition = transform.position;
        if (legAmount > 3 && tiltBody)
        {
            Vector3 v1 = legTransforms[0].position - legTransforms[1].position;
            Vector3 v2 = legTransforms[2].position - legTransforms[3].position;
            Vector3 normal = Vector3.Cross(v1, v2).normalized;
            Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f / (smoothing + 1));
            transform.up = up;
            transform.rotation = Quaternion.LookRotation(transform.forward, up);
            lastBodyUp = up;
        }
    }

    private IEnumerator PerformStep(int index, Vector3 targetPoint)
    {
        Vector3 startPos = lastLegPositions[index];
        for (int i = 0; i < smoothing; i++)
        {
            legTransforms[index].position = Vector3.Lerp(startPos, targetPoint, i / (smoothing + 1f));
            legTransforms[index].position += transform.up * (Mathf.Sin(i / (smoothing + 1f) * Mathf.PI) * stepHeight);
            yield return new WaitForFixedUpdate();
        }

        legTransforms[index].position = targetPoint;
        lastLegPositions[index] = legTransforms[index].position;
        isALegMoving = false;
    }
    
    List<Vector3> MatchToSurfaceFromAbove(Vector3 point, float halfRange, Vector3 up)
    {
        List<Vector3> res = new List<Vector3>(2);
        res.Add(Vector3.zero);
        res.Add(Vector3.zero);
        RaycastHit hit;
        Ray ray = new Ray(point + halfRange * up /2f, -up);
        
        startPoints.Add(ray.origin);
        if (Physics.Raycast(ray, out hit, 2f * halfRange, layerMask:LayerMask.GetMask("Ground")))
        {
            //hitPoints.Add(hit.point);
            res[0] = hit.point;
            res[1] = hit.normal;
        }
        else
        {
            //hitPoints.Add(point);
            res[0] = point;
        }
        return res;
    }
    
    
    private void OnDrawGizmos()
    {
        for (int i = 0; i < legAmount; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(originalLegPositions[i]), stepSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(legTransforms[i].position, 0.05f);
        }

        for (var i = 0; i < hitPoints.Count; i++)
        {
            Gizmos.DrawLine(startPoints[i],hitPoints[i]);
        }
    }
}
