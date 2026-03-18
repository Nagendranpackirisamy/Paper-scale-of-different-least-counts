#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class SelectSceneUsedScripts
{
    [MenuItem("Tools/Select/Scene Used Scripts")]
    public static void SelectScriptsInScene()
    {
        var scene = SceneManager.GetActiveScene();

        if (!scene.isLoaded)
        {
            Debug.LogWarning("No active scene loaded.");
            return;
        }

        GameObject[] rootObjects = scene.GetRootGameObjects();

        HashSet<MonoScript> uniqueScripts = new HashSet<MonoScript>();

        foreach (var root in rootObjects)
        {
            CollectScriptsRecursive(root.transform, uniqueScripts);
        }

        if (uniqueScripts.Count == 0)
        {
            Debug.Log("No scripts found in scene.");
            return;
        }

        Object[] scriptAssets = new Object[uniqueScripts.Count];
        int index = 0;

        foreach (var script in uniqueScripts)
        {
            scriptAssets[index++] = script;
        }

        Selection.objects = scriptAssets;

        Debug.Log($"Selected {scriptAssets.Length} scripts used in scene.");
    }

    private static void CollectScriptsRecursive(Transform t, HashSet<MonoScript> scripts)
    {
        var components = t.GetComponents<MonoBehaviour>();

        foreach (var comp in components)
        {
            if (comp == null) continue;

            MonoScript script = MonoScript.FromMonoBehaviour(comp);

            if (script != null)
            {
                scripts.Add(script);
            }
        }

        foreach (Transform child in t)
        {
            CollectScriptsRecursive(child, scripts);
        }
    }
}
#endif