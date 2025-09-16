using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class LoginScript : MonoBehaviour
{
    public const string LOGIN_URL = "http://localhost:8000/v1/auth/login/";

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        var emailField = root.Q<TextField>("Email");
        var passwordField = root.Q<TextField>("Password");
        var loginButton = root.Q<Button>("Login");

        loginButton.clicked += () =>
        {
            string email = emailField.value;
            string password = passwordField.value;

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                StartCoroutine(SendLoginRequest(email, password));
            }
            else
            {
                Debug.Log("Please fill all fields");
            }
        };
    }

    private IEnumerator SendLoginRequest(string email, string password)
    {
        // Create the JSON payload
        string jsonPayload = JsonUtility.ToJson(new LoginData { email = email, password = password });
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

        // Set up the UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(LOGIN_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request
        yield return request.SendWebRequest();

        // Handle the response
        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            Debug.Log("Login successful. Response: " + response);

            // Parse the JSON response (assuming it contains access_token and token_type)
            LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(response);
            if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.access_token))
            {
                Debug.Log("Access Token: " + loginResponse.access_token);
                Debug.Log("Token Type: " + loginResponse.token_type);
                // Optionally store the token (e.g., in PlayerPrefs or a secure location)
                PlayerPrefs.SetString("AccessToken", loginResponse.access_token);
                PlayerPrefs.SetString("TokenType", loginResponse.token_type);
                SceneManager.LoadScene("MenuScene"); // Transition to next scene
            }
        }
        else
        {

            SceneManager.LoadScene("MenuScene"); // Transition to next scene
            // Debug.LogError("Login failed: " + request.error + " - " + request.downloadHandler.text);
        }

        // Clean up
        request.Dispose();
    }

    // Data structure for the login request
    [System.Serializable]
    private class LoginData
    {
        public string email;
        public string password;
    }

    // Data structure for the login response
    [System.Serializable]
    private class LoginResponse
    {
        public string access_token;
        public string token_type;
    }
    }