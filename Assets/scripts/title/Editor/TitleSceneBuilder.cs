using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class TitleSceneBuilder
{
    private const string ScenePath = "Assets/teamhタイトル画面.unity";
    private static readonly Color Background = Hex("071017");
    private static readonly Color Panel = Hex("0C1920", 0.94f);
    private static readonly Color Gold = Hex("F6B83F");
    private static readonly Color GoldSoft = Hex("A86F20");
    private static readonly Color TextMain = Hex("F2F5F3");
    private static readonly Color TextMuted = Hex("91A3A8");
    private static readonly Color Danger = Hex("D85A48");

    [InitializeOnLoadMethod]
    private static void ScheduleInitialBuild()
    {
        EditorApplication.delayCall += BuildIfSceneIsEmpty;
    }

    private static void BuildIfSceneIsEmpty()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode
            || !File.Exists(ScenePath)
            || File.ReadAllText(ScenePath).Contains("m_Name: TitleCanvas"))
        {
            return;
        }

        Build();
    }

    [MenuItem("TEAM-H/Build Title Screen")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Object.DestroyImmediate(root);
        }

        CreateCamera();
        CreateEventSystem();

        Canvas canvas = CreateCanvas();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        TitleManager manager = canvas.gameObject.AddComponent<TitleManager>();

        CreateImage("Background", canvasRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Background);
        CreateImage(
            "RightGlow",
            canvasRect,
            new Vector2(0.54f, 0f),
            Vector2.one,
            Vector2.zero,
            Vector2.zero,
            Hex("183741", 0.6f));

        RectTransform diagonalBand = CreateImage(
            "DiagonalBand",
            canvasRect,
            new Vector2(0.56f, -0.15f),
            new Vector2(0.74f, 1.15f),
            Vector2.zero,
            Vector2.zero,
            Hex("162B31", 0.72f));
        diagonalBand.localRotation = Quaternion.Euler(0f, 0f, -11f);

        for (int index = 0; index < 30; index++)
        {
            float y = index / 30f;
            CreateImage(
                $"Scanline_{index:00}",
                canvasRect,
                new Vector2(0f, y),
                new Vector2(1f, y),
                new Vector2(0f, -1f),
                new Vector2(0f, 1f),
                Hex("8CC4C8", index % 5 == 0 ? 0.045f : 0.018f));
        }

        RectTransform scanline = CreateImage(
            "MovingScanline",
            canvasRect,
            new Vector2(0f, 0.8f),
            new Vector2(1f, 0.8f),
            Vector2.zero,
            new Vector2(0f, 3f),
            Hex("F6B83F", 0.18f));

        CreateTopBar(canvasRect);
        CreateBottomBar(canvasRect);
        CreateRobotAssetPanel(canvasRect);

        GameObject content = CreateUIObject("TitleContent", canvasRect);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        SetRect(contentRect, new Vector2(0.07f, 0.13f), new Vector2(0.57f, 0.88f), Vector2.zero, Vector2.zero);
        CanvasGroup mainGroup = content.AddComponent<CanvasGroup>();

        RectTransform titleBlock = CreateTitleBlock(contentRect);
        Image accentPulse = CreateAccentLine(contentRect);

        Button startButton = CreateMenuButton(
            contentRect,
            "START MISSION",
            "ゲームを開始",
            new Vector2(0f, 0.20f),
            new Vector2(0.58f, 0.30f),
            Gold,
            Background);
        UnityEventTools.AddPersistentListener(startButton.onClick, manager.StartGame);

        Button guideButton = CreateMenuButton(
            contentRect,
            "SYSTEM GUIDE",
            "遊び方・目的",
            new Vector2(0f, 0.075f),
            new Vector2(0.58f, 0.175f),
            Hex("18313A"),
            TextMain);
        UnityEventTools.AddPersistentListener(guideButton.onClick, manager.OpenGuide);

        Button quitButton = CreateMenuButton(
            contentRect,
            "EXIT",
            "ゲームを終了",
            new Vector2(0.61f, 0.075f),
            new Vector2(0.86f, 0.175f),
            Hex("281817"),
            Hex("E59080"));
        UnityEventTools.AddPersistentListener(quitButton.onClick, manager.QuitGame);

        GameObject guidePanel = CreateGuidePanel(canvasRect, manager);

        SerializedObject managerData = new(manager);
        managerData.FindProperty("gameSceneName").stringValue = "SampleScene";
        managerData.FindProperty("mainGroup").objectReferenceValue = mainGroup;
        managerData.FindProperty("titleBlock").objectReferenceValue = titleBlock;
        managerData.FindProperty("accentPulse").objectReferenceValue = accentPulse;
        managerData.FindProperty("scanline").objectReferenceValue = scanline;
        managerData.FindProperty("guidePanel").objectReferenceValue = guidePanel;
        managerData.FindProperty("firstSelected").objectReferenceValue = startButton;
        managerData.ApplyModifiedPropertiesWithoutUndo();

        AddTitleSceneToBuildSettings();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = canvas.gameObject;
        Debug.Log("TEAM-H title screen built successfully.");
    }

    [MenuItem("TEAM-H/Capture Title Preview")]
    public static void CapturePreview()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Camera camera = Object.FindFirstObjectByType<Camera>();
        if (camera == null)
        {
            throw new MissingReferenceException("Title scene camera was not found.");
        }

        Font japaneseFont = Font.CreateDynamicFontFromOSFont("Yu Gothic UI", 32);
        foreach (Text text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (japaneseFont != null)
            {
                text.font = japaneseFont;
            }
        }

        foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
        }

        const int width = 1920;
        const int height = 1080;
        RenderTexture renderTexture = new(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D screenshot = new(width, height, TextureFormat.RGBA32, false);

        camera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        Canvas.ForceUpdateCanvases();
        camera.Render();
        screenshot.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
        screenshot.Apply();

        string outputPath = Path.Combine(Path.GetTempPath(), "TEAM-H-title-preview.png");
        File.WriteAllBytes(outputPath, screenshot.EncodeToPNG());

        camera.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(renderTexture);
        Object.DestroyImmediate(screenshot);
        Debug.Log($"Title preview captured: {outputPath}");
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Background;
        camera.orthographic = true;
        cameraObject.AddComponent<AudioListener>();
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystemObject = new("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new("TitleCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void CreateTopBar(RectTransform parent)
    {
        RectTransform bar = CreateImage(
            "TopBar",
            parent,
            new Vector2(0f, 0.925f),
            Vector2.one,
            Vector2.zero,
            Vector2.zero,
            Hex("050A0D", 0.96f));

        CreateText(
            "TerminalLabel",
            bar,
            "CORPORATE ASSET LIQUIDATION TERMINAL  //  UNIT 01",
            20,
            TextAnchor.MiddleLeft,
            Gold,
            FontStyle.Bold,
            Vector2.zero,
            Vector2.one,
            new Vector2(52f, 0f),
            new Vector2(-40f, 0f));

        CreateText(
            "OnlineLabel",
            bar,
            "●  SYSTEM ONLINE",
            17,
            TextAnchor.MiddleRight,
            Hex("77D9B1"),
            FontStyle.Bold,
            new Vector2(0.72f, 0f),
            Vector2.one,
            Vector2.zero,
            new Vector2(-46f, 0f));
    }

    private static void CreateBottomBar(RectTransform parent)
    {
        RectTransform bar = CreateImage(
            "BottomBar",
            parent,
            Vector2.zero,
            new Vector2(1f, 0.055f),
            Vector2.zero,
            Vector2.zero,
            Hex("050A0D", 0.96f));

        CreateText(
            "Concept",
            bar,
            "SELL POWER  /  BUY FREEDOM  /  SURVIVE THE DOWNGRADE",
            16,
            TextAnchor.MiddleLeft,
            TextMuted,
            FontStyle.Normal,
            Vector2.zero,
            Vector2.one,
            new Vector2(50f, 0f),
            new Vector2(-50f, 0f));

        CreateText(
            "Version",
            bar,
            "PROTOTYPE 00.1",
            14,
            TextAnchor.MiddleRight,
            GoldSoft,
            FontStyle.Bold,
            new Vector2(0.72f, 0f),
            Vector2.one,
            Vector2.zero,
            new Vector2(-48f, 0f));
    }

    private static RectTransform CreateTitleBlock(RectTransform parent)
    {
        GameObject block = CreateUIObject("TitleBlock", parent);
        RectTransform rect = block.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0f, 0.37f), Vector2.one, Vector2.zero, Vector2.zero);

        CreateText(
            "Eyebrow",
            rect,
            "FIRST-PERSON DOWNGRADE ACTION",
            21,
            TextAnchor.LowerLeft,
            Gold,
            FontStyle.Bold,
            new Vector2(0f, 0.78f),
            new Vector2(1f, 0.88f),
            Vector2.zero,
            Vector2.zero);

        Text title = CreateText(
            "GameTitle",
            rect,
            "SCRAP\nFOR FREEDOM",
            76,
            TextAnchor.MiddleLeft,
            TextMain,
            FontStyle.Bold,
            new Vector2(0f, 0.38f),
            new Vector2(1f, 0.80f),
            Vector2.zero,
            Vector2.zero);
        title.lineSpacing = 0.78f;

        Outline outline = title.gameObject.AddComponent<Outline>();
        outline.effectColor = Hex("000000", 0.8f);
        outline.effectDistance = new Vector2(3f, -3f);

        CreateText(
            "JapaneseTagline",
            rect,
            "最強の身体を、自由のために売れ。",
            27,
            TextAnchor.MiddleLeft,
            Gold,
            FontStyle.Bold,
            new Vector2(0f, 0.28f),
            new Vector2(1f, 0.39f),
            Vector2.zero,
            Vector2.zero);

        CreateText(
            "Description",
            rect,
            "高級パーツを売れば、能力を失う。\nそれでも目標金額を稼ぎ、自分自身を買い戻せ。",
            20,
            TextAnchor.UpperLeft,
            TextMuted,
            FontStyle.Normal,
            new Vector2(0f, 0.02f),
            new Vector2(0.88f, 0.25f),
            Vector2.zero,
            Vector2.zero);

        return rect;
    }

    private static Image CreateAccentLine(RectTransform parent)
    {
        RectTransform line = CreateImage(
            "AccentPulse",
            parent,
            new Vector2(0f, 0.325f),
            new Vector2(0.54f, 0.325f),
            Vector2.zero,
            new Vector2(0f, 4f),
            Gold);
        return line.GetComponent<Image>();
    }

    private static void CreateRobotAssetPanel(RectTransform parent)
    {
        RectTransform panel = CreateImage(
            "RobotAssetPanel",
            parent,
            new Vector2(0.63f, 0.14f),
            new Vector2(0.94f, 0.86f),
            Vector2.zero,
            Vector2.zero,
            Panel);

        Outline border = panel.gameObject.AddComponent<Outline>();
        border.effectColor = Hex("42616A", 0.7f);
        border.effectDistance = new Vector2(2f, -2f);

        CreateText(
            "AssetLabel",
            panel,
            "CURRENT ASSET VALUE",
            18,
            TextAnchor.MiddleLeft,
            TextMuted,
            FontStyle.Bold,
            new Vector2(0.08f, 0.87f),
            new Vector2(0.92f, 0.95f),
            Vector2.zero,
            Vector2.zero);

        CreateText(
            "Price",
            panel,
            "¥ 10,000 G",
            38,
            TextAnchor.MiddleLeft,
            Gold,
            FontStyle.Bold,
            new Vector2(0.08f, 0.76f),
            new Vector2(0.92f, 0.88f),
            Vector2.zero,
            Vector2.zero);

        RectTransform outerCore = CreateImage(
            "OuterCore",
            panel,
            new Vector2(0.23f, 0.30f),
            new Vector2(0.77f, 0.68f),
            Vector2.zero,
            Vector2.zero,
            Hex("25444B", 0.75f));
        outerCore.localRotation = Quaternion.Euler(0f, 0f, 45f);

        RectTransform innerCore = CreateImage(
            "InnerCore",
            panel,
            new Vector2(0.31f, 0.355f),
            new Vector2(0.69f, 0.625f),
            Vector2.zero,
            Vector2.zero,
            Hex("0B151A"));
        innerCore.localRotation = Quaternion.Euler(0f, 0f, 45f);

        CreateText(
            "CoreMark",
            panel,
            "01",
            74,
            TextAnchor.MiddleCenter,
            Gold,
            FontStyle.Bold,
            new Vector2(0.25f, 0.34f),
            new Vector2(0.75f, 0.64f),
            Vector2.zero,
            Vector2.zero);

        CreateImage(
            "EyeLeft",
            panel,
            new Vector2(0.23f, 0.22f),
            new Vector2(0.46f, 0.25f),
            Vector2.zero,
            Vector2.zero,
            Danger);
        CreateImage(
            "EyeRight",
            panel,
            new Vector2(0.54f, 0.22f),
            new Vector2(0.77f, 0.25f),
            Vector2.zero,
            Vector2.zero,
            Danger);

        CreateText(
            "Status",
            panel,
            "PREMIUM FRAME  //  RESALE READY\nDOUBLE JUMP  •  AIR DASH  •  BUSTER",
            17,
            TextAnchor.UpperLeft,
            TextMuted,
            FontStyle.Normal,
            new Vector2(0.08f, 0.06f),
            new Vector2(0.92f, 0.18f),
            Vector2.zero,
            Vector2.zero);
    }

    private static Button CreateMenuButton(
        RectTransform parent,
        string primary,
        string secondary,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color backgroundColor,
        Color foregroundColor)
    {
        GameObject buttonObject = CreateUIObject(primary.Replace(" ", "_"), parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, anchorMin, anchorMax, Vector2.zero, Vector2.zero);

        Image background = buttonObject.AddComponent<Image>();
        background.color = backgroundColor;

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Hex("FFF0C4");
        colors.pressedColor = Hex("D89B31");
        colors.selectedColor = Hex("FFF0C4");
        colors.disabledColor = Hex("4A5254", 0.45f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = Hex("F6B83F", backgroundColor == Gold ? 0.8f : 0.28f);
        outline.effectDistance = new Vector2(2f, -2f);

        CreateText(
            "Primary",
            rect,
            primary,
            23,
            TextAnchor.MiddleLeft,
            foregroundColor,
            FontStyle.Bold,
            Vector2.zero,
            Vector2.one,
            new Vector2(28f, 13f),
            new Vector2(-18f, -4f));

        CreateText(
            "Secondary",
            rect,
            secondary,
            14,
            TextAnchor.LowerLeft,
            foregroundColor,
            FontStyle.Normal,
            Vector2.zero,
            Vector2.one,
            new Vector2(29f, 7f),
            new Vector2(-18f, -8f));

        return button;
    }

    private static GameObject CreateGuidePanel(RectTransform parent, TitleManager manager)
    {
        RectTransform overlay = CreateImage(
            "GuidePanel",
            parent,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero,
            Hex("02070A", 0.94f));
        overlay.GetComponent<Image>().raycastTarget = true;

        RectTransform card = CreateImage(
            "GuideCard",
            overlay,
            new Vector2(0.19f, 0.14f),
            new Vector2(0.81f, 0.86f),
            Vector2.zero,
            Vector2.zero,
            Hex("0D1B21"));
        Outline outline = card.gameObject.AddComponent<Outline>();
        outline.effectColor = GoldSoft;
        outline.effectDistance = new Vector2(2f, -2f);

        CreateText(
            "GuideTitle",
            card,
            "SYSTEM GUIDE  //  売るほど弱く、自由に近づく",
            30,
            TextAnchor.MiddleLeft,
            Gold,
            FontStyle.Bold,
            new Vector2(0.07f, 0.82f),
            new Vector2(0.93f, 0.94f),
            Vector2.zero,
            Vector2.zero);

        CreateText(
            "GuideBody",
            card,
            "目的\n"
            + "敵やステージから資金を回収し、自由を買うための目標金額を達成する。\n\n"
            + "引き算装備\n"
            + "初期装備は最高性能。ショップで高級パーツを売ると大金を得られるが、\n"
            + "二段ジャンプ・空中ダッシュ・高性能バスターなどの能力を失う。\n\n"
            + "基本操作\n"
            + "WASD：移動    Mouse：視点    左クリック：射撃\n"
            + "Space：ジャンプ    Shift：ダッシュ    Esc：パネルを閉じる",
            22,
            TextAnchor.UpperLeft,
            TextMain,
            FontStyle.Normal,
            new Vector2(0.07f, 0.22f),
            new Vector2(0.93f, 0.78f),
            Vector2.zero,
            Vector2.zero);

        Button closeButton = CreateMenuButton(
            card,
            "CLOSE",
            "タイトルへ戻る",
            new Vector2(0.63f, 0.07f),
            new Vector2(0.93f, 0.18f),
            Gold,
            Background);
        UnityEventTools.AddPersistentListener(closeButton.onClick, manager.CloseGuide);

        overlay.gameObject.SetActive(false);
        return overlay.gameObject;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject gameObject = new(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static RectTransform CreateImage(
        string name,
        RectTransform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        Color color)
    {
        GameObject gameObject = CreateUIObject(name, parent);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        SetRect(rect, anchorMin, anchorMax, offsetMin, offsetMax);
        Image image = gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private static Text CreateText(
        string name,
        RectTransform parent,
        string value,
        int size,
        TextAnchor alignment,
        Color color,
        FontStyle style,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        GameObject gameObject = CreateUIObject(name, parent);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        SetRect(rect, anchorMin, anchorMax, offsetMin, offsetMax);

        Text text = gameObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void SetRect(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
    }

    private static void AddTitleSceneToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
            .Where(scene => scene.path != ScenePath)
            .ToList();
        scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static Color Hex(string value, float alpha = 1f)
    {
        ColorUtility.TryParseHtmlString($"#{value}", out Color color);
        color.a = alpha;
        return color;
    }
}
