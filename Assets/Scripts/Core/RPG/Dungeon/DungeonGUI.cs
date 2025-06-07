using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the dungeon's GUI
/// </summary>
public class DungeonGUI : MonoBehaviour
{
    [Header("Other")]
    [SerializeField] private Fade fade;

    void Start()
    {
        fade.ForceAlphaTo(1f);
        fade.FadeTo(0f);
    }
}
