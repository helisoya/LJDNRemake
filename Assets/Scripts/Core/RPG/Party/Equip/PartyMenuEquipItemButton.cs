using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an item button in the equip tab of the party menu
/// </summary>
public class PartyMenuEquipItemButton : MonoBehaviour
{
    [SerializeField] private LocalizedTextAdditive itemText;
    private RPGItem linkedItem;
    private PartyMenuEquipTab menu;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="item">The item</param>
    /// <param name="menu">The root tab</param>
    public void Init(RPGItem item, PartyMenuEquipTab menu)
    {
        this.linkedItem = item;
        this.menu = menu;

        itemText.SetValue(linkedItem ? Locals.GetLocal(item.ID + "_name") : Locals.GetLocal("party_status_none"), linkedItem ? linkedItem.statsValue : 0, true);
    }

    /// <summary>
    /// On Click Event
    /// </summary>
    public void Click()
    {
        menu.SelectItem(linkedItem);
    }
}
