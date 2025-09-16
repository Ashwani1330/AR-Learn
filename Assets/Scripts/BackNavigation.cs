using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem; // New Input System

public class BackNavigation : MonoBehaviour
{
    public ARSession arSession;
    public string previousSceneName = "StartScene";

    public GameObject infoCanvas;
    public GameObject quizCanvas;

    public CanvasToggleManager canvasToggleManager;

    // public CrossPlatformTTS ttsManager;

    void Update()
    {
        // Check Escape key using new Input System
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Check if any UICanvasTag is active
            var allUICanvases = FindObjectsOfType<UICanvasTag>();
            foreach (var canvas in allUICanvases)
            {
                if (canvas.gameObject.activeSelf)
                {
                    /*
                    if (ttsManager != null)
                    {
                        ttsManager.Stop(); // Stop any ongoing TTS before closing the canvas
                    }
                    */
                    canvas.gameObject.SetActive(false);

                    if (canvas.name == "AICanvas" && canvasToggleManager != null)
                        canvasToggleManager.HideAICanvas();

                    return;
                }
            }

            // No UI canvas open -> exit AR session
            Debug.Log("Back button detected in AR Session. Navigating to " + previousSceneName);
            if (arSession != null)
            {
                arSession.Reset(); // Or arSession.enabled = false;
            }
            SceneManager.LoadScene(previousSceneName);
        }
    }
}