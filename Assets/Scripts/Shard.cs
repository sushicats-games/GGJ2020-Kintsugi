using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shard : MonoBehaviour
{
    private Rigidbody body;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public float delay = 1.0f;
    public float positionThreshold = 0.5f;
    public float rotationThreshold = 0.3f;

    private float interpolating = -1.0f;


    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (delay >= 0)
        {
            // initial delay allowing the object to settle
            delay -= Time.deltaTime;
            return; 
        }
        else if (!body.isKinematic)
        {
            CheckDifferenceAndSnapIfCloseEnough();
        }
        else if (interpolating >= 0)
        {
            // shard smoothly snaps into it's origianl location
            var amount = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / .2f) * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, initialRotation, amount);
            transform.position = Vector3.Lerp(transform.position, initialPosition, amount);
            interpolating -= Time.deltaTime;

            if (interpolating <= 0.0)
            {
                // finished snapping into original location
                body.isKinematic = true;
                body.useGravity = false;
                transform.rotation = initialRotation;
                transform.position = initialPosition;
                body.position = initialPosition;
                body.rotation = initialRotation;
                body.angularVelocity = Vector3.zero;
                body.velocity = Vector3.zero;
            }
        }
    }

    private void CheckDifferenceAndSnapIfCloseEnough()
    {
        var positionDelta = initialPosition - transform.position;
        var rotationDelta = initialRotation * Quaternion.Inverse(transform.rotation);
        float positionDiff = Vector3.Distance(initialPosition, transform.position);
        // diff += rotationDelta.w * rotationDelta.w;
        if (positionDiff <= positionThreshold)
        {
            float rotationDiff = 0;
            var a = initialRotation;
            var b = transform.rotation;
            rotationDiff += Math.Abs(a.x - b.x);
            rotationDiff += Math.Abs(a.y - b.y);
            rotationDiff += Math.Abs(a.z - b.z);
            rotationDiff += Math.Abs(a.w - b.w);
            if (rotationDiff <= rotationThreshold)
            {
                body.isKinematic = true;
                body.useGravity = false;
                interpolating = 1.0f;
            }
        }
    }
}
