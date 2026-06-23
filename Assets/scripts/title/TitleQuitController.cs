using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class TitleQuitController : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    private RectTransform buttonRect;

    private void Awake()
    {
        buttonRect = transform as RectTransform;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse?.leftButton.wasReleasedThisFrame == true
            && buttonRect != null
            && RectTransformUtility.RectangleContainsScreenPoint(
                buttonRect,
                mouse.position.ReadValue()))
        {
            QuitGame();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        QuitGame();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        QuitGame();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
