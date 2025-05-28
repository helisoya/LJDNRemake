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
    [SerializeField] private BattleBarFill healthBar;

    public bool healthBarFilling { get { return healthBar.filling; } }

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
        RPGItem item = GameManager.GetRPGManager().GetItem(weaponID);
        if (!string.IsNullOrEmpty(item.linkedAnimatior)) animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/Battles/" + item.linkedAnimatior);

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
    /// <param name="immediate">True if the changes should be immediate</param>
    public void setHealthBarFillAmount(float fillAmount, bool immediate)
    {
        healthBar.SetValue(fillAmount, immediate);
    }

    /// <summary>
    /// Changes if the character is visually blocking or not
    /// </summary>
    /// <param name="blocking">True if the character is blocking</param>
    public void SetBlocking(bool blocking)
    {
        animator.SetBool("Blocking", blocking);
    }

    /// <summary>
    /// Triggers visual damage
    /// </summary>
    public void TriggerDamage()
    {
        animator.SetTrigger("Damage");
    }

    /// <summary>
    /// Plays an animation
    /// </summary>
    /// <param name="animation">The animation's name</param>
    public void PlayAnimation(string animation)
    {
        animator.CrossFade(animation, 0.1f);
    }
}
