using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a battle's data
/// </summary>
[CreateAssetMenu(menuName = "LJDN/Battle Data")]
public class BattleData : ScriptableObject
{
    public RPGCharacterDataInterface[] ennemies;

    public enum CloseType
    {
        VN,
        UNLOAD
    }
}
