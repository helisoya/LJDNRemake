using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a player icon in battle
/// </summary>
public class BattlePlayerIcon : MonoBehaviour
{
    [SerializeField] private Image playerIcon;
    [SerializeField] private BattleBarFill healthFill;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private BattleBarFill spFill;
    [SerializeField] private TextMeshProUGUI spText;
    [SerializeField] private Animator animator;

    public bool fillingSP { get { return spFill.filling; } }
    public bool fillingHealth { get { return healthFill.filling; } }

    private BattleManager.CharacterData data;

    /// <summary>
    /// Gets the component's character ID
    /// </summary>
    /// <returns>Its character ID</returns>
    public string GetID()
    {
        return data.characterData.GetData().ID;
    }

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="data">Its linked data</param>
    public void Init(BattleManager.CharacterData data)
    {
        this.data = data;
        playerIcon.sprite = Resources.Load<Sprite>("RPG/Battles/Icons/" + data.characterData.GetData().ID);
        UpdateIcon(true);
    }

    /// <summary>
    /// Update the component
    /// </summary>
    /// <param name="immediateForBars">True if the health bar changes should be immediate</param>
    public void UpdateIcon(bool immediateForBars = false)
    {
        healthFill.SetValue((float)data.characterData.currentHealth / data.characterData.maxHealth, immediateForBars);
        spFill.SetValue((float)data.characterData.currentSP / data.characterData.maxSP, immediateForBars);
        healthText.text = data.characterData.currentHealth + "/" + data.characterData.maxHealth;
        spText.text = data.characterData.currentSP + "/" + data.characterData.maxSP;
    }

    /// <summary>
    /// Changes if the icon is focused on or not
    /// </summary>
    /// <param name="value">True if focused</param>
    public void SetFocus(bool value)
    {
        animator.SetBool("Selected", value);
    }
}
