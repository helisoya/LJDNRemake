using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents the status tab in the party menu
/// </summary>
public class PartyMenuStatusTab : PartyMenuTab
{
    [Header("Status tab")]
    [SerializeField] private PartyMenuStatusButton buttonPrefab;
    [SerializeField] private Transform buttonsRoot;
    [SerializeField] private LocalizedTextAdditive goldText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private LocalizedTextAdditive hpText;
    [SerializeField] private LocalizedTextAdditive expText;
    [SerializeField] private LocalizedTextAdditive spText;
    [SerializeField] private LocalizedTextAdditive armorText;
    [SerializeField] private LocalizedTextAdditive weaponText;
    [SerializeField] private LocalizedTextAdditive attackText;
    [SerializeField] private LocalizedTextAdditive defenseText;
    [SerializeField] private LocalizedTextAdditive charismaText;
    [SerializeField] private LocalizedTextAdditive evasionText;

    protected override void OnOpen()
    {
        goldText.SetValue(null, GameManager.GetRPGManager().money, true);

        foreach (Transform child in buttonsRoot) Destroy(child.gameObject);
        List<int> inParty = GameManager.GetRPGManager().GetFollowers();
        foreach (int idx in inParty)
        {
            Instantiate(buttonPrefab, buttonsRoot).Init(GameManager.GetRPGManager().GetCharacter(inParty[idx]), this);
        }
        SelectCharacter(GameManager.GetRPGManager().GetCharacter(inParty[0]));
    }

    protected override void OnClose()
    {
        foreach (Transform child in buttonsRoot) Destroy(child.gameObject);
    }

    /// <summary>
    /// Displays the status of a character
    /// </summary>
    /// <param name="character">The new character</param>
    public void SelectCharacter(RPGCharacter character)
    {
        nameText.text = character.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(character.GetData().ID + "_name");
        hpText.SetValue(character.currentHealth, character.maxHealth, true);
        spText.SetValue(character.currentSP, character.maxSP, true);
        expText.SetValue(character.GetData().exp, character.nextExpCap, true);
        weaponText.SetValue(null, string.IsNullOrEmpty(character.GetData().weapon) ? Locals.GetLocal("party_status_none") : Locals.GetLocal(character.GetData().weapon + "_name"), true);
        armorText.SetValue(null, string.IsNullOrEmpty(character.GetData().armor) ? Locals.GetLocal("party_status_none") : Locals.GetLocal(character.GetData().armor + "_name"), true);
        attackText.SetValue(null, character.attack, true);
        defenseText.SetValue(null, character.defense, true);
        evasionText.SetValue(null, System.Math.Round(character.evasion * 100f, 3) + "%", true);
        charismaText.SetValue(null, System.Math.Round(character.priceMultiplier * 100f, 3) + "%", true);
    }
}
