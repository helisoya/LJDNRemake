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
    private List<int> followers;

    [Header("Items")]
    [SerializeField] private string itemsPath = "RPG/Items/";
    private List<InventorySlot> inventory;
    private Dictionary<string, RPGItem> items;


    /// <summary>
    /// Initalize the manager
    /// </summary>
    public void Init()
    {
        followers = new List<int>();
        characters = new List<RPGCharacter>();
        items = new Dictionary<string, RPGItem>();
        inventory = new List<InventorySlot>();
        Reset();
    }

    /// <summary>
    /// Resets the manager
    /// </summary>
    public void Reset()
    {
        followers.Clear();
        followers.Add(0);
        characters.Clear();
        foreach (RPGCharacterDataInterface character in defaultCharacters)
        {
            characters.Add(new RPGCharacter(character.data.Clone()));
        }
    }

    /// <summary>
    /// Gets the current followers (player included)
    /// </summary>
    /// <returns>The current followers</returns>
    public List<int> GetFollowers()
    {
        return followers;
    }

    /// <summary>
    /// Sets the current followers
    /// </summary>
    /// <param name="followers">The new followers</param>
    public void SetFollowers(List<int> followers)
    {
        this.followers = followers;
    }

    /// <summary>
    /// Adds a new follower
    /// </summary>
    /// <param name="characterID">The follower's ID</param>
    public void AddFollower(string characterID)
    {
        int characterIndex = characters.FindIndex(character => character.GetData().ID.Equals(characterID));
        if (characterIndex == -1) return;

        if (!followers.Contains(characterIndex)) followers.Add(characterIndex);
    }

    /// <summary>
    /// Removes a follower
    /// </summary>
    /// <param name="characterID">The follower's ID</param>
    public void RemoveFollower(string characterID)
    {
        int characterIndex = characters.FindIndex(character => character.GetData().ID.Equals(characterID));
        if (characterIndex <= 0) return;

        if (followers.Contains(characterIndex)) followers.Remove(characterIndex);
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
    /// Gets an RPG Character 
    /// </summary>
    /// <param name="index">The Character's index</param>
    /// <returns>The character if it exists</returns>
    public RPGCharacter GetCharacter(int index)
    {
        if (characters.Count < index) return null;
        return characters[index];
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

    /// <summary>
    /// Gets the current inventory
    /// </summary>
    /// <returns>The inventory</returns>
    public List<InventorySlot> GetInventory()
    {
        return inventory;
    }

    /// <summary>
    /// Sets the inventory's value
    /// </summary>
    /// <param name="newInventory">The new inventory</param>
    public void SetInventory(List<InventorySlot> newInventory)
    {
        inventory = newInventory;
    }

    /// <summary>
    /// Adds an item to the inventory
    /// </summary>
    /// <param name="ID">The item's ID</param>
    /// <param name="amount">The amount to add</param>
    public void AddItemToInventory(string ID, int amount)
    {
        if (amount == 0) return; // Nothing to add / remove

        int idx = inventory.FindIndex(c => c.itemID.Equals(ID));
        InventorySlot slot = inventory[idx];
        if (slot == null && amount < 0) return; // Not in inventory & Trying to remove
        if (slot == null)
        {
            // Not in inventory & trying to add
            slot = new InventorySlot
            {
                itemID = ID,
                itemAmount = amount
            };
            inventory.Add(slot);
        }
        else
        {
            // Adding / Removing from an existing slot
            slot.itemAmount = Mathf.Clamp(slot.itemAmount + amount, 0, 999);
            if (slot.itemAmount == 0 && amount < 0) inventory.RemoveAt(idx);
        }
    }

    /// <summary>
    /// Gets an item's amount in inventory
    /// </summary>
    /// <param name="ID">The item's ID</param>
    /// <returns>The amount in inventory. 0 if not in inventory</returns>
    public int GetAmountInInventory(string ID)
    {
        InventorySlot slot = inventory.Find(c => c.itemID.Equals(ID));
        if (slot != null) return slot.itemAmount;
        return 0;
    }
}
