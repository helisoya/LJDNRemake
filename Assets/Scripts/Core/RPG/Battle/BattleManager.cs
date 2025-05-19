using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;

/// <summary>
/// Handles the game's battles
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("Battles")]
    [SerializeField] private RPGItem defaultAttack;
    [SerializeField] private BattleGUI gui;

    [Header("DEBUG")]
    [SerializeField] private BattleData data;

    private List<CharacterData> players;
    private List<CharacterData> ennemies;
    private List<CharacterData> order;
    private int currentOrderIdx;

    void Start()
    {
        players = new List<CharacterData>();
        ennemies = new List<CharacterData>();
        order = new List<CharacterData>();
        gui.SetPlayerScreenActive(false);

        GameManager.GetRPGManager().AddItemToInventory("ITEM_POTION", 4);
        LoadBattle(data);
    }

    /// <summary>
    /// Wins the battle
    /// </summary>
    public void WinBattle()
    {

    }

    /// <summary>
    /// Loses the battle
    /// </summary>
    public void LoseBattle()
    {

    }

    /// <summary>
    /// Loads a new battle
    /// </summary>
    /// <param name="data">The battle's data</param>
    public void LoadBattle(BattleData data)
    {
        this.data = data;
        players.Clear();
        ennemies.Clear();
        order.Clear();
        currentOrderIdx = 0;

        List<RPGItem> items = new List<RPGItem>();
        RPGItem item;
        foreach (InventorySlot slot in GameManager.GetRPGManager().GetInventory())
        {
            item = GameManager.GetRPGManager().GetItem(slot.itemID);
            if (item.type == RPGItem.ItemType.USABLE_COMBAT || item.type == RPGItem.ItemType.USABLE_ALL)
            {
                items.Add(item);
            }
        }

        gui.SetCurrentItems(items);

        GenerateCharacters();
        NextTurn();
    }

    /// <summary>
    /// Checks if all datas in a list are dead
    /// </summary>
    /// <param name="datas">The data list</param>
    /// <returns>True if they are all dead</returns>
    private bool AllDead(List<CharacterData> datas)
    {
        foreach (CharacterData data in datas)
        {
            if (!data.dead) return false;
        }
        return true;
    }

    /// <summary>
    /// Handles the next turn
    /// </summary>
    public void NextTurn()
    {
        if (AllDead(ennemies))
        {
            WinBattle();
            return;
        }
        else if (AllDead(players))
        {
            LoseBattle();
            return;
        }

        CharacterData data = order[currentOrderIdx];

        if (data.dead)
        {
            EndTurn();
            return;
        }

        data.blocking = false;

        if (data.isPlayer)
        {
            gui.SetPlayerIconActive(players.FindIndex(ch => ch == data));
            List<RPGItem> skills = new List<RPGItem>();
            skills.Add(defaultAttack);
            if (!string.IsNullOrEmpty(data.characterData.GetData().weapon))
            {
                foreach (string skillID in GameManager.GetRPGManager().GetItem(data.characterData.GetData().weapon).weaponSkills)
                {
                    skills.Add(GameManager.GetRPGManager().GetItem(skillID));
                }
            }

            gui.SetCurrentSkills(skills);

            gui.SetPlayerScreenActive(true);
            gui.OpenMainScreen();
        }
        else
        {
            HandleAI(data);
        }
    }

    /// <summary>
    /// Use an item on multiple targets
    /// </summary>
    /// <param name="item">The item to use</param>
    /// <param name="targets">The targets</param>
    /// <param name="isFromInventory">True if the item is from the inventory</param>
    public void UseItemOn(RPGItem item, List<CharacterData> targets, bool isFromInventory)
    {
        EndTurn();
    }

    /// <summary>
    /// Gets available targets using an item 
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>The targets</returns>
    public List<List<CharacterData>> GetAvailableTargets(RPGItem item)
    {
        List<List<CharacterData>> result = new List<List<CharacterData>>();

        List<CharacterData> allies = order[currentOrderIdx].isPlayer ? players : ennemies;
        List<CharacterData> foes = order[currentOrderIdx].isPlayer ? ennemies : players;

        List<CharacterData> current;
        switch (item.targetType)
        {
            case RPGItem.TargetType.ALL:
                current = new List<CharacterData>();
                foreach (CharacterData character in allies) if (!character.dead) current.Add(character);
                foreach (CharacterData character in foes) if (!character.dead) current.Add(character);
                result.Add(current);
                break;
            case RPGItem.TargetType.ONEALLY:
                foreach (CharacterData character in allies)
                {
                    if (!character.dead)
                    {
                        current = new List<CharacterData>();
                        current.Add(character);
                        result.Add(current);
                    }
                }
                break;
            case RPGItem.TargetType.ALLALLY:
                current = new List<CharacterData>();
                foreach (CharacterData character in allies) if (!character.dead) current.Add(character);
                result.Add(current);
                break;
            case RPGItem.TargetType.ONEFOE:
                foreach (CharacterData character in foes)
                {
                    if (!character.dead)
                    {
                        current = new List<CharacterData>();
                        current.Add(character);
                        result.Add(current);
                    }
                }
                break;
            case RPGItem.TargetType.ALLFOE:
                current = new List<CharacterData>();
                foreach (CharacterData character in foes) if (!character.dead) current.Add(character);
                result.Add(current);
                break;
            case RPGItem.TargetType.SELF:
                current = new List<CharacterData>();
                current.Add(order[currentOrderIdx]);
                result.Add(current);
                break;
        }
        return result;
    }

    /// <summary>
    /// Ends the current turn
    /// </summary>
    public void EndTurn()
    {
        gui.UpdateAllPlayerIcons();
        gui.SetPlayerIconActive(-1);
        gui.SetPlayerScreenActive(false);
        currentOrderIdx = (currentOrderIdx + 1) % order.Count;
        NextTurn();
    }


    /// <summary>
    /// Use the block action for this turn
    /// </summary>
    public void BlockForTurn()
    {
        order[currentOrderIdx].blocking = true;
        EndTurn();
    }

    /// <summary>
    /// Handles an AI's turn
    /// </summary>
    /// <param name="data">The AI's data</param>
    private void HandleAI(CharacterData data)
    {
        EndTurn();
    }


    /// <summary>
    /// Generate the different characters
    /// </summary>
    private void GenerateCharacters()
    {
        List<int> playersIndex = GameManager.GetRPGManager().GetFollowers();
        RPGCharacter character;
        BattleCharacter visual;

        Vector3 from = new Vector3(0, 0, 0);
        Vector3 to = new Vector3(40, 0, 0);
        int i = 0;
        Quaternion rotation = Quaternion.Euler(0, 0, 0);

        foreach (int index in playersIndex)
        {
            character = GameManager.GetRPGManager().GetCharacter(index);
            visual = Instantiate(Resources.Load<BattleCharacter>("RPG/Battles/Characters/" + character.GetData().ID));
            if (!string.IsNullOrEmpty(character.GetData().weapon)) visual.SetWeapon(character.GetData().weapon);
            visual.transform.position = Vector3.Lerp(from, to, (i + 0.5f) / playersIndex.Count);
            visual.transform.rotation = rotation;

            i++;

            players.Add(new CharacterData
            {
                characterData = character,
                characterVisual = visual,
                dead = false,
                isPlayer = true,
                blocking = false
            });
        }

        from = new Vector3(0, 0, 10);
        to = new Vector3(40, 0, 10);
        i = 0;
        rotation = Quaternion.Euler(0, 180, 0);

        foreach (RPGCharacterDataInterface dataInterface in data.ennemies)
        {
            character = new RPGCharacter(dataInterface.data.Clone());
            character.SetHealthToMax();
            character.SetSPToMax();
            visual = Instantiate(Resources.Load<BattleCharacter>("RPG/Battles/Characters/" + character.GetData().ID));
            if (!string.IsNullOrEmpty(character.GetData().weapon)) visual.SetWeapon(character.GetData().weapon);
            visual.transform.position = Vector3.Lerp(from, to, (i + 0.5f) / data.ennemies.Length);
            visual.transform.rotation = rotation;

            i++;

            ennemies.Add(new CharacterData
            {
                characterData = character,
                characterVisual = visual,
                dead = false,
                isPlayer = false,
                blocking = false
            });
        }

        order.AddRange(players);
        order.AddRange(ennemies);
        order.Sort((c1, c2) => -c1.characterData.evasion.CompareTo(c2.characterData.evasion));

        gui.SetPlayerIcons(players);
    }

    public class CharacterData
    {
        public RPGCharacter characterData;
        public BattleCharacter characterVisual;
        public bool dead;
        public bool isPlayer;
        public bool blocking;
    }

}
