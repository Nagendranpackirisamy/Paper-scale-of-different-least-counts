using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ProjectStructureOrganizer
{
    static string root = "Assets/ProjectStructure";

    [MenuItem("Tools/Organize Project Scripts")]
    static void Organize()
    {
        CreateFolders();

        HashSet<string> usedScripts = new HashSet<string>();
        HashSet<string> usedScriptableObjects = new HashSet<string>();

        // Find scripts used in scenes
        GameObject[] objects = GameObject.FindObjectsOfType<GameObject>(true);

        foreach (GameObject obj in objects)
        {
            MonoBehaviour[] behaviours = obj.GetComponents<MonoBehaviour>();

            foreach (var b in behaviours)
            {
                if (b == null) continue;

                MonoScript script = MonoScript.FromMonoBehaviour(b);
                string path = AssetDatabase.GetAssetPath(script);

                if (!string.IsNullOrEmpty(path))
                    usedScripts.Add(path);
            }

            // Check ScriptableObject references
            SerializedObject so = new SerializedObject(obj);
            SerializedProperty prop = so.GetIterator();

            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    Object reference = prop.objectReferenceValue;

                    if (reference is ScriptableObject)
                    {
                        string path = AssetDatabase.GetAssetPath(reference);
                        usedScriptableObjects.Add(path);
                    }
                }
            }
        }

        // All scripts
        string[] allScripts = AssetDatabase.FindAssets("t:MonoScript");

        foreach (string guid in allScripts)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (path.Contains("/Editor/"))
            {
                MoveAsset(path, root + "/EditorScripts");
            }
            else if (usedScripts.Contains(path))
            {
                MoveAsset(path, root + "/SceneUsedScripts");
            }
            else
            {
                MoveAsset(path, root + "/UnusedScripts");
            }
        }

        // ScriptableObjects
        string[] scriptables = AssetDatabase.FindAssets("t:ScriptableObject");

        foreach (string guid in scriptables)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (usedScriptableObjects.Contains(path))
            {
                MoveAsset(path, root + "/SceneScriptableObjects");
            }
        }

        // Custom Shaders
        string[] shaders = AssetDatabase.FindAssets("t:Shader");

        foreach (string guid in shaders)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MoveAsset(path, root + "/CustomShaders");
        }

        AssetDatabase.Refresh();
        Debug.Log("Project Organized Successfully");
    }

    static void CreateFolders()
    {
        if (!AssetDatabase.IsValidFolder(root))
            AssetDatabase.CreateFolder("Assets", "ProjectStructure");

        Create(root, "EditorScripts");
        Create(root, "SceneUsedScripts");
        Create(root, "UnusedScripts");
        Create(root, "SceneScriptableObjects");
        Create(root, "CustomShaders");
    }

    static void Create(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
    }

    static void MoveAsset(string path, string folder)
    {
        string fileName = Path.GetFileName(path);
        string newPath = folder + "/" + fileName;

        if (path != newPath)
            AssetDatabase.MoveAsset(path, newPath);
    }
}