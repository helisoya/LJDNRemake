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
    public bool fading { get { return fade.fading; } }

    void Start()
    {
        fade.ForceAlphaTo(1f);
        fade.FadeTo(0f);
    }

    /// <summary>
    /// Starts fading the screen
    /// </summary>
    /// <param name="alpha">The alpha target</param>
    public void FadeTo(float alpha)
    {
        fade.FadeTo(alpha);
    }
}
