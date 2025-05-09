using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System;

/// <summary>
/// Represents an RPG Character's data
/// </summary>
[System.Serializable]
public class RPGCharacterData
{
    public string ID;
    public SerializedDictionary<StatType, int> stats;
    public string weapon;
    public string armor;
    public int level;
    public int currentHP;
    public int currentSP;
    public int exp;

    /// <summary>
    /// Gets a raw stat from the character
    /// </summary>
    /// <param name="type">The stat's type</param>
    /// <returns>The stat's value</returns>
    public int GetRawStat(StatType type)
    {
        return stats[type];
    }

    /// <summary>
    /// Clone the character
    /// </summary>
    /// <returns>The clone</returns>
    public RPGCharacterData Clone()
    {
        RPGCharacterData copy = new()
        {
            ID = new string(ID),
            weapon = new string(weapon),
            armor = new string(armor),
            level = level,
            currentHP = currentHP,
            currentSP = currentSP,
            exp = exp,
            stats = new SerializedDictionary<StatType, int>(stats)
        };
        return copy;
    }

    /// <summary>
    /// Copy an existing RPG Character
    /// </summary>
    /// <param name="copy">The character to copy</param>
    public void Copy(RPGCharacterData copy)
    {
        ID = new string(copy.ID);
        weapon = new string(copy.weapon);
        armor = new string(copy.armor);
        level = copy.level;
        currentHP = copy.currentHP;
        currentSP = copy.currentSP;
        exp = copy.exp;
        stats = new SerializedDictionary<StatType, int>(copy.stats);
    }

    public enum StatType
    {
        FORCE,
        RESILIENCE,
        AGILITY,
        CHARISMA,
        STRATEGY,
        BONUSHP,
    }

}
