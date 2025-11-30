using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Represents the LJDN's Utils window
/// </summary>
public class LJDNUtilsEditor : EditorWindow
{

    [MenuItem("LJDN/Utils")]
    public static void ShowExample()
    {
        LJDNUtilsEditor wnd = GetWindow<LJDNUtilsEditor>();
        wnd.titleContent = new GUIContent("LJDN Utils");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        Button itemLocalsButton = new Button();
        itemLocalsButton.text = "Regenerate Items Locals";
        itemLocalsButton.clicked += OnRegenerateItemsLocals;
        root.Add(itemLocalsButton);
    }

    /// <summary>
    /// Regenerate the missing items's local
    /// </summary>
    private void OnRegenerateItemsLocals()
    {
        RegenerateMissingItemsLocals("Assets/Resources/Locals/eng_system.txt");
        RegenerateMissingItemsLocals("Assets/Resources/Locals/fra_system.txt");
    }

    private void RegenerateMissingItemsLocals(string filePath)
    {
        if (File.Exists(filePath))
        {
            string[] filesUnCleaned = Directory.GetFiles("Assets/Resources/RPG/Items/");
            List<string> files = new List<string>();
            int start;
            int end;
            foreach (string file in filesUnCleaned)
            {
                if (!file.EndsWith(".meta"))
                {
                    start = file.LastIndexOf("/") + 1;
                    end = file.LastIndexOf(".");
                    files.Add(file.Substring(start, end - start));
                }
            }


            string[] lines = File.ReadAllLines(filePath);
            string line;
            for (int i = 0; i < lines.Length; i++)
            {
                line = lines[i];
                if (string.IsNullOrEmpty(line)) continue;

                line = line.Split(" = ")[0];
                if (line.EndsWith("_name"))
                {
                    line = line.Substring(0, line.Length - 5);
                    files.Remove(line);
                }
            }

            if (files.Count == 0) return;

            using (StreamWriter sw = new StreamWriter(filePath, true))
            {
                sw.Write("\n");
                foreach (string file in files)
                {
                    sw.WriteLine(file + "_name = " + file);
                    sw.WriteLine(file + "_desc = TODO");
                    Debug.LogWarning("Adding Missing Item Local : " + file + " in file " + filePath);
                }
            }
        }
    }




}
