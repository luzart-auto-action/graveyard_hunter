using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using TMPro;

namespace GraveyardHunter.Editor
{
    public class SceneSetupWizard : EditorWindow
    {
        private Vector2 _scrollPos;
        private List<string> _log = new List<string>();

        [MenuItem("GraveyardHunter/Scene Setup Wizard")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetupWizard>("Scene Setup Wizard");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Graveyard Hunter - Scene Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This wizard creates all required scene objects:\n" +
                "- Managers hierarchy\n" +
                "- Main Camera with CameraController\n" +
                "- Directional Light (dark atmosphere)\n" +
                "- UI Canvas with all panels\n" +
                "- GameConfig ScriptableObject",
                MessageType.Info);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Setup Scene", GUILayout.Height(40)))
            {
                _log.Clear();
                SetupScene();
            }

            EditorGUILayout.Space(10);

            if (_log.Count > 0)
            {
                EditorGUILayout.LabelField("Log:", EditorStyles.boldLabel);
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(300));
                foreach (var entry in _log)
                {
                    EditorGUILayout.LabelField(entry, EditorStyles.wordWrappedLabel);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void Log(string msg)
        {
            _log.Add(msg);
            Debug.Log($"[SceneSetupWizard] {msg}");
        }

        private void SetupScene()
        {
            // 1. Managers
            CreateManagers();

            // 2. Main Camera
            CreateMainCamera();

            // 3. Directional Light
            CreateDirectionalLight();

            // 4. UI Canvas with all panels
            CreateUICanvas();

            // 5. GameConfig ScriptableObject
            CreateGameConfig();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Log("--- Scene setup complete! ---");
        }

        // ===================== MANAGERS =====================

        private void CreateManagers()
        {
            if (GameObject.Find("Managers") != null)
            {
                Log("Managers already exists, skipping.");
                return;
            }

            var managers = new GameObject("Managers");
            Undo.RegisterCreatedObjectUndo(managers, "Create Managers");

            CreateManagerChild<Core.GameManager>(managers, "GameManager");
            CreateManagerChild<GameState.GameStateManager>(managers, "GameStateManager");
            CreateManagerChild<Level.LevelManager>(managers, "LevelManager");
            CreateManagerChild<UI.UIManager>(managers, "UIManager");
            CreateManagerChild<Input.InputManager>(managers, "InputManager");

            var audioGO = CreateManagerChild<Audio.AudioManager>(managers, "AudioManager");
            var audioSource = audioGO.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            // Assign music source via SerializedObject
            var audioSO = new SerializedObject(audioGO.GetComponent<Audio.AudioManager>());
            var musicSourceProp = audioSO.FindProperty("_musicSource");
            if (musicSourceProp != null)
            {
                musicSourceProp.objectReferenceValue = audioSource;
                audioSO.ApplyModifiedPropertiesWithoutUndo();
            }

            CreateManagerChild<FX.FXManager>(managers, "FXManager");
            CreateManagerChild<Core.ObjectPool>(managers, "ObjectPool");
            CreateManagerChild<Command.CommandManager>(managers, "CommandManager");
            CreateManagerChild<Shop.ShopManager>(managers, "ShopManager");

            Log("Created Managers hierarchy with all children.");
        }

        private GameObject CreateManagerChild<T>(GameObject parent, string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.AddComponent<T>();
            return go;
        }

        // ===================== CAMERA =====================

        private void CreateMainCamera()
        {
            // Remove default camera if it exists
            var existingCam = GameObject.Find("Main Camera");
            if (existingCam != null)
            {
                var camComp = existingCam.GetComponent<Camera>();
                if (camComp != null && existingCam.GetComponent<CameraSystem.CameraController>() != null)
                {
                    Log("Main Camera with CameraController already exists, skipping.");
                    return;
                }
                Undo.DestroyObjectImmediate(existingCam);
            }

            var camGO = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(camGO, "Create Main Camera");
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0f, 15f, -8f);
            camGO.transform.eulerAngles = new Vector3(60f, 0f, 0f);

            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 1f);

            camGO.AddComponent<AudioListener>();
            camGO.AddComponent<CameraSystem.CameraController>();

            Log("Created Main Camera with CameraController.");
        }

        // ===================== LIGHT =====================

        private void CreateDirectionalLight()
        {
            var existing = GameObject.Find("Directional Light");
            if (existing != null)
            {
                Log("Directional Light already exists, updating settings.");
                var light = existing.GetComponent<Light>();
                if (light != null)
                {
                    existing.transform.eulerAngles = new Vector3(50f, -30f, 0f);
                    light.intensity = 0.3f;
                }
                return;
            }

            var lightGO = new GameObject("Directional Light");
            Undo.RegisterCreatedObjectUndo(lightGO, "Create Directional Light");
            lightGO.transform.eulerAngles = new Vector3(50f, -30f, 0f);

            var dirLight = lightGO.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.intensity = 0.3f;
            dirLight.color = new Color(0.6f, 0.6f, 0.8f, 1f);

            Log("Created Directional Light (dark atmosphere).");
        }

        // ===================== UI CANVAS =====================

        private void CreateUICanvas()
        {
            if (GameObject.Find("UICanvas") != null)
            {
                Log("UICanvas already exists, skipping.");
                return;
            }

            // Canvas
            var canvasGO = new GameObject("UICanvas");
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create UICanvas");

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // EventSystem
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                Undo.RegisterCreatedObjectUndo(esGO, "Create EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
                Log("Created EventSystem.");
            }

            // --- Create all panels ---
            var canvasRect = canvasGO.GetComponent<RectTransform>();

            // Colors
            Color darkPurple = new Color(0.08f, 0.05f, 0.15f, 0.95f);
            Color darkGray = new Color(0.1f, 0.1f, 0.12f, 0.95f);
            Color darkerPurple = new Color(0.06f, 0.04f, 0.12f, 0.98f);

            // a. UIMainMenu
            var mainMenuPanel = CreatePanel("UIMainMenu", canvasRect, darkPurple);
            var mainMenuComp = mainMenuPanel.AddComponent<UI.UIMainMenu>();
            SetPanelName(mainMenuComp, "UIMainMenu");

            var titleText = CreateText("TitleText", mainMenuPanel.transform, "Graveyard Hunter", 60, TextAlignmentOptions.Center);
            SetAnchoredPos(titleText, new Vector2(0f, 500f));

            var levelText_mm = CreateText("LevelText", mainMenuPanel.transform, "Level 1", 36, TextAlignmentOptions.Center);
            SetAnchoredPos(levelText_mm, new Vector2(0f, 350f));

            var totalScoreText_mm = CreateText("TotalScoreText", mainMenuPanel.transform, "Score: 0", 30, TextAlignmentOptions.Center);
            SetAnchoredPos(totalScoreText_mm, new Vector2(0f, 280f));

            var playBtn = CreateButton("PlayButton", mainMenuPanel.transform, "PLAY", new Vector2(300f, 80f));
            SetAnchoredPos(playBtn, new Vector2(0f, 100f));

            var settingsBtn_mm = CreateButton("SettingsButton", mainMenuPanel.transform, "Settings", new Vector2(250f, 60f));
            SetAnchoredPos(settingsBtn_mm, new Vector2(0f, -20f));

            var shopBtn = CreateButton("ShopButton", mainMenuPanel.transform, "Shop", new Vector2(250f, 60f));
            SetAnchoredPos(shopBtn, new Vector2(0f, -110f));

            // Assign UIMainMenu references
            var mmSO = new SerializedObject(mainMenuComp);
            AssignRef(mmSO, "_playButton", playBtn.GetComponent<Button>());
            AssignRef(mmSO, "_settingsButton", settingsBtn_mm.GetComponent<Button>());
            AssignRef(mmSO, "_shopButton", shopBtn.GetComponent<Button>());
            AssignRef(mmSO, "_levelText", levelText_mm.GetComponent<TextMeshProUGUI>());
            AssignRef(mmSO, "_totalScoreText", totalScoreText_mm.GetComponent<TextMeshProUGUI>());
            mmSO.ApplyModifiedPropertiesWithoutUndo();

            mainMenuPanel.SetActive(true);
            Log("Created UIMainMenu panel.");

            // b. GameplayUI (extends UIPanel, uses CanvasGroup override)
            var gameplayPanel = CreatePanel("GameplayUI", canvasRect, new Color(0f, 0f, 0f, 0f));
            // CanvasGroup will be added by UIPanel.Awake if not present
            if (gameplayPanel.GetComponent<CanvasGroup>() == null)
                gameplayPanel.AddComponent<CanvasGroup>();
            var gameplayComp = gameplayPanel.AddComponent<UI.GameplayUI>();
            SetPanelName(gameplayComp, "GameplayUI");

            var pauseBtn = CreateButton("PauseButton", gameplayPanel.transform, "||", new Vector2(80f, 80f));
            SetAnchoredPos(pauseBtn, new Vector2(-60f, -60f));
            SetAnchors(pauseBtn, new Vector2(1f, 1f), new Vector2(1f, 1f));

            var resetBtn = CreateButton("ResetButton", gameplayPanel.transform, "R", new Vector2(80f, 80f));
            SetAnchoredPos(resetBtn, new Vector2(-150f, -60f));
            SetAnchors(resetBtn, new Vector2(1f, 1f), new Vector2(1f, 1f));

            var hpText = CreateText("HPText", gameplayPanel.transform, "HP: 5/5", 28, TextAlignmentOptions.Left);
            SetAnchoredPos(hpText, new Vector2(60f, -60f));
            SetAnchors(hpText, new Vector2(0f, 1f), new Vector2(0f, 1f));

            // HP icons (5 hearts)
            var hpIconsParent = new GameObject("HPIcons");
            hpIconsParent.AddComponent<RectTransform>();
            hpIconsParent.transform.SetParent(gameplayPanel.transform, false);
            SetAnchoredPos(hpIconsParent, new Vector2(60f, -110f));
            SetAnchors(hpIconsParent, new Vector2(0f, 1f), new Vector2(0f, 1f));

            Image[] hpIcons = new Image[5];
            for (int i = 0; i < 5; i++)
            {
                var heart = new GameObject($"Heart_{i}");
                var heartRect = heart.AddComponent<RectTransform>();
                heart.transform.SetParent(hpIconsParent.transform, false);
                heartRect.sizeDelta = new Vector2(30f, 30f);
                heartRect.anchoredPosition = new Vector2(i * 35f, 0f);
                var heartImg = heart.AddComponent<Image>();
                heartImg.color = Color.red;
                hpIcons[i] = heartImg;
            }

            var treasureText = CreateText("TreasureText", gameplayPanel.transform, "Treasure: 0/3", 28, TextAlignmentOptions.Left);
            SetAnchoredPos(treasureText, new Vector2(60f, -160f));
            SetAnchors(treasureText, new Vector2(0f, 1f), new Vector2(0f, 1f));

            var scoreText_gp = CreateText("ScoreText", gameplayPanel.transform, "Score: 0", 28, TextAlignmentOptions.Left);
            SetAnchoredPos(scoreText_gp, new Vector2(60f, -210f));
            SetAnchors(scoreText_gp, new Vector2(0f, 1f), new Vector2(0f, 1f));

            var levelText_gp = CreateText("LevelText", gameplayPanel.transform, "Level 1", 24, TextAlignmentOptions.Center);
            SetAnchoredPos(levelText_gp, new Vector2(0f, -60f));
            SetAnchors(levelText_gp, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            // Escape indicator (hidden)
            var escapeIndicator = CreateText("EscapeIndicator", gameplayPanel.transform, "ESCAPE!", 40, TextAlignmentOptions.Center);
            SetAnchoredPos(escapeIndicator, new Vector2(0f, -120f));
            SetAnchors(escapeIndicator, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            escapeIndicator.GetComponent<TextMeshProUGUI>().color = Color.red;
            escapeIndicator.SetActive(false);

            // Booster timer UI (hidden)
            var boosterTimerUI = new GameObject("BoosterTimerUI");
            var boosterTimerRect = boosterTimerUI.AddComponent<RectTransform>();
            boosterTimerUI.transform.SetParent(gameplayPanel.transform, false);
            SetAnchoredPos(boosterTimerUI, new Vector2(0f, -180f));
            SetAnchors(boosterTimerUI, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            boosterTimerRect.sizeDelta = new Vector2(200f, 30f);

            var boosterFillBg = new GameObject("BoosterFillBg");
            var boosterFillBgRect = boosterFillBg.AddComponent<RectTransform>();
            boosterFillBg.transform.SetParent(boosterTimerUI.transform, false);
            boosterFillBgRect.sizeDelta = new Vector2(200f, 20f);
            var bgImg = boosterFillBg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var boosterFill = new GameObject("BoosterFill");
            var boosterFillRect = boosterFill.AddComponent<RectTransform>();
            boosterFill.transform.SetParent(boosterTimerUI.transform, false);
            boosterFillRect.sizeDelta = new Vector2(200f, 20f);
            var fillImg = boosterFill.AddComponent<Image>();
            fillImg.color = new Color(0f, 1f, 0.6f, 0.9f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;

            var boosterNameText = CreateText("BoosterNameText", boosterTimerUI.transform, "", 20, TextAlignmentOptions.Center);
            SetAnchoredPos(boosterNameText, new Vector2(0f, 20f));
            boosterTimerUI.SetActive(false);

            // VirtualJoystick (bottom-left)
            var joystickArea = new GameObject("VirtualJoystick");
            var joystickRect = joystickArea.AddComponent<RectTransform>();
            joystickArea.transform.SetParent(gameplayPanel.transform, false);
            SetAnchors(joystickArea, new Vector2(0f, 0f), new Vector2(0f, 0f));
            joystickRect.anchoredPosition = new Vector2(200f, 200f);
            joystickRect.sizeDelta = new Vector2(300f, 300f);
            var joystickImage = joystickArea.AddComponent<Image>();
            joystickImage.color = new Color(1f, 1f, 1f, 0.01f);
            joystickImage.raycastTarget = true;

            var joystickBg = new GameObject("JoystickBackground");
            var joystickBgRect = joystickBg.AddComponent<RectTransform>();
            joystickBg.transform.SetParent(joystickArea.transform, false);
            joystickBgRect.sizeDelta = new Vector2(150f, 150f);
            var jbgImg = joystickBg.AddComponent<Image>();
            jbgImg.color = new Color(1f, 1f, 1f, 0.2f);

            var joystickHandle = new GameObject("JoystickHandle");
            var joystickHandleRect = joystickHandle.AddComponent<RectTransform>();
            joystickHandle.transform.SetParent(joystickBg.transform, false);
            joystickHandleRect.sizeDelta = new Vector2(60f, 60f);
            var jhImg = joystickHandle.AddComponent<Image>();
            jhImg.color = new Color(1f, 1f, 1f, 0.5f);

            var joystickComp = joystickArea.AddComponent<Input.VirtualJoystick>();

            // Assign joystick references
            var joySO = new SerializedObject(joystickComp);
            AssignRef(joySO, "_joystickBackground", joystickBgRect);
            AssignRef(joySO, "_joystickHandle", joystickHandleRect);
            joySO.ApplyModifiedPropertiesWithoutUndo();

            // Assign GameplayUI references
            var gpSO = new SerializedObject(gameplayComp);
            AssignRef(gpSO, "_pauseButton", pauseBtn.GetComponent<Button>());
            AssignRef(gpSO, "_resetButton", resetBtn.GetComponent<Button>());
            AssignRef(gpSO, "_levelText", levelText_gp.GetComponent<TextMeshProUGUI>());
            AssignRef(gpSO, "_hpText", hpText.GetComponent<TextMeshProUGUI>());
            AssignRef(gpSO, "_treasureText", treasureText.GetComponent<TextMeshProUGUI>());
            AssignRef(gpSO, "_scoreText", scoreText_gp.GetComponent<TextMeshProUGUI>());
            AssignRef(gpSO, "_escapeIndicator", escapeIndicator);
            AssignRef(gpSO, "_boosterTimerUI", boosterTimerUI);
            AssignRef(gpSO, "_boosterTimerFill", fillImg);
            AssignRef(gpSO, "_boosterNameText", boosterNameText.GetComponent<TextMeshProUGUI>());

            // Assign HP icons array
            var hpIconsProp = gpSO.FindProperty("_hpIcons");
            if (hpIconsProp != null)
            {
                hpIconsProp.arraySize = 5;
                for (int i = 0; i < 5; i++)
                {
                    hpIconsProp.GetArrayElementAtIndex(i).objectReferenceValue = hpIcons[i];
                }
            }
            gpSO.ApplyModifiedPropertiesWithoutUndo();

            gameplayPanel.SetActive(false);
            Log("Created GameplayUI panel.");

            // c. WinPanel
            var winPanel = CreatePanel("WinPanel", canvasRect, darkerPurple);
            var winComp = winPanel.AddComponent<UI.WinPanel>();
            SetPanelName(winComp, "WinPanel");

            var winTitle = CreateText("TitleText", winPanel.transform, "Level Complete!", 48, TextAlignmentOptions.Center);
            SetAnchoredPos(winTitle, new Vector2(0f, 400f));

            var winScoreText = CreateText("ScoreText", winPanel.transform, "Score: 0", 32, TextAlignmentOptions.Center);
            SetAnchoredPos(winScoreText, new Vector2(0f, 280f));

            var winTimeText = CreateText("TimeText", winPanel.transform, "Time: 0.0s", 28, TextAlignmentOptions.Center);
            SetAnchoredPos(winTimeText, new Vector2(0f, 220f));

            var winLevelText = CreateText("LevelText", winPanel.transform, "Level 1", 28, TextAlignmentOptions.Center);
            SetAnchoredPos(winLevelText, new Vector2(0f, 160f));

            // 3 star images
            Image[] starImages = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var starGO = new GameObject($"Star_{i}");
                var starRect = starGO.AddComponent<RectTransform>();
                starGO.transform.SetParent(winPanel.transform, false);
                starRect.sizeDelta = new Vector2(60f, 60f);
                starRect.anchoredPosition = new Vector2((i - 1) * 80f, 60f);
                var starImg = starGO.AddComponent<Image>();
                starImg.color = new Color(1f, 0.84f, 0f, 1f);
                starImages[i] = starImg;
            }

            var nextBtn = CreateButton("NextButton", winPanel.transform, "Next Level", new Vector2(250f, 60f));
            SetAnchoredPos(nextBtn, new Vector2(0f, -80f));

            var retryBtn_w = CreateButton("RetryButton", winPanel.transform, "Retry", new Vector2(250f, 60f));
            SetAnchoredPos(retryBtn_w, new Vector2(0f, -170f));

            var homeBtn_w = CreateButton("HomeButton", winPanel.transform, "Home", new Vector2(250f, 60f));
            SetAnchoredPos(homeBtn_w, new Vector2(0f, -260f));

            var winSO = new SerializedObject(winComp);
            AssignRef(winSO, "_scoreText", winScoreText.GetComponent<TextMeshProUGUI>());
            AssignRef(winSO, "_timeText", winTimeText.GetComponent<TextMeshProUGUI>());
            AssignRef(winSO, "_levelText", winLevelText.GetComponent<TextMeshProUGUI>());
            AssignRef(winSO, "_nextButton", nextBtn.GetComponent<Button>());
            AssignRef(winSO, "_retryButton", retryBtn_w.GetComponent<Button>());
            AssignRef(winSO, "_homeButton", homeBtn_w.GetComponent<Button>());
            var starsProp = winSO.FindProperty("_stars");
            if (starsProp != null)
            {
                starsProp.arraySize = 3;
                for (int i = 0; i < 3; i++)
                    starsProp.GetArrayElementAtIndex(i).objectReferenceValue = starImages[i];
            }
            winSO.ApplyModifiedPropertiesWithoutUndo();

            winPanel.SetActive(false);
            Log("Created WinPanel.");

            // d. FailPanel
            var failPanel = CreatePanel("FailPanel", canvasRect, darkerPurple);
            var failComp = failPanel.AddComponent<UI.FailPanel>();
            SetPanelName(failComp, "FailPanel");

            var failTitle = CreateText("TitleText", failPanel.transform, "You Died!", 48, TextAlignmentOptions.Center);
            SetAnchoredPos(failTitle, new Vector2(0f, 300f));
            failTitle.GetComponent<TextMeshProUGUI>().color = Color.red;

            var failReasonText = CreateText("ReasonText", failPanel.transform, "Caught by a ghost!", 28, TextAlignmentOptions.Center);
            SetAnchoredPos(failReasonText, new Vector2(0f, 180f));

            var failLevelText = CreateText("LevelText", failPanel.transform, "Level 1", 28, TextAlignmentOptions.Center);
            SetAnchoredPos(failLevelText, new Vector2(0f, 120f));

            var retryBtn_f = CreateButton("RetryButton", failPanel.transform, "Retry", new Vector2(250f, 60f));
            SetAnchoredPos(retryBtn_f, new Vector2(0f, -40f));

            var homeBtn_f = CreateButton("HomeButton", failPanel.transform, "Home", new Vector2(250f, 60f));
            SetAnchoredPos(homeBtn_f, new Vector2(0f, -130f));

            var failSO = new SerializedObject(failComp);
            AssignRef(failSO, "_failReasonText", failReasonText.GetComponent<TextMeshProUGUI>());
            AssignRef(failSO, "_levelText", failLevelText.GetComponent<TextMeshProUGUI>());
            AssignRef(failSO, "_retryButton", retryBtn_f.GetComponent<Button>());
            AssignRef(failSO, "_homeButton", homeBtn_f.GetComponent<Button>());
            failSO.ApplyModifiedPropertiesWithoutUndo();

            failPanel.SetActive(false);
            Log("Created FailPanel.");

            // e. PopupPause
            var pausePanel = CreatePanel("PopupPause", canvasRect, darkGray);
            var pauseComp = pausePanel.AddComponent<UI.PopupPause>();
            SetPanelName(pauseComp, "PopupPause");

            var pauseTitle = CreateText("TitleText", pausePanel.transform, "Paused", 48, TextAlignmentOptions.Center);
            SetAnchoredPos(pauseTitle, new Vector2(0f, 300f));

            var resumeBtn = CreateButton("ResumeButton", pausePanel.transform, "Resume", new Vector2(250f, 60f));
            SetAnchoredPos(resumeBtn, new Vector2(0f, 100f));

            var homeBtn_p = CreateButton("HomeButton", pausePanel.transform, "Home", new Vector2(250f, 60f));
            SetAnchoredPos(homeBtn_p, new Vector2(0f, 10f));

            var settingsBtn_p = CreateButton("SettingsButton", pausePanel.transform, "Settings", new Vector2(250f, 60f));
            SetAnchoredPos(settingsBtn_p, new Vector2(0f, -80f));

            var pauseSO = new SerializedObject(pauseComp);
            AssignRef(pauseSO, "_resumeButton", resumeBtn.GetComponent<Button>());
            AssignRef(pauseSO, "_homeButton", homeBtn_p.GetComponent<Button>());
            AssignRef(pauseSO, "_settingsButton", settingsBtn_p.GetComponent<Button>());
            pauseSO.ApplyModifiedPropertiesWithoutUndo();

            pausePanel.SetActive(false);
            Log("Created PopupPause.");

            // f. PopupSettings
            var settingsPanel = CreatePanel("PopupSettings", canvasRect, darkGray);
            var settingsComp = settingsPanel.AddComponent<UI.PopupSettings>();
            SetPanelName(settingsComp, "PopupSettings");

            var settingsTitle = CreateText("TitleText", settingsPanel.transform, "Settings", 48, TextAlignmentOptions.Center);
            SetAnchoredPos(settingsTitle, new Vector2(0f, 350f));

            var sfxLabel = CreateText("SFXLabel", settingsPanel.transform, "SFX Volume", 24, TextAlignmentOptions.Left);
            SetAnchoredPos(sfxLabel, new Vector2(-100f, 180f));

            var sfxSlider = CreateSlider("SFXSlider", settingsPanel.transform);
            SetAnchoredPos(sfxSlider, new Vector2(0f, 130f));

            var musicLabel = CreateText("MusicLabel", settingsPanel.transform, "Music Volume", 24, TextAlignmentOptions.Left);
            SetAnchoredPos(musicLabel, new Vector2(-100f, 50f));

            var musicSlider = CreateSlider("MusicSlider", settingsPanel.transform);
            SetAnchoredPos(musicSlider, new Vector2(0f, 0f));

            var closeBtn_s = CreateButton("CloseButton", settingsPanel.transform, "Close", new Vector2(200f, 60f));
            SetAnchoredPos(closeBtn_s, new Vector2(0f, -120f));

            var settingsSO = new SerializedObject(settingsComp);
            AssignRef(settingsSO, "_sfxSlider", sfxSlider.GetComponent<Slider>());
            AssignRef(settingsSO, "_musicSlider", musicSlider.GetComponent<Slider>());
            AssignRef(settingsSO, "_closeButton", closeBtn_s.GetComponent<Button>());
            settingsSO.ApplyModifiedPropertiesWithoutUndo();

            settingsPanel.SetActive(false);
            Log("Created PopupSettings.");

            // g. ShopPanel
            var shopPanel = CreatePanel("ShopPanel", canvasRect, darkPurple);
            var shopComp = shopPanel.AddComponent<UI.ShopPanel>();
            SetPanelName(shopComp, "ShopPanel");

            var shopTitle = CreateText("TitleText", shopPanel.transform, "Shop", 48, TextAlignmentOptions.Center);
            SetAnchoredPos(shopTitle, new Vector2(0f, 450f));

            var shopScoreText = CreateText("TotalScoreText", shopPanel.transform, "Score: 0", 30, TextAlignmentOptions.Center);
            SetAnchoredPos(shopScoreText, new Vector2(0f, 370f));

            // Scroll area for skins
            var scrollArea = new GameObject("SkinScrollArea");
            var scrollRect = scrollArea.AddComponent<RectTransform>();
            scrollArea.transform.SetParent(shopPanel.transform, false);
            scrollRect.sizeDelta = new Vector2(900f, 600f);
            scrollRect.anchoredPosition = new Vector2(0f, -50f);
            var scrollRectComp = scrollArea.AddComponent<ScrollRect>();
            var scrollMask = scrollArea.AddComponent<Mask>();
            var scrollBgImg = scrollArea.AddComponent<Image>();
            scrollBgImg.color = new Color(0.05f, 0.03f, 0.1f, 0.5f);

            var scrollContent = new GameObject("Content");
            var contentRect = scrollContent.AddComponent<RectTransform>();
            scrollContent.transform.SetParent(scrollArea.transform, false);
            contentRect.sizeDelta = new Vector2(900f, 1200f);
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            var vlg = scrollContent.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10f;
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var csf = scrollContent.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRectComp.content = contentRect;
            scrollRectComp.vertical = true;
            scrollRectComp.horizontal = false;

            var closeBtn_shop = CreateButton("CloseButton", shopPanel.transform, "Close", new Vector2(200f, 60f));
            SetAnchoredPos(closeBtn_shop, new Vector2(0f, -450f));

            var shopSO = new SerializedObject(shopComp);
            AssignRef(shopSO, "_skinItemParent", scrollContent.transform);
            AssignRef(shopSO, "_totalScoreText", shopScoreText.GetComponent<TextMeshProUGUI>());
            AssignRef(shopSO, "_closeButton", closeBtn_shop.GetComponent<Button>());
            shopSO.ApplyModifiedPropertiesWithoutUndo();

            shopPanel.SetActive(false);
            Log("Created ShopPanel.");

            // ---- Assign UIManager references ----
            var managersGO = GameObject.Find("Managers");
            if (managersGO != null)
            {
                var uiManagerGO = managersGO.transform.Find("UIManager");
                if (uiManagerGO != null)
                {
                    var uiManagerComp = uiManagerGO.GetComponent<UI.UIManager>();
                    if (uiManagerComp != null)
                    {
                        var uiSO = new SerializedObject(uiManagerComp);

                        // Collect all UIPanel instances
                        var panels = new List<UI.UIPanel>();
                        panels.Add(mainMenuComp);
                        panels.Add(winComp);
                        panels.Add(failComp);
                        panels.Add(pauseComp);
                        panels.Add(settingsComp);
                        panels.Add(shopComp);

                        var panelsProp = uiSO.FindProperty("_panels");
                        if (panelsProp != null)
                        {
                            panelsProp.arraySize = panels.Count;
                            for (int i = 0; i < panels.Count; i++)
                            {
                                panelsProp.GetArrayElementAtIndex(i).objectReferenceValue = panels[i];
                            }
                        }

                        AssignRef(uiSO, "_gameplayUI", gameplayComp);
                        uiSO.ApplyModifiedPropertiesWithoutUndo();
                        Log("Assigned UIManager panel references.");
                    }
                }

                // Assign InputManager joystick
                var inputManagerGO = managersGO.transform.Find("InputManager");
                if (inputManagerGO != null)
                {
                    var inputComp = inputManagerGO.GetComponent<Input.InputManager>();
                    if (inputComp != null)
                    {
                        var inputSO = new SerializedObject(inputComp);
                        AssignRef(inputSO, "_joystick", joystickComp);
                        inputSO.ApplyModifiedPropertiesWithoutUndo();
                        Log("Assigned InputManager joystick reference.");
                    }
                }
            }
        }

        // ===================== GAME CONFIG =====================

        private void CreateGameConfig()
        {
            string path = "Assets/_Game/ScriptableObjects/Configs/GameConfig.asset";

            if (AssetDatabase.LoadAssetAtPath<Data.GameConfig>(path) != null)
            {
                Log("GameConfig already exists at " + path);
                return;
            }

            // Ensure directory
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                CreateFoldersRecursive(dir);
            }

            var config = ScriptableObject.CreateInstance<Data.GameConfig>();
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            Log("Created GameConfig at " + path);
        }

        // ===================== HELPERS =====================

        private GameObject CreatePanel(string name, Transform parent, Color bgColor)
        {
            var panel = new GameObject(name);
            var rect = panel.AddComponent<RectTransform>();
            panel.transform.SetParent(parent, false);

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = panel.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            return panel;
        }

        private GameObject CreateText(string name, Transform parent, string defaultText, int fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            rect.sizeDelta = new Vector2(600f, 80f);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.enableAutoSizing = false;
            tmp.raycastTarget = false;

            return go;
        }

        private GameObject CreateButton(string name, Transform parent, string buttonText, Vector2 size)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            rect.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.15f, 0.35f, 1f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.2f, 0.15f, 0.35f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.25f, 0.5f, 1f);
            colors.pressedColor = new Color(0.15f, 0.1f, 0.25f, 1f);
            btn.colors = colors;

            var textGO = new GameObject("Text");
            var textRect = textGO.AddComponent<RectTransform>();
            textGO.transform.SetParent(go.transform, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = buttonText;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            return go;
        }

        private GameObject CreateSlider(string name, Transform parent)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            rect.sizeDelta = new Vector2(400f, 30f);

            // Background
            var bgGO = new GameObject("Background");
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgGO.transform.SetParent(go.transform, false);
            bgRect.anchorMin = new Vector2(0f, 0.25f);
            bgRect.anchorMax = new Vector2(1f, 0.75f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.1f, 0.25f, 1f);

            // Fill Area
            var fillAreaGO = new GameObject("Fill Area");
            var fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
            fillAreaGO.transform.SetParent(go.transform, false);
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(5f, 0f);
            fillAreaRect.offsetMax = new Vector2(-5f, 0f);

            var fillGO = new GameObject("Fill");
            var fillRect = fillGO.AddComponent<RectTransform>();
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            fillRect.sizeDelta = new Vector2(0f, 0f);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = new Color(0f, 0.8f, 0.5f, 1f);

            // Handle
            var handleAreaGO = new GameObject("Handle Slide Area");
            var handleAreaRect = handleAreaGO.AddComponent<RectTransform>();
            handleAreaGO.transform.SetParent(go.transform, false);
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10f, 0f);
            handleAreaRect.offsetMax = new Vector2(-10f, 0f);

            var handleGO = new GameObject("Handle");
            var handleRect = handleGO.AddComponent<RectTransform>();
            handleGO.transform.SetParent(handleAreaGO.transform, false);
            handleRect.sizeDelta = new Vector2(20f, 0f);
            var handleImg = handleGO.AddComponent<Image>();
            handleImg.color = new Color(0.8f, 0.8f, 0.9f, 1f);

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImg;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            return go;
        }

        private void SetAnchoredPos(GameObject go, Vector2 pos)
        {
            var rect = go.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = pos;
        }

        private void SetAnchors(GameObject go, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rect = go.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
            }
        }

        private void SetPanelName(UI.UIPanel panel, string panelName)
        {
            var so = new SerializedObject(panel);
            var prop = so.FindProperty("_panelName");
            if (prop != null)
            {
                prop.stringValue = panelName;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private void AssignRef(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
        }

        private void CreateFoldersRecursive(string path)
        {
            path = path.Replace("\\", "/");
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
