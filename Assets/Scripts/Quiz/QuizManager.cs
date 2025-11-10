using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class QuizManager : MonoBehaviour
{
    [Header("References")]
    public QuizAPIService quizApiService;         // set in inspector
    public QuizUIController quizUIController;     // set in inspector (controls the UIDocument)
    public UIDocument uiDocumentRoot;             // root UIDocument that contains the quiz button (or assign root that contains the quiz button)

    [Header("Quiz options")]
    public int numQuestions = 5;
    public string difficulty = "beginner";

    // Default quiz button name in UI Toolkit (change if your button uses another name)
    [Tooltip("Name of the Quiz start button inside the UIDocument. Change if different.")]
    public string quizButtonName = "quiz-button";

    // Loader element created dynamically
    private VisualElement loaderOverlay;
    private Button quizButton;

    void Awake()
    {
        if (quizApiService == null) Debug.LogWarning("QuizAPIService not assigned.");
        if (quizUIController == null) Debug.LogWarning("QuizUIController not assigned.");
        if (uiDocumentRoot == null) Debug.LogWarning("uiDocumentRoot not assigned - needed to find quiz button.");

        // Try find button inside provided UIDocument
        if (uiDocumentRoot != null)
        {
            var root = uiDocumentRoot.rootVisualElement;
            quizButton = root.Q<Button>(quizButtonName);
            if (quizButton == null)
            {
                Debug.LogWarning($"Quiz button with name '{quizButtonName}' not found in UIDocument. Please set correct name.");
            }
            else
            {
                quizButton.clicked += OnQuizButtonPressed_UIButton; // hook UI button if present
            }
        }

        CreateLoaderOverlay(); // prepare loader (hidden)
    }

    /// <summary>
    /// Public method you can call from other scripts (eg JetSpawner or APIManager) to start quiz.
    /// </summary>
    public void OnQuizButtonPressed()
    {
        StartCoroutine(StartQuizFlow());
    }

    // Also wired to UI Toolkit button (if found)
    private void OnQuizButtonPressed_UIButton()
    {
        OnQuizButtonPressed();
    }

    private IEnumerator StartQuizFlow()
    {
        // disable quiz button (if found) and show loader
        if (quizButton != null) quizButton.SetEnabled(false);
        ShowLoader(true);

        // call API
        bool finished = false;
        List<QuizQuestion> fetched = null;
        string errorMsg = null;

        yield return StartCoroutine(quizApiService.FetchQuiz(
            modelId: quizUIController != null ? quizUIController.GetModelId() : null,
            modelName: quizUIController != null ? quizUIController.GetModelName() : null,
            onSuccess: (qs) => { fetched = qs; finished = true; },
            onError: (err) => { errorMsg = err; finished = true; },
            numQuestions: numQuestions,
            difficulty: difficulty
        ));

        // Hide loader, re-enable button
        ShowLoader(false);
        if (quizButton != null) quizButton.SetEnabled(true);

        if (!string.IsNullOrEmpty(errorMsg))
        {
            Debug.LogError("Quiz API error: " + errorMsg);
            // Optionally show a toast in UI — using quizUIController
            quizUIController?.ShowError($"Failed to load quiz: {errorMsg}");
            yield break;
        }

        if (fetched == null || fetched.Count == 0)
        {
            quizUIController?.ShowError("No questions returned by the server.");
            yield break;
        }

        // Start the quiz UI (modal pop-up)
        quizUIController?.StartQuiz(fetched);
    }

    #region Loader Overlay Helpers

    private void CreateLoaderOverlay()
    {
        // We'll create a simple overlay VisualElement and parent it to the UIDocument root (if available)
        if (uiDocumentRoot == null) return;
        var root = uiDocumentRoot.rootVisualElement;

        loaderOverlay = new VisualElement();
        loaderOverlay.name = "quiz-loader-overlay";
        loaderOverlay.style.position = Position.Absolute;
        loaderOverlay.style.left = 0;
        loaderOverlay.style.top = 0;
        loaderOverlay.style.right = 0;
        loaderOverlay.style.bottom = 0;
        loaderOverlay.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.55f));
        loaderOverlay.style.justifyContent = Justify.Center;
        loaderOverlay.style.alignItems = Align.Center;
        loaderOverlay.style.display = DisplayStyle.None;

        var label = new Label("Generating quiz...");
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.color = Color.white;
        label.style.fontSize = 18;
        loaderOverlay.Add(label);

        root.Add(loaderOverlay);
    }

    private void ShowLoader(bool show)
    {
        if (loaderOverlay == null) return;
        loaderOverlay.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    #endregion
}
