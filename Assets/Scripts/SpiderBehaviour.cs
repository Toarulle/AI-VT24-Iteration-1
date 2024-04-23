using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBehaviour : MonoBehaviour
{
    [SerializeField] private int smoothing = 6;
    [SerializeField] private float stepSize = 1f;
    [SerializeField] private float stepHeight = 1f;
    [SerializeField] private float legSpeed = 2f;
    [SerializeField] private List<Transform> legTargets = new List<Transform>();
    [SerializeField] private int footRayAmount;
    [SerializeField] private float footRayLength;
    [SerializeField] private float footRayAngle;
    [SerializeField] private float footRayOffset;
    [SerializeField] private bool showGizmoFootStepSphere = true;
    [SerializeField] private bool showGizmoFootNewPos = true;
    [SerializeField] private bool showGizmoFootNewPosRayAngle = true;
    [SerializeField] private AudioClip footstep;
    
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
    private int legToReset = -1;
    private int secondLegToReset = -1;
    private GameManager gameManager;
    
    private void Start()
    {
        legAmount = legTargets.Count;
        defaultLegPositions = new List<Vector3>(legAmount);
        latestLegPositions = new List<Vector3>(legAmount);
        lastUpVector = GetComponentInParent<SpiderMovement>().UpVector;
        lastBodyPosition = transform.position;
        foreach (var leg in legTargets)
        {
            defaultLegPositions.Add(leg.localPosition);
            latestLegPositions.Add(leg.position);
        }

        gameManager = FindObjectOfType<GameManager>();
    }

    private Vector3 GetNewFootPosition(Vector3 newPos, int rayAmount, Vector3 forward, Vector3 up, float angle, float length, float offset, out bool grounded)
    {
        Vector3 pos = Vector3.zero;
        List<Vector3> directions = new List<Vector3>();
        Vector3 right = Vector3.Cross(up, forward);
        grounded = false;
        
        int hitAmount = 0;
        float angleStep = (360 / rayAmount);
        
        for (int i = 0; i < rayAmount; i++)
        {
            float theta = i * angleStep;
            directions.Add(-up + (right * Mathf.Sin(theta * Mathf.Deg2Rad) + forward * Mathf.Cos(theta * Mathf.Deg2Rad)) * angle);
        }
        foreach (var direction in directions)
        {
            Vector3 projection = Vector3.ProjectOnPlane(direction, up);
            RaycastHit hit;
            Ray ray = new Ray(newPos - (direction + projection) * length/2 + projection.normalized * offset / 2, direction);
            if (showGizmoFootNewPos)
                Debug.DrawRay(ray.origin, ray.direction * length, Color.blue);
            if (Physics.SphereCast(ray.origin, stepSize/2, ray.direction, out hit, length, LayerMask.GetMask("Ground")))
            {
                pos += hit.point;
                hitAmount++;
                grounded = true;
            }
        }

        if (grounded)
            pos /= hitAmount;
        return pos;
    }

    private bool AllLegsCloseToBody()
    {
        bool allLegsCloseby = true;
        for (var i = 0; i < legAmount; i++)
        {
            float distance = Vector3.Distance(latestLegPositions[i], transform.TransformPoint(defaultLegPositions[i]));
            allLegsCloseby = distance < stepSize;
        }
        return allLegsCloseby;
    }
    
    private void FixedUpdate()
    {
        velocity = transform.position - lastBodyPosition;
        velocity = (velocity + smoothing * lastVelocity) / (smoothing + 1f);
        if (velocity.magnitude < 0.000025f)
        {
            velocity = lastVelocity;
            if (!haveResetAllLegs && !shouldResetLegs && AllLegsCloseToBody())
            {
                //shouldResetLegs = true;
                Debug.Log("All legs close!");
                haveResetAllLegs = true;
            }
        }
        
        lastVelocity = velocity;
        legToMove = -1;
        float maxStep = stepSize;
        List<Vector3> newPosition = new List<Vector3>();

        for (int i = 0; i < legAmount; i++)
        {
            // newPosition.Add(transform.TransformPoint(defaultLegPositions[i]));
            // Ray ray = new Ray(newPosition[i] + ((raycastRange/2) * lastUpVector) + (velocity.magnitude * velocityMultiplier)*(newPosition[i] - legTargets[i].position), -transform.up);
            // if(showGizmoFootNewPos)
            //     Debug.DrawRay(ray.origin, ray.direction*raycastRange, Color.red);
            // RaycastHit hit;
            // bool hitGround;
            // Vector3 pos = GetNewFootPosition(newPosition[i]+velocity * velocityMultiplier, footRayAmount, transform.forward, transform.up, footRayAngle, footRayLength, footRayOffset, out hitGround);
            // if (hitGround)
            // {
            //     float distance = Vector3.Distance(latestLegPositions[i], pos);
            //     if (shouldResetLegs)
            //     {
            //         newPosition[i] = pos;
            //     }
            //     else
            //     {
            //         if (distance > maxStep)
            //         {
            //             maxStep = distance;
            //
            //             legToMove = i;                        
            //             newPosition[i] = pos;
            //         }
            //     }
            // }
            newPosition.Add(transform.TransformPoint(defaultLegPositions[i]));
            float distance = Vector3.ProjectOnPlane(newPosition[i] + velocity * velocityMultiplier - latestLegPositions[i],transform.up).magnitude;
            if (distance > maxStep)
            {
                maxStep = distance;
                legToMove = i;                        
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
            
            Vector3 pos = GetNewFootPosition(newPosition[legToMove]+velocity * velocityMultiplier, footRayAmount, transform.forward, transform.up, footRayAngle, footRayLength, footRayOffset, out bool hitGround);

            movingLeg = true;
            StartCoroutine(MoveLeg(legToMove, pos));

            haveResetAllLegs = false;
        }
        else if (shouldResetLegs && !movingLeg)
        {
            movingLeg = true;
            StartCoroutine(MoveLeg(legToReset, newPosition[legToReset]));
        }
        
        lastBodyPosition = transform.position;

        /*
        Vector3 v1 = legTargets[0].position - legTargets[1].position;
        Vector3 v2 = legTargets[2].position - legTargets[3].position;
        Vector3 normal = Vector3.Cross(v2, v1).normalized;
        Vector3 up = Vector3.Lerp(lastUpVector, normal, 1f / (smoothing + 1));
        transform.up = up;
        transform.rotation = Quaternion.LookRotation(transform.parent.forward, up);
        lastUpVector = transform.up;
        */
        //lastUpVector = transform.parent.up;
    }
    
    private IEnumerator MoveLeg(int index, Vector3 newPoint)
    {
        float time = 0f;
        Vector3 lastLegPos = legTargets[index].position;
        Vector3 bezMiddlePoint = Vector3.Lerp(lastLegPos, newPoint, 0.5f);
        bezMiddlePoint += stepHeight*lastUpVector;
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
        PlayFootStepSound();
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

    private void PlayFootStepSound()
    {
        gameManager.PlaySound(footstep);
    }
    
    private void OnDrawGizmos()
    {
        if (showGizmoFootStepSphere)
            for (var i = 0; i < legAmount; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.TransformPoint(defaultLegPositions[i]),stepSize);
            }
    }
}