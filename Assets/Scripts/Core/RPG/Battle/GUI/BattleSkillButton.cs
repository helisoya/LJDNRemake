using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Represents a skill button in the battle GUI
/// </summary>
public class BattleSkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private LocalizedText skillNameText;
    private RPGItem linkedSkill;
    private BattleGUI gui;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="item">The linked item</param>
    /// <param name="gui">The gui</param>
    public void Init(RPGItem item, BattleGUI gui)
    {
        this.linkedSkill = item;
        this.gui = gui;
        skillNameText.SetNewKey(item.ID + "_name");
    }

    public void Click()
    {
        gui.SelectSkill(linkedSkill);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        gui.SetSkillDescription(linkedSkill);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gui.SetSkillDescription(null);
    }
}
