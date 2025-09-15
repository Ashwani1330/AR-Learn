using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;

public class JetSpawner : MonoBehaviour
{
    [Header("AR Managers")]
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;

    [Header("Prefab to Spawn")]
    public GameObject refractionPrefab;

    private GameObject spawnedObject;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public APIManager apiManager;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        if (spawnedObject != null) return;

#if UNITY_EDITOR
        // --- Editor Mode (mouse click) ---
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Debug.Log($"[Editor] Mouse click at: {mousePos}");

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 1f));
            spawnedObject = Instantiate(refractionPrefab, worldPos, Quaternion.identity);
            Debug.Log("[Editor] Spawned prefab at: " + worldPos);

            InitializeAPIManager();
            DisablePlaneDetection();
        }
#else
        // --- Device Mode (AR tap) ---
        var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        if (touches.Count == 0) return;

        var touch = touches[0];
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            Vector2 touchPosition = touch.screenPosition;
            Debug.Log($"[Device] Touch at: {touchPosition}");

            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                spawnedObject = Instantiate(refractionPrefab, hitPose.position, hitPose.rotation);
                spawnedObject.AddComponent<ARAnchor>();

                Debug.Log("[Device] Spawned prefab at: " + hitPose.position);

                InitializeAPIManager();
                DisablePlaneDetection();
            }
            else
            {
                Debug.Log("[Device] Raycast returned no hits");
            }
        }
#endif
    }

    private void InitializeAPIManager()
    {
        if (apiManager != null && spawnedObject != null)
        {
            apiManager.InitializeUIFromPrefab(spawnedObject);
            Debug.Log("[AR] APIManager initialized with spawned prefab's UI.");

            // Rotate the spawned object to face the camera
            RotateToFaceCamera(spawnedObject);
        }
        else
        {
            Debug.LogError("APIManager or spawnedObject is null. Cannot initialize UI.");
        }
    }

    private void RotateToFaceCamera(GameObject targetObject)
    {
        if (Camera.main != null)
        {
            // Calculate the direction from the object to the camera
            Vector3 directionToCamera = Camera.main.transform.position - targetObject.transform.position;
            directionToCamera.y = 0; // Keep the Y rotation flat (optional, remove if you want full 3D rotation)

            // Create a rotation that looks along the direction to the camera
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);

            // Apply the rotation to the object
            targetObject.transform.rotation = targetRotation;

            Debug.Log("[AR] Rotated prefab to face camera at: " + Camera.main.transform.position);
        }
        else
        {
            Debug.LogError("No Main Camera found. Cannot rotate to face camera.");
        }
    }

    private void DisablePlaneDetection()
    {
        // Turn off plane visualization
        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }

        // Stop detecting new planes
        planeManager.enabled = false;

        Debug.Log("[AR] Plane detection disabled and planes hidden.");
    }
}
