using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles the game's RPG Elements
/// </summary>
public class RPGManager : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private List<RPGCharacterData> defaultCharacters;
    private List<RPGCharacterData> characters;

    /// <summary>
    /// Initalize the manager
    /// </summary>
    public void Init()
    {
        characters = new List<RPGCharacterData>();
        Reset();
    }

    /// <summary>
    /// Resets the manager
    /// </summary>
    public void Reset()
    {
        characters.Clear();
        foreach (RPGCharacterData character in defaultCharacters)
        {
            characters.Add(character.Clone());
        }
    }

    /// <summary>
    /// Gets the game's characters
    /// </summary>
    /// <returns>The characters</returns>
    public List<RPGCharacterData> GetCharacters()
    {
        return characters;
    }

    /// <summary>
    /// Loads the game's character from a list
    /// </summary>
    /// <param name="characters">The list</param>
    public void LoadCharactersFromList(List<RPGCharacterData> characters)
    {
        foreach (RPGCharacterData character in characters)
        {
            characters.Find(other => other.ID.Equals(character.ID))?.Copy(character);
        }
    }
}
