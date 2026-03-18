using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;

public class BatchNamespaceAdder : EditorWindow
{
    private string namespaceName = "YourNamespace";

    [MenuItem("Tools/Add Namespace (Proper)")]
    public static void ShowWindow()
    {
        GetWindow<BatchNamespaceAdder>("Namespace Adder");
    }

    void OnGUI()
    {
        GUILayout.Label("Batch Namespace Adder", EditorStyles.boldLabel);

        namespaceName = EditorGUILayout.TextField("Namespace", namespaceName);

        GUILayout.Space(10);

        if (GUILayout.Button("Apply To Selected Scripts"))
        {
            AddNamespaceToSelected();
        }
    }

    void AddNamespaceToSelected()
    {
        var selectedObjects = Selection.objects;

        foreach (var obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (!path.EndsWith(".cs"))
                continue;

            string content = File.ReadAllText(path);

            // Skip if already has namespace
            if (content.Contains("namespace "))
            {
                Debug.Log($"Skipped (already has namespace): {path}");
                continue;
            }

            var lines = content.Split('\n');

            StringBuilder usingBlock = new StringBuilder();
            StringBuilder codeBlock = new StringBuilder();

            bool isUsingSection = true;

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                if (isUsingSection && (trimmed.StartsWith("using ") || trimmed == ""))
                {
                    usingBlock.AppendLine(line);
                }
                else
                {
                    isUsingSection = false;
                    codeBlock.AppendLine(line);
                }
            }

            string newContent =
$@"{usingBlock.ToString()}
namespace {namespaceName}
{{
{Indent(codeBlock.ToString())}
}}";

            File.WriteAllText(path, newContent);
        }

        AssetDatabase.Refresh();
        Debug.Log("Namespace applied with proper structure.");
    }

    string Indent(string input)
    {
        var lines = input.Split('\n');
        return string.Join("\n", lines.Select(l => "    " + l));
    }
}