using System;
using UnityEngine;

namespace UnityTemplateProjects
{
    public class SimpleCameraController : MonoBehaviour
    {
        class CameraState
        {
            public float yaw;
            public float pitch;
            public float roll;
            public float distance;
            public Vector3 targetPosition;

            private float minPitch;
            private float maxPitch;
            private float minDistance;
            private float maxDistance;

            public void Init(Transform t, Vector3 targetPosition, float distance, float minPitch, float maxPitch, float minDistance, float maxDistance)
            {
                pitch = t.eulerAngles.x;
                yaw = t.eulerAngles.y;
                roll = t.eulerAngles.z;
                this.distance = distance;
                this.minPitch = minPitch;
                this.maxPitch = maxPitch;
                this.minDistance = minDistance;
                this.maxDistance = maxDistance;
                this.targetPosition = targetPosition;
                EnforceConstraints();
            }

            public void Translate(Vector3 translation)
            {
                Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;
            }

            public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
            {
                target.EnforceConstraints();
                yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
                pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
                roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);
                targetPosition = Vector3.Lerp(targetPosition, target.targetPosition, positionLerpPct);

                distance = Mathf.Lerp(distance, target.distance, positionLerpPct);
            }

            public void UpdateTransform(Transform t)
            {
                t.eulerAngles = new Vector3(pitch, yaw, roll);
                t.position = t.rotation * Vector3.forward * -distance + targetPosition;
            }

            private void EnforceConstraints()
            {
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }
        
        CameraState m_TargetCameraState = new CameraState();
        CameraState m_InterpolatingCameraState = new CameraState();

        public float distance = 10.0f;
        public float minPitch = 10.0f;
        public float maxPitch = 80.0f;
        public float minDistance = 1.0f;
        public float maxDistance = 4.0f;
        public float distanceSteps = 0.1f;
        public Vector3 targetPosition;

        [Header("Movement Settings")]
        [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
        public float boost = 3.5f;

        [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
        public float positionLerpTime = 0.2f;

        [Header("Rotation Settings")]
        [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
        public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

        [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
        public float rotationLerpTime = 0.01f;

        [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
        public bool invertY = false;
        internal bool dontMove;

        public Vector3 lookAtPosition;

        void OnEnable()
        {
            m_TargetCameraState.Init(transform, targetPosition, distance, minPitch, maxPitch, minDistance, maxDistance);
            m_InterpolatingCameraState.Init(transform, targetPosition, distance, minPitch, maxPitch, minDistance, maxDistance);
        }

        
        void Update()
        {
            // Exit Sample  
            if (dontMove)
            {
                return;
            }

            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
				#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false; 
				#endif
            }
            // Hide and lock cursor when right mouse button pressed
            if (Input.GetMouseButtonDown(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            // Unlock and show cursor when right mouse button released
            if (Input.GetMouseButtonUp(1))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            // Rotation
            if (Input.GetMouseButton(1))
            {
                var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));
                
                var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

                m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
                m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
            }

            m_TargetCameraState.distance -= Input.mouseScrollDelta.y * distanceSteps;
            m_TargetCameraState.targetPosition = targetPosition;

            // Framerate-independent interpolation
            // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
            var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
            var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
            m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

            m_InterpolatingCameraState.UpdateTransform(transform);
        }
    }

}