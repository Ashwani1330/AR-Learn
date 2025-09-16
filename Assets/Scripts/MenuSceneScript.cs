using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuSceneScript : MonoBehaviour
{
    // Model navigation data - map card name to target scene
    private Dictionary<string, string> modelScenes = new Dictionary<string, string>
    {
        { "model-card-1", "RefractionScene" },
        { "model-card-2", "HeartScene" },
        { "model-card-3", "JetEngineScene" },
        { "model-card-4", "PyramidScene" }
        // Add more models as needed
    };

    // Root + screen references
    private VisualElement root;
    private VisualElement analyticsScreen;
    private VisualElement homeScreen;
    private VisualElement profileScreen;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        // Setup model cards
        SetupModelCards(root);

        // Setup bottom navigation
        SetupBottomNavigation(root);

        // Initialize screens (hide all except home)
        InitializeScreens(root);
    }

    private void SetupModelCards(VisualElement root)
    {
        foreach (var modelCard in modelScenes)
        {
            var cardButton = root.Q<Button>(modelCard.Key);
            if (cardButton != null)
            {
                cardButton.clicked += () =>
                {
                    string targetScene = modelCard.Value;
                    Debug.Log($"Navigating to model scene: {targetScene}");
                    SceneManager.LoadScene(targetScene);
                };
            }
        }
    }

    /*    private void SetupBottomNavigation(VisualElement root)
        {
            var analyticsBtn = root.Q<Button>("analytics-btn");
            var homeBtn = root.Q<Button>("home-btn");
            var profileBtn = root.Q<Button>("profile-btn");

            // Find or create screen containers
            analyticsScreen = root.Q<VisualElement>("analytics-screen");
            homeScreen = root.Q<VisualElement>("home-screen") ?? root.Q<VisualElement>("main-content"); // Use main-content as home if not found
            profileScreen = root.Q<VisualElement>("profile-screen");

            if (analyticsScreen == null) CreateAnalyticsScreen(root);
            if (profileScreen == null) CreateProfileScreen(root);

            // Event handlers
            analyticsBtn.clicked += () => SwitchToScreen("analytics");
            homeBtn.clicked += () => SwitchToScreen("home");
            profileBtn.clicked += () => SwitchToScreen("profile");
        }
    */


    private void SetupBottomNavigation(VisualElement root)
    {
        var analyticsBtn = root.Q<Button>("analytics-btn");
        var homeBtn = root.Q<Button>("home-btn");
        var profileBtn = root.Q<Button>("profile-btn");

        // Find or create screen containers
        analyticsScreen = root.Q<VisualElement>("analytics-screen");
        homeScreen = root.Q<VisualElement>("home-screen") ?? root.Q<VisualElement>("main-content");
        profileScreen = root.Q<VisualElement>("profile-screen");

        if (analyticsScreen == null) CreateAnalyticsScreen(root);
        if (profileScreen == null) CreateProfileScreen(root);

        // Event handlers
        analyticsBtn.clicked += () =>
        {
            Debug.Log("Navigating to BookScanScene from Analytics button");
            SceneManager.LoadScene("BookScanScene");
        };

        homeBtn.clicked += () => SwitchToScreen("home");
        profileBtn.clicked += () => SwitchToScreen("profile");
    }

    private void InitializeScreens(VisualElement root)
    {
        // Default to home screen
        SwitchToScreen("home");
    }

    private void SwitchToScreen(string screenType)
    {
        // Update button states
        var analyticsBtn = root.Q<Button>("analytics-btn");
        var homeBtn = root.Q<Button>("home-btn");
        var profileBtn = root.Q<Button>("profile-btn");

        // Reset all buttons
        analyticsBtn.RemoveFromClassList("active");
        homeBtn.RemoveFromClassList("active");
        profileBtn.RemoveFromClassList("active");

        // Hide all screens
        if (analyticsScreen != null) analyticsScreen.style.display = DisplayStyle.None;
        homeScreen.style.display = DisplayStyle.None;
        if (profileScreen != null) profileScreen.style.display = DisplayStyle.None;

        // Show selected screen and activate button
        switch (screenType)
        {
            case "analytics":
                if (analyticsScreen != null)
                {
                    analyticsScreen.style.display = DisplayStyle.Flex;
                    analyticsBtn.AddToClassList("active");
                }
                break;
            case "home":
                homeScreen.style.display = DisplayStyle.Flex;
                homeBtn.AddToClassList("active");
                break;
            case "profile":
                if (profileScreen != null)
                {
                    profileScreen.style.display = DisplayStyle.Flex;
                    profileBtn.AddToClassList("active");
                }
                break;
        }
    }

    private void CreateAnalyticsScreen(VisualElement root)
    {
        analyticsScreen = new VisualElement();
        analyticsScreen.name = "analytics-screen";
        analyticsScreen.AddToClassList("analytics-screen");
        analyticsScreen.style.flexDirection = FlexDirection.Column;
        analyticsScreen.style.alignItems = Align.Center;
        analyticsScreen.style.justifyContent = Justify.Center;
        analyticsScreen.style.height = Length.Percent(100);

        // Example analytics content
        var streakLabel = new Label("Current Streak: 7 days");
        streakLabel.style.fontSize = 24;
        streakLabel.style.color = Color.white;
        streakLabel.style.marginBottom = 20;

        var quizzesLabel = new Label("Quizzes Attempted: 42");
        quizzesLabel.style.fontSize = 18;
        quizzesLabel.style.color = Color.white;
        quizzesLabel.style.marginBottom = 10;

        var progressBar = new VisualElement();
        progressBar.style.width = 200;
        progressBar.style.height = 20;
        progressBar.style.backgroundColor = Color.gray;

        // Rounded corners
        progressBar.style.borderTopLeftRadius = 10;
        progressBar.style.borderTopRightRadius = 10;
        progressBar.style.borderBottomLeftRadius = 10;
        progressBar.style.borderBottomRightRadius = 10;

        var progressFill = new VisualElement();
        progressFill.style.width = Length.Percent(75); // 75% progress
        progressFill.style.height = Length.Percent(100);
        progressFill.style.backgroundColor = Color.green;

        // Rounded corners
        progressFill.style.borderTopLeftRadius = 10;
        progressFill.style.borderTopRightRadius = 10;
        progressFill.style.borderBottomLeftRadius = 10;
        progressFill.style.borderBottomRightRadius = 10;

        progressBar.Add(progressFill);

        analyticsScreen.Add(streakLabel);
        analyticsScreen.Add(new Label("Quiz Progress"));
        analyticsScreen.Add(progressBar);

        // Add to main-content
        root.Q<VisualElement>("main-content").Add(analyticsScreen);
    }

    private void CreateProfileScreen(VisualElement root)
    {
        profileScreen = new VisualElement();
        profileScreen.name = "profile-screen";
        profileScreen.AddToClassList("profile-screen");
        profileScreen.style.flexDirection = FlexDirection.Column;
        profileScreen.style.alignItems = Align.Center;
        profileScreen.style.justifyContent = Justify.Center;
        profileScreen.style.height = Length.Percent(100);

        // Example profile content
        var profileLabel = new Label("User Profile");
        profileLabel.style.fontSize = 24;
        profileLabel.style.color = Color.white;
        profileLabel.style.marginBottom = 20;

        var settingsBtn = new Button(() => Debug.Log("Open Settings"));
        settingsBtn.text = "Settings";
        settingsBtn.style.backgroundColor = Color.blue;
        settingsBtn.style.color = Color.white;
        settingsBtn.style.paddingLeft = 20;
        settingsBtn.style.paddingRight = 20;
        settingsBtn.style.paddingTop = 10;
        settingsBtn.style.paddingBottom = 10;

        // Rounded corners
        settingsBtn.style.borderTopLeftRadius = 8;
        settingsBtn.style.borderTopRightRadius = 8;
        settingsBtn.style.borderBottomLeftRadius = 8;
        settingsBtn.style.borderBottomRightRadius = 8;

        var logoutBtn = new Button(() => Debug.Log("Logout"));
        logoutBtn.text = "Logout";
        logoutBtn.style.backgroundColor = Color.red;
        logoutBtn.style.color = Color.white;
        logoutBtn.style.paddingLeft = 20;
        logoutBtn.style.paddingRight = 20;
        logoutBtn.style.paddingTop = 10;
        logoutBtn.style.paddingBottom = 10;
        logoutBtn.style.marginTop = 10;

        // Rounded corners
        logoutBtn.style.borderTopLeftRadius = 8;
        logoutBtn.style.borderTopRightRadius = 8;
        logoutBtn.style.borderBottomLeftRadius = 8;
        logoutBtn.style.borderBottomRightRadius = 8;

        profileScreen.Add(profileLabel);
        profileScreen.Add(settingsBtn);
        profileScreen.Add(logoutBtn);

        root.Q<VisualElement>("main-content").Add(profileScreen);
    }
}
