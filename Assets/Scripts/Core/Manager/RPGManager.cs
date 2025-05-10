using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles the game's RPG Elements
/// </summary>
public class RPGManager : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private RPGCharacterDataInterface[] defaultCharacters;
    private List<RPGCharacter> characters;

    /// <summary>
    /// Initalize the manager
    /// </summary>
    public void Init()
    {
        characters = new List<RPGCharacter>();
        Reset();
    }

    /// <summary>
    /// Resets the manager
    /// </summary>
    public void Reset()
    {
        characters.Clear();
        foreach (RPGCharacterDataInterface character in defaultCharacters)
        {
            characters.Add(new RPGCharacter(character.data.Clone()));
        }
    }

    /// <summary>
    /// Gets the game's characters
    /// </summary>
    /// <returns>The characters</returns>
    public List<RPGCharacterData> GetCharacters()
    {
        List<RPGCharacterData> datas = new List<RPGCharacterData>();
        foreach (RPGCharacter character in characters)
        {
            datas.Add(character.GetData());
        }
        return datas;
    }

    /// <summary>
    /// Gets an RPG Character 
    /// </summary>
    /// <param name="ID">The Character's ID</param>
    /// <returns>The character if it exists</returns>
    public RPGCharacter GetCharacter(string ID)
    {
        RPGCharacter character = characters.Find(c => c.GetData().ID.Equals(ID));
        if (character != null) return character;

        return null;
    }

    /// <summary>
    /// Loads the game's character from a list
    /// </summary>
    /// <param name="list">The list</param>
    public void LoadCharactersFromList(List<RPGCharacterData> list)
    {
        foreach (RPGCharacterData character in list)
        {
            characters.Find(other => other.GetData().ID.Equals(character.ID))?.GetData().Copy(character);
        }
    }
}
