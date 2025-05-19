using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents an item button in the battle GUI
/// </summary>
public class BattleItemButton : MonoBehaviour
{
    [SerializeField] private LocalizedText skillNameText;
    [SerializeField] private TextMeshProUGUI amountText;
    private RPGItem linkedItem;
    private BattleGUI gui;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="item">The linked item</param>
    /// <param name="gui">The gui</param>
    public void Init(RPGItem item, BattleGUI gui)
    {
        this.linkedItem = item;
        this.gui = gui;
        skillNameText.SetNewKey(item.ID + "_name");
        skillNameText.GetText().ForceMeshUpdate(true);
        amountText.fontSize = skillNameText.GetText().fontSize;
        amountText.text = "- " + GameManager.GetRPGManager().GetAmountInInventory(item.ID).ToString();
    }

    public void Click()
    {
        gui.SelectItem(linkedItem);
    }
}
