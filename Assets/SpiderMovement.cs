using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 2;
    [SerializeField] private float rotateSpeed = 2;
    
    private float horizontal, vertical, strafe;
    private Vector3 direction, forward;
    private Rigidbody rb;
    private Quaternion lastRotation;
    private SpiderBehaviour spider;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        spider = GetComponentInChildren<SpiderBehaviour>();
        lastRotation = transform.rotation;
        forward = transform.forward;
    }

    private void FixedUpdate()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        strafe = 0;
        strafe = Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.Q) ? -1 :
                 !Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.Q) ? 1 : 0;

        direction = new Vector3(vertical, 0, strafe);
        //rb.AddForce(direction*movementSpeed);
        if (vertical != 0)
        {
            transform.position += transform.TransformDirection(transform.forward) * (vertical * movementSpeed * Time.fixedDeltaTime);
        }
        if (strafe != 0)
        {
            transform.position += Vector3.Cross(transform.forward, transform.up) * (strafe * movementSpeed * Time.fixedDeltaTime);
        }

        Vector3 upVector = spider.GetUpVector();
        //transform.up = upVector;
        if (horizontal != 0)
        {
            //transform.position += Vector3.Cross(transform.forward, transform.up) * (horizontal * rotateSpeed * Time.fixedDeltaTime);
        }

        if (horizontal != 0)
        {
            Vector3 dir = transform.right + (forward * horizontal);
            //Quaternion q = Quaternion.LookRotation((transform.right + (transform.forward * (horizontal * rotateSpeed * Time.fixedDeltaTime))), upVector);
            forward = spider.GetVelocity().normalized;
            Quaternion q = Quaternion.LookRotation(forward, transform.up);
            Debug.DrawRay(transform.position + upVector*2, forward, Color.blue);
            transform.rotation = Quaternion.Lerp(lastRotation, q, 1f/8);
            //transform.Rotate(transform.up, horizontal*rotateSpeed*Time.fixedDeltaTime, Space.World);
        }
        
        lastRotation = transform.rotation;

        //rb.MoveRotation(Quaternion.Euler(0,horizontal*rotateSpeed,0));
    }
}