using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an RPG Item
/// </summary>
[CreateAssetMenu(menuName = "LJDN/RPG Item")]
public class RPGItem : ScriptableObject
{
    public string ID;
    public int sellValue;
    public ItemType type;
    public TargetType targetType;
    public float attackMultiplier;
    public float defenseMultiplier;
    public WeaponType weaponType;
    public int statsValue;

    public enum TargetType
    {
        ALL,
        ONEALLY,
        ALLALLY,
        ONEFOE,
        ALLFOE
    }

    public enum WeaponType
    {
        ONEHANDED,
        TWOHANDED,
        GUN
    }

    public enum ItemType
    {
        NO_USE,
        USABLE_ALL,
        USABLE_COMBAT,
        WEAPON,
        ARMOR
    }
}
