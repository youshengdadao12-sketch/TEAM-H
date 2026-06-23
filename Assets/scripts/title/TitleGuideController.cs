using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class TitleGuideController : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private Button closeButton;

    private RectTransform openButtonRect;

    private void Awake()
    {
        openButtonRect = transform as RectTransform;
        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (guidePanel != null
            && guidePanel.activeSelf
            && Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            CloseGuide();
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse?.leftButton.wasReleasedThisFrame != true)
        {
            return;
        }

        Vector2 position = mouse.position.ReadValue();
        if (guidePanel != null && guidePanel.activeSelf)
        {
            if (closeButton != null
                && RectTransformUtility.RectangleContainsScreenPoint(
                    closeButton.transform as RectTransform,
                    position))
            {
                CloseGuide();
            }

            return;
        }

        if (openButtonRect != null
            && RectTransformUtility.RectangleContainsScreenPoint(openButtonRect, position))
        {
            OpenGuide();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OpenGuide();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        OpenGuide();
    }

    public void OpenGuide()
    {
        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
        }
    }

    public void CloseGuide()
    {
        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }
    }
}
