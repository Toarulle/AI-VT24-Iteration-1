using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class SpiderMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 2;
    [SerializeField] private float rotateSpeed = 2;
    [SerializeField] private int amountOfRays = 16;
    [SerializeField] private float rayAngle = 16;
    [SerializeField] private float bodyHeightOffset = 1.4f;
    [SerializeField] private float bodyOffsetOutwardRays = 0.5f;
    [SerializeField] private float bodyOffsetInwardRays = 0.5f;
    
    private float horizontal, vertical, strafe;
    private Vector3 forward, lastPosition, upVector;
    private Quaternion lastRotation;
    private Vector3 velocity, lastVelocity;

    private void Start()
    {
        lastRotation = transform.rotation;
        forward = transform.forward;
        upVector = transform.up;
        lastPosition = transform.position;
        velocity = new Vector3();
    }

    private void FixedUpdate()
    {
        velocity = (7 * velocity + (transform.position - lastPosition)) / 8;
        if (velocity.magnitude < 0.00025f)
            velocity = lastVelocity;
        lastPosition = transform.position;
        lastVelocity = velocity;
        
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        strafe = 0;
        strafe = Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.Q) ? 1 :
                 !Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.Q) ? -1 : 0;

        if (vertical != 0)
        {
            transform.position += transform.forward * (vertical * movementSpeed * Time.fixedDeltaTime);
        }
        if (strafe != 0 && horizontal == 0)
        {
            transform.position += Vector3.Cross(transform.up, transform.forward) * (strafe * movementSpeed * Time.fixedDeltaTime);
        }
        if (horizontal != 0 && strafe == 0)
        {
            transform.position += Vector3.Cross(transform.up, transform.forward) * (horizontal*rotateSpeed * Time.fixedDeltaTime);
        }
        
        if ((vertical != 0 || horizontal != 0) && strafe == 0)
        {
        
            forward = velocity.normalized;

            List<Vector3> posnor = GetBodyPositionAndNormal(transform.position, transform.forward, transform.up, bodyOffsetInwardRays, bodyOffsetOutwardRays, rayAngle, 5f, amountOfRays);

            upVector = posnor[1];

            Debug.Log(posnor[0] + " - " + posnor[1]);
            float diff = posnor[0].y - lastPosition.y;
            if (diff > 0.2 || 0.2 > diff)
            {
                //transform.position += upVector*(diff/(8 + 1));
            }
            transform.position = Vector3.Lerp(lastPosition, posnor[0], 1 / 8f);

            Quaternion q = Quaternion.LookRotation(forward, upVector);
            transform.rotation = Quaternion.Lerp(lastRotation, q, 1/8f);
        }
        
        lastRotation = transform.rotation;
    }

    private List<Vector3> GetBodyPositionAndNormal(Vector3 startPos, Vector3 forward, Vector3 up, float offset1, float offset2, float angle, float length, int numberOfRays)
    {
        List<Vector3> positionAndNormal = new List<Vector3>{startPos, Vector3.up};
        Vector3 right = Vector3.Cross(up, forward);
        List<Vector3> directions = new List<Vector3>();
        int positions = 1;
        int normals = 1;

        float angleStep = (360 / numberOfRays);
        
        for (int i = 0; i < numberOfRays; i++)
        {
            float theta = i * angleStep;
            directions.Add(-up + (right * Mathf.Cos(theta * Mathf.Deg2Rad) + forward * Mathf.Sin(theta * Mathf.Deg2Rad)) * angle);
        }

        foreach (var direction in directions)
        {
            Vector3 projection = Vector3.ProjectOnPlane(direction, up);
            RaycastHit hit;
            Ray ray = new Ray(startPos - (direction + projection) + projection.normalized * offset1 / 2, direction);
            Debug.DrawRay(ray.origin, ray.direction * length, Color.blue);
            if (Physics.Raycast(ray.origin, ray.direction, out hit, length, LayerMask.GetMask("Ground")))
            {
                positionAndNormal[0] += hit.point;
                positionAndNormal[1] += hit.normal;
                positions++;
                normals++;
            }
            
            ray = new Ray(startPos - (direction + projection) + projection.normalized * offset2 / 2, direction);
            Debug.DrawRay(ray.origin, ray.direction * length, Color.green);
            if (Physics.Raycast(ray.origin, ray.direction, out hit, length, LayerMask.GetMask("Ground")))
            {
                positionAndNormal[0] += hit.point;
                positionAndNormal[1] += hit.normal;
                positions++;
                normals++;
            }
        }

        positionAndNormal[0] /= positions;
        positionAndNormal[1] /= normals;
        //positionAndNormal[0] += new Vector3(0, bodyHeightOffset, 0);
        return positionAndNormal;
    }
}