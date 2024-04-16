using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 2;
    [SerializeField] private float rotateSpeed = 2;
    
    private float horizontal, vertical, strafe;
    private Vector3 direction, forward, lastPosition;
    private Rigidbody rb;
    private Quaternion lastRotation;
    private SpiderBehaviour spider;
    private Vector3 velocity, lastVelocity;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        spider = GetComponentInChildren<SpiderBehaviour>();
        lastRotation = transform.rotation;
        forward = transform.forward;
        lastPosition = transform.position;
        velocity = new Vector3();
    }

    private void FixedUpdate()
    {
        velocity = (velocity + (transform.position - lastPosition)) / 8;
        if (velocity.magnitude < 0.00025f)
            velocity = lastVelocity;
        lastPosition = transform.position;
        lastVelocity = velocity;
        forward = velocity.normalized;
        
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        strafe = 0;
        strafe = Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.Q) ? -1 :
                 !Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.Q) ? 1 : 0;

        direction = new Vector3(vertical, 0, strafe);
        //rb.AddForce(direction*movementSpeed);
        if (vertical != 0)
        {
            transform.position += transform.forward * (vertical * movementSpeed * Time.fixedDeltaTime);
        }
        if (strafe != 0 && horizontal == 0)
        {
            transform.position += Vector3.Cross(transform.forward, transform.up) * (strafe * movementSpeed * Time.fixedDeltaTime);
        }

        Vector3 upVector = spider.GetUpVector();
        if ((vertical != 0 || horizontal != 0) && strafe == 0)
        {
            transform.position += Vector3.Cross(transform.forward, upVector) * (-horizontal * rotateSpeed * Time.fixedDeltaTime);
        
            //Quaternion q = Quaternion.LookRotation((transform.right + (transform.forward * (horizontal * rotateSpeed * Time.fixedDeltaTime))), upVector);
            
            Debug.Log(forward);
            Quaternion q = Quaternion.LookRotation(Vector3.Cross(transform.forward*-horizontal, upVector), upVector);
            Debug.DrawRay(transform.position + upVector*2, Vector3.Cross(transform.forward*-horizontal, upVector), Color.blue);
            transform.rotation = Quaternion.Lerp(lastRotation, q, 1f/50);
            //transform.Rotate(transform.up, horizontal*rotateSpeed*Time.fixedDeltaTime, Space.World);
        }
        
        lastRotation = transform.rotation;

        //rb.MoveRotation(Quaternion.Euler(0,horizontal*rotateSpeed,0));
    }
}