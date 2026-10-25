#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UntitledGame.EditorTools
{
    public static class EnterPlayMode
    {
        /// <summary>
        /// CLI: Unity.exe -projectPath ... -executeMethod UntitledGame.EditorTools.EnterPlayMode.Run
        /// </summary>
        public static void Run()
        {
            var scenePath = "Assets/Scenes/SampleScene.unity";
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogError("SampleScene missing: " + scenePath);
                EditorApplication.Exit(1);
                return;
            }

            EditorSceneManager.OpenScene(scenePath);
            EditorApplication.isPlaying = true;
        }
    }
}
#endif
