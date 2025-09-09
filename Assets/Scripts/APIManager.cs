using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System;
using System.IO;
using System.Text;

/// <summary>
/// Manages all communication with the backend AI tutor.
/// Handles both text-based and audio-based requests.
/// </summary>
public class APIManager : MonoBehaviour
{
    [Header("Backend Configuration")]
    [Tooltip("The local IP address of the computer running the backend server.")]
    public string backendUrl = "https://4085dbf43b18.ngrok-free.app"; // IMPORTANT: Set your computer's IP here

    [Header("UI Elements")]
    public GameObject infoPanel;
    public TextMeshProUGUI partNameText;
    public TextMeshProUGUI descriptionText;

    [Header("Audio Components")]
    [Tooltip("The AudioSource used to play the AI's spoken reply.")]
    public AudioSource replyAudioSource;

    private string currentPartContext = "";

    #region Public Interface

    /// <summary>
    /// Called when a part is tapped. Shows the info panel and sets the context.
    /// </summary>
    public void SetPartContext(string partName)
    {
        currentPartContext = partName;
        if (infoPanel.activeSelf && partNameText.text == partName)
        {
            HideInfoPanel();
        }
        else
        {
            ShowInfoPanel(partName, "Tap the microphone to ask a question about the " + partName + "...");
        }
    }

    /// <summary>
    /// Starts the process of sending a recorded audio clip to the backend.
    /// </summary>
    public void SendAudioQuestion(AudioClip clip)
    {
        if (string.IsNullOrEmpty(currentPartContext))
        {
            Debug.LogError("Cannot send audio question: No part context is set.");
            return;
        }

        UpdateDescription("Thinking...");

        StartCoroutine(PostAudioRequestCoroutine(clip));
    }

    #endregion

    #region Web Request Coroutines

    private IEnumerator PostAudioRequestCoroutine(AudioClip clip)
    {
        // Use the WavUtility to convert the AudioClip to a WAV byte array, then to Base64
        byte[] wavData = WavUtility.FromAudioClip(clip);
        string base64Audio = Convert.ToBase64String(wavData);

        // This endpoint needs to be created by your backend developer.
        // It should accept audio data, transcribe it, and then proceed like the existing endpoint.
        string url = backendUrl + "/qa/ask-about-part-audio";

        // Create the JSON payload for the audio request
        AskAboutPartAudioRequest payload = new AskAboutPartAudioRequest(currentPartContext, base64Audio);
        string jsonPayload = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = CreatePostRequest(url, jsonPayload))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API Error: " + request.error);
                UpdateDescription("Error: Could not connect to the AI tutor.");
            }
            else
            {
                Debug.Log("API Response: " + request.downloadHandler.text);

                // Parse the response which contains both text and audio data
                AskAboutPartAudioResponse response = JsonUtility.FromJson<AskAboutPartAudioResponse>(request.downloadHandler.text);
                UpdateDescription(response.response_text);

                // If the response includes an audio reply, decode it and play it
                if (!string.IsNullOrEmpty(response.audio_reply) && replyAudioSource != null)
                {
                    byte[] audioBytes = Convert.FromBase64String(response.audio_reply);
                    AudioClip replyClip = WavUtility.ToAudioClip(audioBytes);
                    if (replyClip != null)
                    {
                        replyAudioSource.clip = replyClip;
                        replyAudioSource.Play();
                    }
                }
            }
        }
    }

    private UnityWebRequest CreatePostRequest(string url, string jsonPayload)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    #endregion

    #region UI Helper Methods

    public void ShowInfoPanel(string partName, string description)
    {
        if (infoPanel == null) return;
        partNameText.text = partName;
        descriptionText.text = description;
        infoPanel.SetActive(true);
    }

    public void HideInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    public void UpdateDescription(string newDescription)
    {
        if (descriptionText != null)
        {
            descriptionText.text = newDescription;
        }
    }

    #endregion
}


#region JSON Data Transfer Objects (DTOs)

/// <summary>
/// JSON structure for sending an audio question to the backend.
/// </summary>
[System.Serializable]
public class AskAboutPartAudioRequest
{
    public string part_name;
    public string audio_data; // The user's question, as Base64 encoded WAV audio

    public AskAboutPartAudioRequest(string name, string audio)
    {
        this.part_name = name;
        this.audio_data = audio;
    }
}

/// <summary>
/// JSON structure for receiving a response that includes text and spoken audio.
/// </summary>
[System.Serializable]
public class AskAboutPartAudioResponse
{
    public string response_text; // The AI's written reply
    public string audio_reply;   // The AI's spoken reply, as Base64 encoded WAV audio
}

#endregion


#region Audio Utilities

/// <summary>
/// A utility class to handle conversion between Unity's AudioClip and WAV byte arrays.
/// </summary>
public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        if (clip == null) { return null; }
        using (var memoryStream = new MemoryStream())
        {
            memoryStream.Write(new byte[44], 0, 44);
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            short[] intData = new short[samples.Length];
            byte[] bytesData = new byte[samples.Length * 2];
            float rescaleFactor = 32767;
            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
                byte[] byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }
            memoryStream.Write(bytesData, 0, bytesData.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            byte[] riff = Encoding.UTF8.GetBytes("RIFF"); memoryStream.Write(riff, 0, 4);
            byte[] chunkSize = BitConverter.GetBytes(memoryStream.Length - 8); memoryStream.Write(chunkSize, 0, 4);
            byte[] wave = Encoding.UTF8.GetBytes("WAVE"); memoryStream.Write(wave, 0, 4);
            byte[] fmt = Encoding.UTF8.GetBytes("fmt "); memoryStream.Write(fmt, 0, 4);
            byte[] subChunk1 = BitConverter.GetBytes(16); memoryStream.Write(subChunk1, 0, 4);
            byte[] audioFormat = BitConverter.GetBytes((ushort)1); memoryStream.Write(audioFormat, 0, 2);
            byte[] numChannels = BitConverter.GetBytes((ushort)clip.channels); memoryStream.Write(numChannels, 0, 2);
            byte[] sampleRate = BitConverter.GetBytes(clip.frequency); memoryStream.Write(sampleRate, 0, 4);
            byte[] byteRate = BitConverter.GetBytes(clip.frequency * clip.channels * 2); memoryStream.Write(byteRate, 0, 4);
            byte[] blockAlign = BitConverter.GetBytes((ushort)(clip.channels * 2)); memoryStream.Write(blockAlign, 0, 2);
            byte[] bitsPerSample = BitConverter.GetBytes((ushort)16); memoryStream.Write(bitsPerSample, 0, 2);
            byte[] dataString = Encoding.UTF8.GetBytes("data"); memoryStream.Write(dataString, 0, 4);
            byte[] subChunk2 = BitConverter.GetBytes(clip.samples * clip.channels * 2); memoryStream.Write(subChunk2, 0, 4);
            return memoryStream.ToArray();
        }
    }

    public static AudioClip ToAudioClip(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length < 44) return null;
        int channels = BitConverter.ToInt16(fileBytes, 22);
        int frequency = BitConverter.ToInt32(fileBytes, 24);
        int sampleCount = (fileBytes.Length - 44) / 2 / channels;
        float[] samples = new float[sampleCount * channels];
        for (int i = 0; i < sampleCount * channels; i++)
        {
            short sample = BitConverter.ToInt16(fileBytes, 44 + i * 2);
            samples[i] = sample / 32767.0f;
        }
        AudioClip audioClip = AudioClip.Create("AI_Response", sampleCount, channels, frequency, false);
        audioClip.SetData(samples, 0);
        return audioClip;
    }
}

#endregion
