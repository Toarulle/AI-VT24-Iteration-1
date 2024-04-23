using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class SpiderMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 2;
    [SerializeField] private float speedMultiplier = 2;
    [SerializeField] private float rotateSpeed = 2;
    [SerializeField] private int amountOfRays = 16;
    [SerializeField] private float rayAngle = 16;
    [SerializeField] private float bodyOffsetOutwardRays = 0.5f;
    [SerializeField] private float bodyOffsetInwardRays = 0.5f;
    [SerializeField] private float raysLength = 5f;
    [SerializeField] private int wideAmountOfRays = 16;
    [SerializeField] private float wideRayAngle = 16;
    [SerializeField] private float wideRayAngleOffset = 45f;
    [SerializeField] private float wideBodyOffset1 = 0.5f;
    [SerializeField] private float wideBodyOffset2 = 0.5f;
    [SerializeField] private float wideRaysLength = 5f;
    [SerializeField] private int lerpSmoothing = 8;
    [SerializeField][Range(1,90)] private float maxTurnAngleDegrees = 15;
    [SerializeField] private Transform headTarget;
    [SerializeField] private bool showBottomGizmosNor = true;
    [SerializeField] private bool showBottomGizmosPos = true;
    [SerializeField] private bool showDirectionGizmos = true;
    
    private float horizontal, vertical;
    private Vector3 forward, lastPosition, upVector;
    private Quaternion lastRotation;
    private Vector3 velocity, lastVelocity;
    private Vector3 headTargetOffset;

    public Vector3 UpVector => upVector;
    private void Start()
    {
        lastRotation = transform.rotation;
        forward = transform.forward;
        upVector = transform.up;
        lastPosition = transform.position;
        velocity = Vector3.zero;
        headTargetOffset = transform.InverseTransformDirection(headTarget.position - transform.position);
    }

    private void FixedUpdate()
    {
        velocity = (lerpSmoothing * velocity + (transform.position - lastPosition)) / (lerpSmoothing+1);
        if (velocity.magnitude < 0.00025f)
            velocity = lastVelocity;
        lastPosition = transform.position;
        lastVelocity = velocity;

        float mult = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            mult = speedMultiplier;
        
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        
        if (vertical != 0)
        {
            transform.position += transform.forward * (vertical * movementSpeed * mult * Time.fixedDeltaTime);
        }

        if (horizontal != 0)
        {
            transform.position += Vector3.Cross(transform.up, transform.forward) * (horizontal * rotateSpeed * Time.fixedDeltaTime);
        }
        TurnHead(horizontal);
        
            List<Vector3> normal = GetBodyPositionAndNormal(transform.position, transform.forward, transform.up, wideBodyOffset1, wideBodyOffset2, wideRayAngle, wideRaysLength, wideAmountOfRays, showBottomGizmosNor, wideRayAngleOffset);
            List<Vector3> posnor = GetBodyPositionAndNormal(transform.position, transform.forward, transform.up, bodyOffsetInwardRays, bodyOffsetOutwardRays, rayAngle, raysLength, amountOfRays, showBottomGizmosPos, 0);
        if (vertical != 0 || horizontal != 0)
        {
            forward = velocity.normalized;
            upVector = normal[1];
            
            transform.position = Vector3.Lerp(lastPosition, posnor[0], 1 / (lerpSmoothing+1f));
            
            int lookingForward = vertical >= 0 ? 1 : -1;
            float angle = Vector3.SignedAngle(transform.forward*lookingForward, forward, upVector);

            if (horizontal != 0)
            {
                Vector3 maxTurn = forward;
                if (angle > maxTurnAngleDegrees)
                {
                    maxTurn = transform.right * Mathf.Sin(maxTurnAngleDegrees * Mathf.Deg2Rad) +
                              transform.forward * (lookingForward * (Mathf.Cos(maxTurnAngleDegrees * Mathf.Deg2Rad)));
                }
                else if (angle < -maxTurnAngleDegrees)
                {
                    maxTurn = transform.right * Mathf.Sin(-maxTurnAngleDegrees * Mathf.Deg2Rad) +
                              transform.forward * (lookingForward * (Mathf.Cos(-maxTurnAngleDegrees * Mathf.Deg2Rad)));
                }
                forward = maxTurn.normalized;
            }
            if (showDirectionGizmos)
            {
                Debug.DrawRay(transform.position+transform.up*3, forward*2, Color.red);
                Debug.DrawRay(transform.position+transform.up*2, transform.forward*lookingForward, Color.magenta);
            }

            if (!(vertical < 0))
            {
                Quaternion q = Quaternion.LookRotation(forward, upVector);
                transform.rotation = Quaternion.Lerp(lastRotation, q, 1/(lerpSmoothing+1f));
            }
            
        }
        lastRotation = transform.rotation;
    }

    private void TurnHead(float horizontal)
    {
        headTarget.position = transform.TransformPoint(headTargetOffset) + transform.right * horizontal;
    }
    
    private List<Vector3> GetBodyPositionAndNormal(Vector3 startPos, Vector3 forward, Vector3 up, float offset1, float offset2, float angle, float length, int numberOfRays, bool showGizmo, float angleOffset)
    {
        List<Vector3> positionAndNormal = new List<Vector3>{startPos, up};
        Vector3 right = Vector3.Cross(up, forward);
        List<Vector3> directions = new List<Vector3>();
        int positionsAndNormalsAmount = 1;
        
        float angleStep = (360 / numberOfRays);
        
        for (int i = 0; i < numberOfRays; i++)
        {
            float theta = i * angleStep+angleOffset;
            directions.Add(-up + (right * Mathf.Cos(theta * Mathf.Deg2Rad) + forward * Mathf.Sin(theta * Mathf.Deg2Rad)) * angle);
        }

        foreach (var direction in directions)
        {
            Vector3 projection = Vector3.ProjectOnPlane(direction, up);
            RaycastHit hit;
            Ray ray = new Ray(startPos - (direction + projection)*length/2 + projection.normalized * offset1 / 2, direction);
            if (showGizmo)
                Debug.DrawRay(ray.origin, ray.direction * length, Color.blue);
            if (Physics.SphereCast(ray.origin, 0.2f, ray.direction, out hit, length, LayerMask.GetMask("Ground")))
            {
                positionAndNormal[0] += hit.point;
                positionAndNormal[1] += hit.normal;
                if (showGizmo)
                    Debug.DrawRay(hit.point, hit.normal * length, Color.magenta);
                positionsAndNormalsAmount++;
            }
            
            ray = new Ray(startPos - (direction + projection)*length/2 + projection.normalized * offset2 / 2, direction);
            if (showGizmo)
                Debug.DrawRay(ray.origin, ray.direction * length, Color.green);
            if (Physics.SphereCast(ray.origin, 0.2f, ray.direction, out hit, length, LayerMask.GetMask("Ground")))
            {
                positionAndNormal[0] += hit.point;
                positionAndNormal[1] += hit.normal;
                if (showGizmo)
                    Debug.DrawRay(hit.point, hit.normal * length, Color.magenta);
                positionsAndNormalsAmount++;
            }
        }

        positionAndNormal[0] /= positionsAndNormalsAmount;
        positionAndNormal[1] /= positionsAndNormalsAmount;
        if (showGizmo)
            Debug.DrawRay(positionAndNormal[0], positionAndNormal[1] * length, Color.red);
        return positionAndNormal;
    }
}