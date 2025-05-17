using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

/// <summary>
/// Represents a graphical character in a battle
/// </summary>
public class BattleCharacter : MonoBehaviour
{
    [SerializeField] private string ID;
    [SerializeField] private Transform[] weaponRoots;
    [SerializeField] private Animator animator;

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
}
