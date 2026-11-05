using System.Linq;
using Unity.Multiplayer.PlayMode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneMoreFloor.EditorTools
{
    /// <summary>
    /// Unity can start with an empty, unsaved scene, and pressing Play then shows "No cameras
    /// rendering". This opens the game's first scene instead. A scene someone is working on stays open.
    /// </summary>
    [InitializeOnLoad]
    public static class OpenGameSceneOnStartup
    {
        private const string AlreadyRanKey = "OneMoreFloor.OpenGameSceneOnStartup.Ran";

        static OpenGameSceneOnStartup()
        {
            // SessionState resets when Unity restarts, so this runs once per launch, not on every recompile.
            // Virtual players follow the main editor's scene, so they skip it too.
            if (Application.isBatchMode || !CurrentPlayer.IsMainEditor || SessionState.GetBool(AlreadyRanKey, false))
                return;

            SessionState.SetBool(AlreadyRanKey, true);
            EditorApplication.delayCall += OpenIfEmpty;
        }

        private static void OpenIfEmpty()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            Scene current = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(current.path) || current.isDirty)
                return;

            EditorBuildSettingsScene firstScene = EditorBuildSettings.scenes.FirstOrDefault(scene => scene.enabled);
            if (firstScene != null)
                EditorSceneManager.OpenScene(firstScene.path);
        }
    }
}
