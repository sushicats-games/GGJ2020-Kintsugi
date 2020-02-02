using System;
using UnityEngine;
using UnityTemplateProjects;

public class ObjectPicker : MonoBehaviour
{
    public AudioClip OnPick;
    public AudioClip OnRelease;
    private SimpleCameraController cameraController;
    new Camera camera;
    private Rigidbody target;
    private float hitDistance = 0;
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private Vector3 targetDesiredPosition;
    private Quaternion desiredRotation;
    public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));



    // Start is called before the first frame update
    void Start()
    {
        cameraController = GetComponent<SimpleCameraController>();
        camera = GetComponent<Camera>();
    }


    private class InputState
    {
        internal bool pickObject;
        internal bool releaseObject;
        internal Vector3 currentMousePosition;
        internal bool fineControl;
        internal Vector3 keyboardTranslation;
        internal Vector3 keyboardRotation;
    }

    // Update is called once per frame
    void Update()
    {
        var input = GetInputState();


        // READ INPUT ///////////////////////////////////

        if (input.pickObject) // left click
        {
            target = null;
            if (TryGetObjectAtMouse(out var hit))
            {
                if (!hit.rigidbody.isKinematic)
                {
                    cameraController.dontMove = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    target = hit.rigidbody;
                    hitDistance = hit.distance;
                    targetDesiredPosition = target.position;
                    desiredRotation = target.rotation;
                    isDragging = true;
                    target.GetComponent<AudioSource>().PlayOneShot(OnPick);
                }
            }
        }

        if (input.releaseObject) // release left click
        {
            cameraController.dontMove = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            isDragging = false;
            if(target != null)
            {
                target.GetComponent<AudioSource>().PlayOneShot(OnRelease);
            }
            target = null;
        }
        if (target != null)
        {
            var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude*0.25f);
            mouseSensitivityFactor *= 0.1f;
            mouseMovement *= mouseSensitivityFactor;

            var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / .5f) * Time.deltaTime);
            float amount = input.fineControl ? 0.1f : 1.0f;

            float rotationSpeed = 180.0f * amount;
            float rotateX = input.keyboardRotation.y * rotationSpeed;
            float rotateY = -input.keyboardRotation.x * rotationSpeed;
            float rotateZ = input.keyboardRotation.z * rotationSpeed;

            if (Input.GetMouseButton(1))
            {
                rotateX += mouseMovement.y * rotationSpeed;
                rotateY += -mouseMovement.x * rotationSpeed;
            }
            else
            {
                float moveZ = Input.mouseScrollDelta.y * .05f;
                hitDistance += moveZ;
                targetDesiredPosition += camera.transform.rotation * Vector3.forward * moveZ * amount;
                targetDesiredPosition += camera.transform.rotation * mouseMovement * .5f * hitDistance * amount;
            }

            desiredRotation = Quaternion.Inverse(camera.transform.rotation) * desiredRotation;
            desiredRotation = Quaternion.Euler(rotateX, rotateY, rotateZ) * desiredRotation;
            desiredRotation = camera.transform.rotation * desiredRotation;

            targetDesiredPosition += camera.transform.rotation * input.keyboardTranslation * amount;

            target.angularVelocity = Vector3.zero;
            target.velocity = (targetDesiredPosition - target.position)*10.0f;
            target.rotation = Quaternion.Lerp(target.rotation, desiredRotation, rotationLerpPct);
        }

        lastMousePosition = input.currentMousePosition;
    }

    private InputState GetInputState()
    {
        var state = new InputState
        {
            pickObject = Input.GetMouseButtonDown(0),
            releaseObject = Input.GetMouseButtonUp(0) || (target != null && target.isKinematic),
            currentMousePosition = Input.mousePosition,
            fineControl = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift),
            keyboardTranslation = GetKeyboardTranslation() * Time.deltaTime * 0.25f,
            keyboardRotation = GetKeyboardRotation() * Time.deltaTime,
        };
        return state;
    }

    Vector3 GetKeyboardTranslation()
    {
        Vector3 direction = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector3.right;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            direction += Vector3.down;
        }
        if (Input.GetKey(KeyCode.E))
        {
            direction += Vector3.up;
        }
        return direction;
    }

    Vector3 GetKeyboardRotation()
    {
        Vector3 direction = new Vector3();
        if (Input.GetKey(KeyCode.T))
        {
            direction += Vector3.up;
        }
        if (Input.GetKey(KeyCode.G))
        {
            direction += Vector3.down;
        }
        if (Input.GetKey(KeyCode.F))
        {
            direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.H))
        {
            direction += Vector3.right;
        }
        if (Input.GetKey(KeyCode.R))
        {
            direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.Y))
        {
            direction += Vector3.back;
        }
        return direction;
    }

    bool TryGetObjectAtMouse(out RaycastHit hit)
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.DrawLine(transform.position, hit.point, Color.yellow);
            return true;
        }
        return false;
    }
}
