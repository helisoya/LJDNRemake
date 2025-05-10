using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Represents an RPG Item's editor
/// </summary>
[CustomEditor(typeof(RPGItem))]
public class RPGItemEditor : Editor
{
    SerializedProperty m_ID;
    SerializedProperty m_sellValue;
    SerializedProperty m_type;
    SerializedProperty m_targetType;
    SerializedProperty m_weaponType;
    SerializedProperty m_stats;
    SerializedProperty m_attackMultiplier;
    SerializedProperty m_defenseMultiplier;

    void OnEnable()
    {
        m_ID = serializedObject.FindProperty("ID");
        m_sellValue = serializedObject.FindProperty("sellValue");
        m_type = serializedObject.FindProperty("type");
        m_targetType = serializedObject.FindProperty("targetType");
        m_weaponType = serializedObject.FindProperty("weaponType");
        m_stats = serializedObject.FindProperty("statsValue");
        m_attackMultiplier = serializedObject.FindProperty("attackMultiplier");
        m_defenseMultiplier = serializedObject.FindProperty("defenseMultiplier");
    }


    override public void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(m_ID, new GUIContent("ID"));
        EditorGUILayout.PropertyField(m_sellValue, new GUIContent("Sell value"));
        EditorGUILayout.PropertyField(m_type, new GUIContent("Type"));

        if ((RPGItem.ItemType)m_type.enumValueIndex == RPGItem.ItemType.WEAPON ||
            (RPGItem.ItemType)m_type.enumValueIndex == RPGItem.ItemType.ARMOR)
        {
            if ((RPGItem.ItemType)m_type.enumValueIndex == RPGItem.ItemType.WEAPON)
            {
                EditorGUILayout.PropertyField(m_weaponType, new GUIContent("Weapon type"));
            }

            EditorGUILayout.PropertyField(m_stats, new GUIContent("Weapon/Armor value"));
        }
        else if ((RPGItem.ItemType)m_type.enumValueIndex != RPGItem.ItemType.NO_USE)
        {
            EditorGUILayout.PropertyField(m_targetType, new GUIContent("Target type"));
            EditorGUILayout.PropertyField(m_attackMultiplier, new GUIContent("Attack Multiplier"));
            EditorGUILayout.PropertyField(m_defenseMultiplier, new GUIContent("Defense Multiplier"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
