using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;

[CustomEditor(typeof(Character))]
public class CharacterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        string pathToAnimations = "Assets/Resources/Animations/Characters/" + target.GetComponent<Character>().characterName;
        GetAnimatorControllers(pathToAnimations, "Body");
        GetAnimatorControllers(pathToAnimations, "Eye");
        GetAnimatorControllers(pathToAnimations, "Mouth");
    }

    private void GetAnimatorControllers(string path, string name)
    {
        path = path + "/" + name + "/";

        GUILayout.Space(5);
        GUILayout.Label(name);

        string[] tempSplit;
        string[] files = Directory.GetFiles(path);
        foreach (string file in files)
        {
            if (file.EndsWith(".overrideController"))
            {
                tempSplit = file.Split('/');
                GUILayout.Label("- " + tempSplit[tempSplit.Length - 1].Split('.')[0]);
            }
        }
    }
}
