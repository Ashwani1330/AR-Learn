using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSceneInitializer : MonoBehaviour
{
    [SerializeField] private ARSession arSession;

    private void OnEnable()
    {
        if (arSession != null)
        {
            Debug.Log("Resetting ARSession...");
            arSession.Reset();
        }
    }
}