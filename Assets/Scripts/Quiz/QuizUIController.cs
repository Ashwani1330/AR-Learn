using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class QuizUIController : MonoBehaviour
{
    [Header("UI Document (Quiz Screen)")]
    public UIDocument quizDocument; // assign the UIDocument that has QuizScreen.uxml

    // Optional model context (if the UI should display model name/title)
    [SerializeField] private string modelId = "jet_engine_001";
    [SerializeField] private string modelName = "Jet Engine";

    private VisualElement root;
    private Label questionCounter;
    private Label questionStem;
    private List<Button> optionButtons = new List<Button>();
    private Button nextButton;
    private VisualElement modalContainer; // the panel root - we will show/hide it as a modal

    // Quiz state
    private List<QuizQuestion> questions;
    private int currentIndex = 0;
    private int score = 0;
    private bool answered = false;

    void Awake()
    {
        if (quizDocument == null) Debug.LogError("QuizUIController: quizDocument not set.");

        root = quizDocument.rootVisualElement.Q<VisualElement>("quiz-root");
        if (root == null) Debug.LogError("QuizUIController: quiz-root element not found in UIDocument.");

        questionCounter = root.Q<Label>("question-counter");
        questionStem = root.Q<Label>("question-stem");

        optionButtons.Clear();
        optionButtons.Add(root.Q<Button>("option-1"));
        optionButtons.Add(root.Q<Button>("option-2"));
        optionButtons.Add(root.Q<Button>("option-3"));
        optionButtons.Add(root.Q<Button>("option-4"));

        nextButton = root.Q<Button>("next-btn");
        nextButton.clicked += OnNextClicked;

        // Start hidden (modal)
        root.style.display = DisplayStyle.None;
    }

    // Public getters to provide context to the QuizManager/Api (optional)
    public string GetModelId() => modelId;
    public string GetModelName() => modelName;

    /// <summary>
    /// Start quiz flow with fetched questions (shows modal)
    /// </summary>
    public void StartQuiz(List<QuizQuestion> fetchedQuestions)
    {
        if (fetchedQuestions == null || fetchedQuestions.Count == 0)
        {
            ShowError("No quiz data available.");
            return;
        }

        questions = fetchedQuestions;
        currentIndex = 0;
        score = 0;
        ShowModal(true);
        ShowQuestion();
    }

    private void ShowModal(bool show)
    {
        root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ShowQuestion()
    {
        if (questions == null || questions.Count == 0) return;

        answered = false;
        nextButton.SetEnabled(false);

        var q = questions[currentIndex];
        questionCounter.text = $"Question {currentIndex + 1} of {questions.Count}";
        questionStem.text = q.stem ?? "";

        // Setup option buttons
        for (int i = 0; i < optionButtons.Count; i++)
        {
            var btn = optionButtons[i];
            btn.RemoveFromClassList("correct");
            btn.RemoveFromClassList("wrong");
            btn.SetEnabled(true);

            // Defensive: if options length is less than 4, hide unused buttons
            if (i < q.options.Count)
            {
                btn.style.display = DisplayStyle.Flex;
                btn.text = q.options[i];
                // Remove previous handlers, then add new
                btn.clicked -= () => OnOptionClicked(i); // remove any previous - safe-guard
                int index = i; // capture
                btn.clicked += () => OnOptionClicked(index);
            }
            else
            {
                btn.style.display = DisplayStyle.None;
            }
        }
    }

    private void OnOptionClicked(int selectedIndex)
    {
        if (answered) return;
        answered = true;

        var q = questions[currentIndex];
        bool correct = (selectedIndex == q.correct_index);
        if (correct) score++;

        // Disable all buttons and apply classes
        for (int i = 0; i < optionButtons.Count; i++)
        {
            var b = optionButtons[i];
            b.SetEnabled(false);

            if (i == q.correct_index)
                b.AddToClassList("correct");
            else if (i == selectedIndex)
                b.AddToClassList(correct ? "correct" : "wrong");
        }

        // Optionally show explanation (if present) by appending to questionStem or a dedicated label
        if (!string.IsNullOrEmpty(q.explanation))
        {
            // Append explanation below question (simple approach)
            questionStem.text = q.stem + "\n\n" + "<i>Explanation:</i> " + q.explanation;
        }

        nextButton.SetEnabled(true);
    }

    private void OnNextClicked()
    {
        // Move next, or finish and show results
        if (currentIndex < questions.Count - 1)
        {
            currentIndex++;
            ShowQuestion();
        }
        else
        {
            ShowResults();
        }
    }

    private void ShowResults()
    {
        // Simple results modal: you can replace with a dedicated result UI
        ShowModal(false);

        // Use Unity UI Toolkit or UnityEngine.UI for results — here we simply log + use a quick popup
        Debug.Log($"Quiz finished: Score {score}/{questions.Count}");

        // You can expand: show a nice UI result window — but for now:
        // - call back into any other manager for analytics
        // - display a quick native popup (or use another UIDocument)
        ShowSimpleResultOverlay($"You scored {score}/{questions.Count}");
    }

    private void ShowSimpleResultOverlay(string msg)
    {
        // Create a simple one-off UI overlay to show result (transient)
        var overlay = new VisualElement();
        overlay.style.position = Position.Absolute;
        overlay.style.left = 0;
        overlay.style.top = 0;
        overlay.style.right = 0;
        overlay.style.bottom = 0;
        overlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.6f));
        overlay.style.alignItems = Align.Center;
        overlay.style.justifyContent = Justify.Center;

        var box = new VisualElement();
        box.style.width = Length.Percent(80);
        box.style.paddingTop = 20;
        box.style.paddingBottom = 20;
        box.style.paddingLeft = 18;
        box.style.paddingRight = 18;
        box.style.backgroundColor = new StyleColor(Color.white);
        box.style.borderTopLeftRadius = 8;
        box.style.borderTopRightRadius = 8;
        box.style.borderBottomLeftRadius = 8;
        box.style.borderBottomRightRadius = 8;

        var label = new Label(msg);
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.fontSize = 18;
        label.style.marginBottom = 10;

        var closeBtn = new Button(() => { overlay.RemoveFromHierarchy(); }) { text = "Close" };
        closeBtn.style.alignSelf = Align.Center;
        closeBtn.style.marginTop = 8;
        closeBtn.style.paddingLeft = 12;
        closeBtn.style.paddingRight = 12;

        box.Add(label);
        box.Add(closeBtn);
        overlay.Add(box);

        // attach to root of quizDocument (top-level)
        quizDocument.rootVisualElement.Add(overlay);
    }

    // Allow outside scripts to show a small error message on the quiz UI
    public void ShowError(string message)
    {
        Debug.LogWarning("QuizUIController: " + message);
        // quick overlay approach:
        ShowSimpleResultOverlay(message);
    }
}
