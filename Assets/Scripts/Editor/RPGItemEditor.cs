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
    SerializedProperty m_attackValue;
    SerializedProperty m_defenseValue;
    SerializedProperty m_damageType;
    SerializedProperty m_attackEquation;
    SerializedProperty m_defenseEquation;
    SerializedProperty m_costSP;
    SerializedProperty m_weaponSkills;


    void OnEnable()
    {
        m_ID = serializedObject.FindProperty("ID");
        m_sellValue = serializedObject.FindProperty("sellValue");
        m_type = serializedObject.FindProperty("type");
        m_targetType = serializedObject.FindProperty("targetType");
        m_weaponType = serializedObject.FindProperty("weaponType");
        m_stats = serializedObject.FindProperty("statsValue");
        m_attackValue = serializedObject.FindProperty("attackValue");
        m_defenseValue = serializedObject.FindProperty("defenseValue");
        m_damageType = serializedObject.FindProperty("damageType");
        m_attackEquation = serializedObject.FindProperty("attackEquation");
        m_defenseEquation = serializedObject.FindProperty("defenseEquation");
        m_costSP = serializedObject.FindProperty("costSP");
        m_weaponSkills = serializedObject.FindProperty("weaponSkills");
    }


    override public void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(m_ID, new GUIContent("ID"));
        EditorGUILayout.PropertyField(m_sellValue, new GUIContent("Sell value"));
        EditorGUILayout.PropertyField(m_type, new GUIContent("Type"));

        if ((RPGItem.ItemType)m_type.enumValueIndex == RPGItem.ItemType.WEAPON ||
            (RPGItem.ItemType)m_type.enumValueIndex == RPGItem.ItemType.ARMOR)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if ((RPGItem.ItemType)m_type.enumValueIndex == RPGItem.ItemType.WEAPON)
            {
                EditorGUILayout.PropertyField(m_weaponType, new GUIContent("Weapon type"));
                EditorGUILayout.PropertyField(m_weaponSkills, new GUIContent("Weapon skills"));
            }

            EditorGUILayout.PropertyField(m_stats, new GUIContent("Weapon/Armor value"));
        }
        else if ((RPGItem.ItemType)m_type.enumValueIndex != RPGItem.ItemType.NO_USE)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_costSP, new GUIContent("Cost (SP)"));
            EditorGUILayout.PropertyField(m_targetType, new GUIContent("Target type"));
            EditorGUILayout.PropertyField(m_damageType, new GUIContent("Attack type"));

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_attackEquation, new GUIContent("Attack equation"));
            EditorGUILayout.PropertyField(m_attackValue, new GUIContent("Attack value"));

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_defenseEquation, new GUIContent("Defense equation"));
            EditorGUILayout.PropertyField(m_defenseValue, new GUIContent("Defense value"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
