using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SugarCube : MonoBehaviour
{
    public UnityAction<SugarCube> OnCollectSugarCube = delegate{};
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float bounceSpeed;
    [SerializeField] private float bounceHeight;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed);
        float newY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight + startPos.y;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnCollectSugarCube.Invoke(this);
        }
    }
}
