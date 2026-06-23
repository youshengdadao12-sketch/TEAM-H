using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class MissingScriptCleaner
{
    static MissingScriptCleaner()
    {
        EditorApplication.delayCall += CleanOpenScenes;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("TEAM-H/Remove Missing Scripts")]
    public static void CleanOpenScenes()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode
            || EditorApplication.isCompiling
            || EditorApplication.isUpdating)
        {
            return;
        }

        int removedCount = 0;

        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIndex);
            if (!scene.isLoaded)
            {
                continue;
            }

            int removedFromScene = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                removedFromScene += RemoveFromHierarchy(root);
            }

            if (removedFromScene <= 0)
            {
                continue;
            }

            removedCount += removedFromScene;
            EditorSceneManager.MarkSceneDirty(scene);

            if (!string.IsNullOrWhiteSpace(scene.path))
            {
                EditorSceneManager.SaveScene(scene);
            }
        }

        if (removedCount > 0)
        {
            Debug.Log($"Removed {removedCount} missing script component(s) from open scene(s).");
        }
    }

    private static int RemoveFromHierarchy(GameObject gameObject)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);

        foreach (Transform child in gameObject.transform)
        {
            removed += RemoveFromHierarchy(child.gameObject);
        }

        return removed;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.delayCall += CleanOpenScenes;
        }
    }
}
