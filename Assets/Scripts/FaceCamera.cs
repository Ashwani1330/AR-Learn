using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Cache the main camera for efficiency
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found in the scene. Please ensure a camera is tagged as 'MainCamera'.");
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Make the canvas face the camera
            transform.LookAt(transform.position + mainCamera.transform.forward, mainCamera.transform.up);
        }
    }
}