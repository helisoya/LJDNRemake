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

    [Header("Items")]
    [SerializeField] private string itemsPath = "RPG/Items/";
    private Dictionary<string, RPGItem> items;


    /// <summary>
    /// Initalize the manager
    /// </summary>
    public void Init()
    {
        characters = new List<RPGCharacter>();
        items = new Dictionary<string, RPGItem>();
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

    /// <summary>
    /// Gets an RPG item, load to memory if needed
    /// </summary>
    /// <param name="ID">The Item's ID</param>
    /// <returns>The item</returns>
    public RPGItem GetItem(string ID)
    {
        RPGItem item;
        if (items.TryGetValue(ID, out item)) return item;

        item = Resources.Load<RPGItem>(itemsPath + ID);
        items.Add(ID, item);
        return item;
    }
}
