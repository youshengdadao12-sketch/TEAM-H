using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class TitleSceneNavigator : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    [SerializeField] private string destinationScenePath = "Assets/Scenes/SampleScene.unity";

    private RectTransform buttonRect;
    private bool loading;

    private void Awake()
    {
        buttonRect = transform as RectTransform;
    }

    private void Update()
    {
        if (Keyboard.current?.enterKey.wasPressedThisFrame == true)
        {
            StartGame();
        }

        Mouse mouse = Mouse.current;
        if (mouse?.leftButton.wasReleasedThisFrame == true
            && Contains(mouse.position.ReadValue()))
        {
            StartGame();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StartGame();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        StartGame();
    }

    public void StartGame()
    {
        if (loading)
        {
            return;
        }

        int buildIndex = SceneUtility.GetBuildIndexByScenePath(destinationScenePath);
        if (buildIndex < 0)
        {
            Debug.LogError($"遷移先シーンがBuild Settingsにありません: {destinationScenePath}");
            return;
        }

        loading = true;
        Debug.Log($"START MISSION: {destinationScenePath} を読み込みます。");
        SceneManager.LoadScene(buildIndex);
    }

    private bool Contains(Vector2 screenPosition)
    {
        return buttonRect != null
            && gameObject.activeInHierarchy
            && RectTransformUtility.RectangleContainsScreenPoint(buttonRect, screenPosition);
    }
}
