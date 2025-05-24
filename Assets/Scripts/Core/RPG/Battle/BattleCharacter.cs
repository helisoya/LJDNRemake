using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;

/// <summary>
/// Represents a graphical character in a battle
/// </summary>
public class BattleCharacter : MonoBehaviour
{
    [SerializeField] private string ID;
    [SerializeField] private Transform[] weaponRoots;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject healthBarCanvas;
    [SerializeField] private Image healthBarFill;

    /// <summary>
    /// Gets the character's ID
    /// </summary>
    /// <returns>Its ID</returns>
    public string GetID()
    {
        return ID;
    }

    /// <summary>
    /// Set the character's weapon
    /// </summary>
    /// <param name="weaponID">The weapon's ID</param>
    public void SetWeapon(string weaponID)
    {
        GameObject weapon = Resources.Load<GameObject>("RPG/Battles/Weapons/" + weaponID);

        foreach (Transform root in weaponRoots)
        {
            foreach (Transform child in root) Destroy(child.gameObject);
            Instantiate(weapon, root);
        }
    }

    /// <summary>
    /// Gets the character's animator
    /// </summary>
    /// <returns>Its animator</returns>
    public Animator GetAnimator()
    {
        return animator;
    }

    /// <summary>
    /// Changes if the health bar is visible or not
    /// </summary>
    /// <param name="visible">True if visible</param>
    public void SetHealthBarVisible(bool visible)
    {
        healthBarCanvas.SetActive(visible);
        healthBarCanvas.transform.LookAt(Camera.main.transform);
    }

    /// <summary>
    /// Sets the health bar's fill amount
    /// </summary>
    /// <param name="fillAmount">Its new fill amount</param>
    public void setHealthBarFillAmount(float fillAmount)
    {
        healthBarFill.fillAmount = fillAmount;
    }
}
