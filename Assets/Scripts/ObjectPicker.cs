using UnityEngine;
using UnityTemplateProjects;

public class ObjectPicker : MonoBehaviour
{
    new Camera camera;
    private SimpleCameraController cameraController;
    private Rigidbody target;
    private float hitDistance = 0;
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private Vector3 targetDesiredPosition;
    private Quaternion desiredRotation;


    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        cameraController = GetComponent<SimpleCameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left click
        {
            target = null;
            if (TryGetObjectAtMouse(out var hit))
            {
                if (!hit.rigidbody.isKinematic)
                {
                    target = hit.rigidbody;
                    hitDistance = hit.distance;
                    targetDesiredPosition = target.position;
                    desiredRotation = target.rotation;
                    isDragging = true;
                    cameraController.enabled = false;
                }
            }
        }
        if (Input.GetMouseButtonUp(0) || (target != null && target.isKinematic)) // release left click
        {
            isDragging = false;
            target = null;
            cameraController.enabled = true;
        }
        var currentMousePosition = Input.mousePosition;
        if (target != null)
        {
            var delta = (currentMousePosition - lastMousePosition);
            var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / .5f) * Time.deltaTime);

            if (Input.GetMouseButton(1))
            {
                float rotationSpeed = 180.0f;
                float rotateX = delta.y / camera.pixelHeight * rotationSpeed;
                float rotateY = delta.x / camera.pixelHeight * rotationSpeed;
                desiredRotation = Quaternion.Euler(rotateX, rotateY, 0.0f) * desiredRotation;
            }
            else
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                float moveZ = Input.mouseScrollDelta.y * .5f;
                hitDistance += moveZ;
                targetDesiredPosition += camera.transform.rotation * Vector3.forward * moveZ;
                targetDesiredPosition += camera.transform.rotation * delta / camera.pixelHeight * 1.2f * hitDistance;
            }

            target.angularVelocity = Vector3.zero;
            target.velocity = (targetDesiredPosition - target.position)*10.0f;
            target.rotation = Quaternion.Lerp(target.rotation, desiredRotation, rotationLerpPct);
        }

        lastMousePosition = currentMousePosition;
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
