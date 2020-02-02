using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shard : MonoBehaviour
{
    public AudioClip[] OnSnap;
    private Rigidbody body;
    private AudioSource audioSource;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private float delay = 1.0f;
    private float positionThreshold = 0.01f;
    private float rotationThreshold = 30.0f;
    private float interpolating = -1.0f;

    private static int onSnapClipIndex = 0;


    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
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
            float rotationDiff 
                = Quaternion.Angle(initialRotation, transform.rotation);

            if (rotationDiff <= rotationThreshold)
            {
                PlaySnapSFX();
                body.isKinematic = true;
                body.useGravity = false;
                interpolating = 1.0f;
            }
        }
    }

    private void PlaySnapSFX()
    {
        audioSource.PlayOneShot(OnSnap[onSnapClipIndex]);
        onSnapClipIndex = (onSnapClipIndex + 1) % OnSnap.Length;
    }
}
