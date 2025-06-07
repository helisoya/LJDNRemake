using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a dungeon's data
/// </summary>
[CreateAssetMenu(menuName = "LJDN/Dungeon Data")]
public class DungeonData : ScriptableObject
{
    public BattleData[] encounters;
    public string battleBackground;
}
