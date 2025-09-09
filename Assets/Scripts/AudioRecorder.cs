using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles recording audio from the microphone when a UI button is held down.
/// Requires an EventTrigger component on the same button to work.
/// </summary>
public class AudioRecorder : MonoBehaviour
{
    [Tooltip("The APIManager script that will receive the recorded audio clip.")]
    public APIManager apiManager;

    private AudioClip recording;
    private string microphoneDevice;
    private const int RECORD_TIME_SECONDS = 10; // Max recording duration

    void Start()
    {
        // Get the default microphone device
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("No microphone found!");
            gameObject.SetActive(false); // Disable the record button if no mic
        }
    }

    // This function will be called by the EventTrigger's 'PointerDown' event
    public void OnPointerDown()
    {
        Debug.Log("Recording started...");
        recording = Microphone.Start(microphoneDevice, false, RECORD_TIME_SECONDS, 44100);
    }

    // This function will be called by the EventTrigger's 'PointerUp' event
    public void OnPointerUp()
    {
        Debug.Log("Recording stopped.");
        Microphone.End(microphoneDevice);

        if (recording != null && apiManager != null)
        {
            // Send the completed audio clip to the APIManager
            apiManager.SendAudioQuestion(recording);
        }
    }
}