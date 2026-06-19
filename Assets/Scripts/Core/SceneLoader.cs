using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoader : MonoBehaviour
{
    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("読み込むシーン名が指定されていません。");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public static void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadTitle() => LoadScene("TitleScene");
    public void LoadShop() => LoadScene("ShopScene");
    public void LoadStage1() => LoadScene("Stage1");
    public void LoadEnding() => LoadScene("EndingScene");
    public void QuitGame() => Application.Quit();
}
