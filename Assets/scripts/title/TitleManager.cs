using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class TitleManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Animation")]
    [SerializeField] private CanvasGroup mainGroup;
    [SerializeField] private RectTransform titleBlock;
    [SerializeField] private Image accentPulse;
    [SerializeField] private RectTransform scanline;
    [SerializeField] private float introDuration = 0.65f;

    [Header("Panels")]
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private Selectable firstSelected;

    private Vector2 titleDestination;
    private Font runtimeFont;
    private Button startButton;
    private Button guideButton;
    private Button quitButton;
    private Button closeButton;
    private bool transitionRequested;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        EnsureUiInputWorks();
        FindButtons();

        runtimeFont = CreateJapaneseFont();
        if (runtimeFont != null)
        {
            foreach (Text label in GetComponentsInChildren<Text>(true))
            {
                label.font = runtimeFont;
            }
        }

        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }

        if (mainGroup != null)
        {
            mainGroup.alpha = 0f;
            mainGroup.interactable = false;
            mainGroup.blocksRaycasts = false;
        }

        if (titleBlock != null)
        {
            titleDestination = titleBlock.anchoredPosition;
            titleBlock.anchoredPosition += Vector2.up * 40f;
        }
    }

    private void Start()
    {
        StartCoroutine(PlayIntro());
    }

    private void Update()
    {
        if (accentPulse != null)
        {
            Color color = accentPulse.color;
            color.a = Mathf.Lerp(0.35f, 0.9f, (Mathf.Sin(Time.unscaledTime * 2.4f) + 1f) * 0.5f);
            accentPulse.color = color;
        }

        if (scanline != null)
        {
            float normalized = Mathf.Repeat(Time.unscaledTime * 0.08f, 1f);
            scanline.anchorMin = new Vector2(0f, 1f - normalized);
            scanline.anchorMax = new Vector2(1f, 1f - normalized);
        }

        if (guidePanel != null
            && guidePanel.activeSelf
            && Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            CloseGuide();
        }

        if (guidePanel != null
            && !guidePanel.activeSelf
            && Keyboard.current?.enterKey.wasPressedThisFrame == true)
        {
            StartGame();
        }

        HandleFallbackMouseClick();
    }

    public void StartGame()
    {
        if (transitionRequested)
        {
            return;
        }

        int buildIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/SampleScene.unity");
        if (buildIndex >= 0)
        {
            transitionRequested = true;
            Debug.Log($"START MISSION: loading build scene {buildIndex}.");
            SceneManager.LoadScene(buildIndex);
            return;
        }

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogWarning("遷移先のゲームシーンが設定されていません。");
            return;
        }

        transitionRequested = true;
        Debug.Log($"START MISSION: loading scene {gameSceneName}.");
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenGuide()
    {
        if (guidePanel == null)
        {
            return;
        }

        guidePanel.SetActive(true);
        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void CloseGuide()
    {
        if (guidePanel == null)
        {
            return;
        }

        guidePanel.SetActive(false);
        if (firstSelected != null)
        {
            firstSelected.Select();
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator PlayIntro()
    {
        float elapsed = 0f;
        while (elapsed < introDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / introDuration);
            float eased = 1f - Mathf.Pow(1f - progress, 3f);

            if (mainGroup != null)
            {
                mainGroup.alpha = eased;
            }

            if (titleBlock != null)
            {
                titleBlock.anchoredPosition = Vector2.Lerp(
                    titleDestination + Vector2.up * 40f,
                    titleDestination,
                    eased);
            }

            yield return null;
        }

        if (mainGroup != null)
        {
            mainGroup.alpha = 1f;
            mainGroup.interactable = true;
            mainGroup.blocksRaycasts = true;
        }

        if (firstSelected != null)
        {
            firstSelected.Select();
            EventSystem.current?.SetSelectedGameObject(firstSelected.gameObject);
        }
    }

    private static void EnsureUiInputWorks()
    {
        EventSystem eventSystem = EventSystem.current ?? FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        inputModule.UnassignActions();
        inputModule.AssignDefaultActions();
        inputModule.enabled = false;
        inputModule.enabled = true;
    }

    private void FindButtons()
    {
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            switch (button.name)
            {
                case "START_MISSION":
                    startButton = button;
                    break;
                case "SYSTEM_GUIDE":
                    guideButton = button;
                    break;
                case "EXIT":
                    quitButton = button;
                    break;
                case "CLOSE":
                    closeButton = button;
                    break;
            }
        }
    }

    private void HandleFallbackMouseClick()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasReleasedThisFrame)
        {
            return;
        }

        Vector2 position = mouse.position.ReadValue();

        if (guidePanel != null && guidePanel.activeSelf)
        {
            if (ContainsScreenPoint(closeButton, position))
            {
                CloseGuide();
            }

            return;
        }

        if (ContainsScreenPoint(startButton, position))
        {
            StartGame();
        }
        else if (ContainsScreenPoint(guideButton, position))
        {
            OpenGuide();
        }
        else if (ContainsScreenPoint(quitButton, position))
        {
            QuitGame();
        }
    }

    private static bool ContainsScreenPoint(Button button, Vector2 position)
    {
        return button != null
            && button.interactable
            && button.gameObject.activeInHierarchy
            && RectTransformUtility.RectangleContainsScreenPoint(
                button.transform as RectTransform,
                position);
    }

    private static Font CreateJapaneseFont()
    {
        string[] preferredFonts =
        {
            "Yu Gothic UI",
            "Yu Gothic",
            "Meiryo UI",
            "Meiryo",
            "Noto Sans CJK JP"
        };

        foreach (string fontName in preferredFonts)
        {
            Font font = Font.CreateDynamicFontFromOSFont(fontName, 32);
            if (font != null)
            {
                return font;
            }
        }

        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}
