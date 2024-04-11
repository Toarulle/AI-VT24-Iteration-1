using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 2;
    [SerializeField] private float rotateSpeed = 2;
    
    private float horizontal, vertical, strafe;
    private Vector3 direction;
    private Rigidbody rb;
    private Quaternion lastRotation;
    private SpiderBehaviour spider;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        spider = GetComponent<SpiderBehaviour>();
        lastRotation = transform.rotation;
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
            transform.position += transform.right * (vertical * movementSpeed * Time.fixedDeltaTime);
        }
        if (strafe != 0)
        {
            transform.position += Vector3.Cross(transform.right, transform.up) * (strafe * movementSpeed * Time.fixedDeltaTime);
        }

        Vector3 upVector = spider.GetUpVector();
        transform.up = upVector;
        if (horizontal == 0)
        {
            transform.rotation = Quaternion.LookRotation(transform.forward, upVector);
        }
        else
        {
            Vector3 dir = transform.right + (transform.forward * horizontal);
            Quaternion q = Quaternion.LookRotation((transform.right + (transform.forward * (horizontal * rotateSpeed * Time.fixedDeltaTime))), upVector);
            Debug.DrawRay(transform.position + upVector*2, dir);
            //transform.rotation = Quaternion.Lerp(lastRotation, q, 1f/8);
            transform.Rotate(transform.up, horizontal*rotateSpeed*Time.fixedDeltaTime, Space.World);
            //transform.rotation *= Quaternion.Euler(0, horizontal * rotateSpeed*Time.fixedDeltaTime, 0);
            //transform.forward = q;
        }
        
        lastRotation = transform.rotation;

        //rb.MoveRotation(Quaternion.Euler(0,horizontal*rotateSpeed,0));
    }
}