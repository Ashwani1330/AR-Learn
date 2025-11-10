using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class QuizQuestion
{
    public string id;
    public string stem;
    public List<string> options;
    public int correct_index;
    public string explanation;
}

public class QuizAPIService : MonoBehaviour
{
    [Tooltip("Set your backend base URL (eg https://your-host.com)")]
    public string backendUrl = "https://4085dbf43b18.ngrok-free.app";

    [Serializable]
    public class GenerateQuizRequest
    {
        public string model_id;
        public string model_name;
        public int num_questions = 5;
        public string difficulty = "beginner";
    }

    [Serializable]
    public class GenerateQuizResponse
    {
        public List<QuizQuestion> questions;
    }

    /// <summary>
    /// Fetch quiz from backend. Call via StartCoroutine(quizApiService.FetchQuiz(...))
    /// onSuccess receives List&lt;QuizQuestion&gt;
    /// onError receives error message
    /// </summary>
    public IEnumerator FetchQuiz(string modelId, string modelName, Action<List<QuizQuestion>> onSuccess, Action<string> onError, int numQuestions = 5, string difficulty = "beginner")
    {
        var reqObject = new GenerateQuizRequest
        {
            model_id = modelId,
            model_name = modelName,
            num_questions = numQuestions,
            difficulty = difficulty
        };

        string json = JsonUtility.ToJson(reqObject);
        string url = $"{backendUrl.TrimEnd('/')}/quiz/generate";

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (req.result == UnityWebRequest.Result.Success)
#else
            if (!req.isNetworkError && !req.isHttpError)
#endif
            {
                try
                {
                    var response = JsonUtility.FromJson<GenerateQuizResponse>(req.downloadHandler.text);
                    onSuccess?.Invoke(response?.questions ?? new List<QuizQuestion>());
                }
                catch (Exception e)
                {
                    onError?.Invoke($"Failed to parse quiz response: {e.Message}");
                }
            }
            else
            {
                string err = req.error;
                // Try to surface backend message if any
                if (!string.IsNullOrEmpty(req.downloadHandler?.text))
                {
                    err += $" | response: {req.downloadHandler.text}";
                }
                onError?.Invoke(err);
            }
        }
    }
}
