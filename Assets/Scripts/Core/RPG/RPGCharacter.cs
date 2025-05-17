using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a RPG Character
/// </summary>
public class RPGCharacter
{
    private RPGCharacterData baseData;

    #region STATS CONSTANTS
    private const int HEALTH_BASE = 25;
    private const float HEALTH_MULT_LEVEL = 5;
    private const float HEALTH_MULT_BONUS = 2.5f;

    private const int ATTACK_BASE = 5;
    private const float ATTACK_MULT_LEVEL = 5.0f;
    private const float ATTACK_MULT_FORCE = 10.0f;

    private const float EVASION_BASE = 0.05f;
    private const float EVASION_MULT_AGILITY = 105.0f;

    private const int SP_BASE = 5;
    private const float SP_MULT_LEVEL = 1.25f;
    private const float SP_MULT_STRATEGY = 0.75f;

    private const float PRICE_BASE = 1.0f;
    private const float PRICE_MULT_CHARISMA = 175.0f;

    private const int DEFENSE_BASE = 2;
    private const float DEFENSE_MULT_LEVEL = 6f;
    private const float DEFENSE_MULT_RESILIENCE = 12.0f;

    private const int EXP_BASE = 20;

    #endregion

    public int currentHealth { get { return baseData.currentHP; } }
    public int currentSP { get { return baseData.currentSP; } }
    public int maxHealth { get; private set; }
    public int attack { get; private set; }
    public float evasion { get; private set; }
    public int maxSP { get; private set; }
    public float priceMultiplier { get; private set; }
    public int defense { get; private set; }
    public int nextExpCap { get; private set; }
    public bool canLevelUp { get { return baseData.exp >= nextExpCap; } }

    public RPGCharacter(RPGCharacterData data)
    {
        SetData(data);
    }

    /// <summary>
    /// Gets the character's savable data
    /// </summary>
    /// <returns>The savable data</returns>
    public RPGCharacterData GetData()
    {
        return baseData;
    }

    /// <summary>
    /// Sets the character's data
    /// </summary>
    /// <param name="data">The data</param>
    public void SetData(RPGCharacterData data)
    {
        baseData = data;
        UpdateComputedStats();
    }

    /// <summary>
    /// Update the computed stats
    /// </summary>
    public void UpdateComputedStats()
    {
        // Heath = BASE + LVL * MULT_LVL + BONUS * MULT_BONUS
        // Attack = BASE + (1.0f + LVL / MULT_LVL + FORCE / MULT_FORCE) * WEAPON_ATTACK
        // Evasion = BASE + AGILITY / MULT_AGILITY
        // SP = BASE + LVL * MULT_LEVEL + STRATEGY * MULT_STRATEGY
        // DEFENSE = BASE + (1.0f + LVL / MULT_LVL + RESILIENCE / MULT_RESILIENCE) * ARMOR_VALUE
        // Price = BASE - CHARISMA * MULT_CHARISMA
        // Next EXP = BASE + (LVL*2)**2

        maxHealth = HEALTH_BASE + Mathf.FloorToInt(HEALTH_MULT_LEVEL * baseData.level + HEALTH_MULT_BONUS * baseData.GetRawStat(RPGCharacterData.StatType.BONUSHP));
        attack = Mathf.FloorToInt(
            (1.0f + baseData.GetRawStat(RPGCharacterData.StatType.FORCE) / ATTACK_MULT_FORCE + baseData.level / ATTACK_MULT_LEVEL)
            * (ATTACK_BASE + (string.IsNullOrEmpty(baseData.weapon) ? 0.0f : GameManager.GetRPGManager().GetItem(baseData.weapon).statsValue)));
        evasion = EVASION_BASE + baseData.GetRawStat(RPGCharacterData.StatType.AGILITY) / EVASION_MULT_AGILITY;
        maxSP = SP_BASE + Mathf.FloorToInt(baseData.level * SP_MULT_LEVEL + baseData.GetRawStat(RPGCharacterData.StatType.STRATEGY) * SP_MULT_STRATEGY);
        defense = Mathf.FloorToInt(
            (1.0f + baseData.GetRawStat(RPGCharacterData.StatType.RESILIENCE) / DEFENSE_MULT_RESILIENCE + baseData.level / DEFENSE_MULT_LEVEL)
            * (DEFENSE_BASE + (string.IsNullOrEmpty(baseData.armor) ? 0.0f : GameManager.GetRPGManager().GetItem(baseData.armor).statsValue)));
        priceMultiplier = PRICE_BASE - baseData.GetRawStat(RPGCharacterData.StatType.CHARISMA) / PRICE_MULT_CHARISMA;
        nextExpCap = EXP_BASE + Mathf.FloorToInt(Mathf.Pow(baseData.level * 2, 2));

        attack = Mathf.Clamp(attack, ATTACK_BASE, 999);
        defense = Mathf.Clamp(defense, DEFENSE_BASE, 999);
        maxHealth = Mathf.Clamp(maxHealth, HEALTH_BASE, 999);
        evasion = Mathf.Clamp(evasion, EVASION_BASE, 0.8f);
        maxSP = Mathf.Clamp(maxSP, SP_BASE, 999);
        priceMultiplier = Mathf.Clamp(priceMultiplier, 0.1f, PRICE_BASE);
        nextExpCap = Mathf.Clamp(nextExpCap, EXP_BASE, 999999);
    }

    /// <summary>
    /// Sets the character's health to max
    /// </summary>
    public void SetHealthToMax()
    {
        baseData.currentHP = maxHealth;
    }

    /// <summary>
    /// Set the character's SP to max
    /// </summary>
    public void SetSPToMax()
    {
        baseData.currentSP = maxSP;
    }

    /// <summary>
    /// Level up the character
    /// </summary>
    public void LevelUp()
    {
        if (baseData.level >= 99) return;

        baseData.level++;
        baseData.exp -= nextExpCap;



        List<int> possibilities = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            if (baseData.stats[(RPGCharacterData.StatType)i] < 99) possibilities.Add(i);
        }


        int awarded = 0;
        bool canContinue = possibilities.Count > 0;

        while (awarded < 4 && canContinue)
        {
            int selectedIdx = Random.Range(0, possibilities.Count);

            int selectedStat = possibilities[selectedIdx];
            baseData.stats[(RPGCharacterData.StatType)selectedStat]++;
            if (baseData.stats[(RPGCharacterData.StatType)selectedStat] >= 99)
            {
                possibilities.RemoveAt(selectedIdx);
                if (possibilities.Count == 0) canContinue = false;
            }

            awarded++;
        }

        UpdateComputedStats();
        SetHealthToMax();
        SetSPToMax();
    }
}
