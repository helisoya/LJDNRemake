using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTargetButton : MonoBehaviour
{
    [SerializeField] private LocalizedText targetNameText;
    private List<BattleManager.CharacterData> linkedTarget;
    private BattleGUI gui;

    /// <summary>
    /// Initialize the component
    /// </summary>
    /// <param name="target">The linked target</param>
    /// <param name="gui">The gui</param>
    public void Init(List<BattleManager.CharacterData> target, BattleGUI gui)
    {
        this.linkedTarget = target;
        this.gui = gui;

        if (target.Count == 1) targetNameText.SetNewKey(target[0].characterData.GetData().ID + "_name");
        else targetNameText.SetNewKey("battle_target_multiple");
    }

    public void Click()
    {
        gui.SelectTarget(linkedTarget);
    }
}
