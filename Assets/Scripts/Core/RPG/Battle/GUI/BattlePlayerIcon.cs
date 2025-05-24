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
    [SerializeField] private Image healthFill;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image spFill;
    [SerializeField] private TextMeshProUGUI spText;
    [SerializeField] private Animator animator;

    private BattleManager.CharacterData data;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="data">Its linked data</param>
    public void Init(BattleManager.CharacterData data)
    {
        this.data = data;
        playerIcon.sprite = Resources.Load<Sprite>("RPG/Battles/Icons/" + data.characterData.GetData().ID);
        UpdateIcon();
    }

    /// <summary>
    /// Update the component
    /// </summary>
    public void UpdateIcon()
    {
        healthFill.fillAmount = (float)data.characterData.currentHealth / data.characterData.maxHealth;
        spFill.fillAmount = (float)data.characterData.currentSP / data.characterData.maxSP;
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
