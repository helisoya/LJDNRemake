using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Handles the game's battles
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("Battles")]
    [SerializeField] private RPGItem defaultAttack;
    [SerializeField] private BattleGUI gui;

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform freeLookTransform;

    [Header("Placements")]
    [SerializeField] private float posStart = 0;
    [SerializeField] private float posEnd = 40;
    [SerializeField] private float ennemyDistance = 10;
    [SerializeField] private float cameraUpDistance = 5;

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
    /// Checks if the current player has more SP than the specified amount
    /// </summary>
    /// <param name="amount">The amount of SP to check</param>
    /// <returns>True if the current player has more SP than the amount given</returns>
    public bool CurrentPlayerHasMoreSPThan(int amount)
    {
        return order[currentOrderIdx].characterData.currentSP >= amount;
    }

    /// <summary>
    /// Sets the current camera target
    /// </summary>
    /// <param name="target">The new target</param>
    public void SetCameraTarget(Transform target)
    {
        virtualCamera.LookAt = target;
    }

    /// <summary>
    /// Sets the current camera target
    /// </summary>
    /// <param name="position">The new target's position</param>
    public void SetCameraTarget(Vector3 position)
    {
        freeLookTransform.position = position;
        SetCameraTarget(freeLookTransform);
    }

    /// <summary>
    /// Sets the current camera target to the current player
    /// </summary>
    public void SetCameraTargetToCurrentPlayer()
    {
        SetCameraTarget(order[currentOrderIdx].characterVisual.transform);
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

        List<ItemData> items = new List<ItemData>();
        RPGItem item;
        foreach (InventorySlot slot in GameManager.GetRPGManager().GetInventory())
        {
            item = GameManager.GetRPGManager().GetItem(slot.itemID);
            if (item.type == RPGItem.ItemType.USABLE_COMBAT || item.type == RPGItem.ItemType.USABLE_ALL)
            {
                items.Add(new ItemData { item = item, amountInInventory = slot.itemAmount });
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
        SetCameraTarget(data.characterVisual.transform);

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
        if (isFromInventory)
        {
            GameManager.GetRPGManager().AddItemToInventory(item.ID, -1);
        }

        bool isHealing = item.damageType == RPGItem.DamageType.HEAL;
        order[currentOrderIdx].characterData.AddSP(-(int)item.costSP);


        int damage = item.attackEquation == RPGItem.EquationType.REPLACE ?
            (int)item.attackValue // Attack is set
            : Mathf.FloorToInt(order[currentOrderIdx].characterData.attack * item.attackValue); // Defense is set
        if (isHealing) damage = -damage;

        int defense;

        foreach (CharacterData data in targets)
        {
            // Evasion
            if (!isHealing && Random.Range(0.0f, 1.0f) <= data.characterData.evasion) continue;

            if (isHealing) defense = 0; // No resistance on heal
            else if (item.defenseEquation == RPGItem.EquationType.REPLACE) defense = (int)item.defenseValue; // Defense is set
            else defense = Mathf.FloorToInt(data.characterData.defense * item.defenseValue); // Defense is normal

            int actualDamage = Mathf.Clamp(damage - defense, isHealing ? -999 : 2, 999);
            print(actualDamage + "(" + damage + "/" + defense + ")");
            data.characterData.AddHealth(-actualDamage);

            if (data.characterData.currentHealth == 0)
            {
                data.dead = true;
            }
        }

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
        virtualCamera.transform.position = new Vector3(posStart + posEnd / 2.0f, cameraUpDistance, ennemyDistance / 2.0f);

        List<int> playersIndex = GameManager.GetRPGManager().GetFollowers();
        RPGCharacter character;
        BattleCharacter visual;

        Vector3 from = new Vector3(posStart, 0, 0);
        Vector3 to = new Vector3(posEnd, 0, 0);
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

        from = new Vector3(posStart, 0, ennemyDistance);
        to = new Vector3(posEnd, 0, ennemyDistance);
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
        players.Sort((c1, c2) => -c1.characterData.evasion.CompareTo(c2.characterData.evasion));
        ennemies.Sort((c1, c2) => -c1.characterData.evasion.CompareTo(c2.characterData.evasion));

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

    public class ItemData
    {
        public RPGItem item;
        public int amountInInventory;
    }
}
