using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class TitleManager : MonoBehaviour
{
    [Header("Title Presentation")]
    [SerializeField] private CanvasGroup mainGroup;
    [SerializeField] private RectTransform titleBlock;
    [SerializeField] private Image accentPulse;
    [SerializeField] private RectTransform scanline;
    [SerializeField] private Selectable firstSelected;
    [SerializeField] private float introDuration = 0.65f;

    private Vector2 titleDestination;

    private void Awake()
    {
        transform.localScale = Vector3.one;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ApplyJapaneseFont();

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

    private void ApplyJapaneseFont()
    {
        Font font = CreateJapaneseFont();
        if (font == null)
        {
            return;
        }

        foreach (Text label in GetComponentsInChildren<Text>(true))
        {
            label.font = font;
        }
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
