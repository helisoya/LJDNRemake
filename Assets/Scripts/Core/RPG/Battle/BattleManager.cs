using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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

    private BattleData currentData;
    private List<CharacterData> players;
    private List<CharacterData> ennemies;
    private List<CharacterData> order;
    private int currentOrderIdx;
    private Coroutine routineAttack = null;

    void Start()
    {
        players = new List<CharacterData>();
        ennemies = new List<CharacterData>();
        order = new List<CharacterData>();
        gui.SetPlayerScreenActive(false);

        GameManager.GetRPGManager().AddItemToInventory("ITEM_POTION", 4);
        GameManager.GetRPGManager().AddItemToInventory("ITEM_REVIVE", 4);
        GameManager.GetRPGManager().AddItemToInventory("ITEM_POISON", 4);
        GameManager.GetRPGManager().AddItemToInventory("ITEM_CURE", 4);
        GameManager.GetRPGManager().AddItemToInventory("ITEM_SLEEP", 4);
        GameManager.GetRPGManager().AddItemToInventory("ITEM_ATTACK_UP", 4);
        GameManager.GetRPGManager().AddItemToInventory("ITEM_DEFENSE_UP", 4);
        GameManager.GetRPGManager().AddItemToInventory("ITEM_CONFUSE", 4);
        LoadBattle(data);
    }


    /// <summary>
    /// Routine for losing a battle
    /// </summary>   
    IEnumerator Routine_Lose()
    {
        gui.GetActionText().ResetAdditive();
        gui.GetActionText().SetNewKey("battle_lost");
        gui.SetActionTextVisible(true);
        yield return new WaitForSeconds(1);

        gui.FadeTo(1);
        yield return new WaitForEndOfFrame();
        while (gui.fading)
        {
            yield return new WaitForEndOfFrame();
        }

        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Routine for winning a battle
    /// </summary>
    IEnumerator Routine_Win()
    {
        gui.GetActionText().ResetAdditive();
        gui.GetActionText().SetNewKey("battle_won");
        gui.SetActionTextVisible(true);
        yield return new WaitForSeconds(1);

        foreach (CharacterData ennemy in ennemies)
        {
            foreach (CharacterData follower in players)
            {
                follower.characterData.GetData().exp += ennemy.characterData.GetData().exp;
            }
        }

        foreach (CharacterData follower in players)
        {
            if (follower.characterData.canLevelUp)
            {

                SetCameraTarget(follower.characterVisual.transform);
                string playerName = order[currentOrderIdx].characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(order[currentOrderIdx].characterData.GetData().ID + "_name");
                gui.GetActionText().SetParameters("", " ", "", "");
                gui.GetActionText().SetValue(playerName, null, false);
                gui.GetActionText().SetNewKey("battle_levelUp");
                gui.SetActionTextVisible(true);
                yield return new WaitForSeconds(1.0f);

                gui.OpenLevelUpMenu(follower.characterData);
                yield return new WaitForEndOfFrame();
                while (gui.levelingUp) yield return new WaitForEndOfFrame();
            }
        }

        gui.SetActionTextVisible(false);
        gui.FadeTo(1);
        yield return new WaitForEndOfFrame();
        while (gui.fading)
        {
            yield return new WaitForEndOfFrame();
        }

        if (data.closeType == BattleData.CloseType.VN)
        {
            GameManager.instance.SetNextChapter(data.nextChapter);
            SceneManager.LoadScene("VN");
        }
        else
        {
            SceneManager.UnloadSceneAsync("Battle");
        }
    }

    /// <summary>
    /// Wins the battle
    /// </summary>
    public void WinBattle()
    {
        StopAllCoroutines();
        routineAttack = StartCoroutine(Routine_Win());
    }

    /// <summary>
    /// Loses the battle
    /// </summary>
    public void LoseBattle()
    {
        StopAllCoroutines();
        routineAttack = StartCoroutine(Routine_Lose());
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
        this.currentData = data;
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

        if (data.blocking)
        {
            data.blocking = false;
            data.characterVisual.SetBlocking(false);
        }
        SetCameraTarget(data.characterVisual.transform);

        if (data.status.Find(entry => entry.status == RPGCharacterData.StatusType.SLEEP) != null)
        {
            StopAllCoroutines();
            routineAttack = StartCoroutine(Routine_Sleep());
            return;
        }
        else if (data.status.Find(entry => entry.status == RPGCharacterData.StatusType.CONFUSED) != null)
        {
            StopAllCoroutines();
            routineAttack = StartCoroutine(Routine_Confused());
            return;
        }

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
    /// Routine for blocking
    /// </summary>
    private IEnumerator Routine_Block()
    {
        // Play animation
        SetCameraTargetToCurrentPlayer();
        string playerName = order[currentOrderIdx].characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(order[currentOrderIdx].characterData.GetData().ID + "_name");
        gui.GetActionText().SetParameters("", " ", "", "");
        gui.GetActionText().SetValue(playerName, null, false);
        gui.GetActionText().SetNewKey("battle_action_block");
        gui.SetActionTextVisible(true);

        yield return new WaitForSeconds(1.0f);

        yield return Routine_Status(true);
        routineAttack = null;
        EndTurn();
    }

    /// <summary>
    /// Use an item on multiple targets
    /// </summary>
    /// <param name="item">The item to use</param>
    /// <param name="targets">The targets</param>
    /// <param name="isFromInventory">True if the item is from the inventory</param>
    public void UseItemOn(RPGItem item, List<CharacterData> targets, bool isFromInventory)
    {
        StopAllCoroutines();
        routineAttack = StartCoroutine(Routine_UseItemOn(item, targets, isFromInventory));
    }

    /// <summary>
    /// Use an item on multiple targets (Internal routine)
    /// </summary>
    /// <param name="item">The item to use</param>
    /// <param name="targets">The targets</param>
    /// <param name="isFromInventory">True if the item is from the inventory</param>
    private IEnumerator Routine_UseItemOn(RPGItem item, List<CharacterData> targets, bool isFromInventory)
    {
        if (isFromInventory)
        {
            GameManager.GetRPGManager().AddItemToInventory(item.ID, -1);
        }


        bool isHealing = item.damageType == RPGItem.DamageType.HEAL || item.damageType == RPGItem.DamageType.HEAL_DEAD;
        bool canResurect = item.damageType == RPGItem.DamageType.HEAL_DEAD;
        order[currentOrderIdx].characterData.AddSP(-(int)item.costSP);


        string playerName = order[currentOrderIdx].characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(order[currentOrderIdx].characterData.GetData().ID + "_name");
        gui.GetActionText().SetParameters("", " ", " ", "");
        gui.GetActionText().SetValue(playerName, Locals.GetLocal(item.ID + "_name"), false);
        gui.GetActionText().SetNewKey("battle_action_attack");
        gui.SetActionTextVisible(true);

        SetCameraTargetToCurrentPlayer();
        if (order[currentOrderIdx].isPlayer) gui.GetPlayerIcon(order[currentOrderIdx].characterData.GetData().ID).UpdateIcon();
        order[currentOrderIdx].characterVisual.PlayAnimation(item.animationName);
        yield return new WaitForSeconds(1f);


        int damage = item.attackEquation == RPGItem.EquationType.REPLACE ?
            (int)item.attackValue // Attack is set
            : Mathf.RoundToInt(order[currentOrderIdx].characterData.attack * (item.attackValue + Random.Range(-0.1f, 0.1f))); // Defense is set

        int refValue = damage;
        if (order[currentOrderIdx].status.Find(status => status.status == RPGCharacterData.StatusType.ATTACK_UP) != null) damage += Mathf.RoundToInt(refValue * 0.5f);
        if (order[currentOrderIdx].status.Find(status => status.status == RPGCharacterData.StatusType.ATTACK_DOWN) != null) damage -= Mathf.RoundToInt(refValue * 0.5f);
        if (isHealing) damage = -damage;

        int defense;

        foreach (CharacterData data in targets)
        {
            // Evasion
            if (!isHealing && Random.Range(0.0f, 1.0f) <= data.characterData.evasion)
            {
                SetCameraTarget(data.characterVisual.transform);
                yield return new WaitForSeconds(0.25f);
                data.characterVisual.TriggerEvassion();
                yield return new WaitForSeconds(1f);
                continue;
            }

            if (isHealing) defense = 0; // No resistance on heal
            else if (item.defenseEquation == RPGItem.EquationType.REPLACE) defense = (int)item.defenseValue; // Defense is set
            else defense = Mathf.RoundToInt(data.characterData.defense * item.defenseValue) * (data.blocking ? 2 : 1); // Defense is normal

            refValue = defense;
            if (data.status.Find(status => status.status == RPGCharacterData.StatusType.DEFENSE_UP) != null) defense += Mathf.RoundToInt(refValue * 0.5f);
            if (data.status.Find(status => status.status == RPGCharacterData.StatusType.DEFENSE_DOWN) != null) defense -= Mathf.RoundToInt(refValue * 0.5f);

            int actualDamage = Mathf.Clamp(damage - defense, isHealing ? -999 : 2, 999);
            print(actualDamage + "(" + damage + "/" + defense + ")");

            SetCameraTarget(data.characterVisual.transform);
            yield return new WaitForSeconds(0.25f);

            if (data.dead && canResurect)
            {
                data.dead = false;
                data.characterVisual.PlayAnimation("Idle");
            }

            // Actual Attack
            data.characterData.AddHealth(-actualDamage);
            if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateIcon();
            if (!isHealing && data.status.Find(entry => entry.status == RPGCharacterData.StatusType.SLEEP) == null) data.characterVisual.TriggerDamage();

            data.characterVisual.SetHealthBarVisible(true);
            data.characterVisual.setHealthBarFillAmount(data.characterData.currentHealth / (float)data.characterData.maxHealth, false);
            yield return new WaitForEndOfFrame();
            while (data.characterVisual.healthBarFilling)
            {
                yield return new WaitForEndOfFrame();
            }

            // Death
            if (data.characterData.currentHealth == 0)
            {
                data.dead = true;
                data.blocking = false;
                data.status.Clear();
                data.characterVisual.SetBlocking(false);
                data.characterVisual.TriggerDeath();
                if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateIcon();
            }
            else
            {
                // Status 
                if (item.statusEffect != RPGItem.StatusEffect.NOTHING && Random.Range(0.0f, 1.0f) <= item.statusChance)
                {
                    int index = data.status.FindIndex(entry => entry.status == item.linkedStatus);
                    string otherName = data.characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(data.characterData.GetData().ID + "_name");

                    if (item.statusEffect == RPGItem.StatusEffect.CURE)
                    {
                        if (index != -1)
                        {
                            data.status.RemoveAt(index);

                            gui.GetActionText().SetParameters("", " ", " ", "");
                            gui.GetActionText().SetValue(otherName, Locals.GetLocal("ailement_" + item.linkedStatus.ToString().ToLower()), false);
                            gui.GetActionText().SetNewKey("battle_action_cured");
                            yield return new WaitForSeconds(1.0f);
                            if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateIcon();
                        }
                    }
                    else
                    {
                        if (index != -1) data.status[index].remainingTurns += item.statusLength;
                        else
                        {
                            if (item.linkedStatus == RPGCharacterData.StatusType.SLEEP) data.characterVisual.TriggerDeath();
                            data.status.Add(new StatusData { status = item.linkedStatus, remainingTurns = item.statusLength });
                        }

                        gui.GetActionText().SetParameters("", " ", " ", "");
                        gui.GetActionText().SetValue(otherName, Locals.GetLocal("ailement_" + item.linkedStatus.ToString().ToLower()), false);
                        gui.GetActionText().SetNewKey("battle_action_ailement");
                        yield return new WaitForSeconds(1.0f);
                        if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateIcon();
                    }
                }
            }

            yield return new WaitForSeconds(1f);

            data.characterVisual.SetHealthBarVisible(false);
        }

        yield return Routine_Status(false);
        routineAttack = null;
        EndTurn();
    }

    /// <summary>
    /// Routine for confusion
    /// </summary>
    private IEnumerator Routine_Confused()
    {
        string playerName = order[currentOrderIdx].characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(order[currentOrderIdx].characterData.GetData().ID + "_name");

        yield return new WaitForSeconds(0.25f);
        gui.GetActionText().SetParameters("", " ", " ", "");
        gui.GetActionText().SetValue(playerName, null, false);
        gui.GetActionText().SetNewKey("battle_action_confused");
        gui.SetActionTextVisible(true);

        yield return new WaitForSeconds(1f);

        gui.SetActionTextVisible(false);

        List<CharacterData> data = new List<CharacterData>();
        List<CharacterData> candidates = new List<CharacterData>();
        foreach (CharacterData entity in order)
        {
            if (!entity.dead && entity != order[currentOrderIdx]) candidates.Add(entity);
        }

        if (candidates.Count > 0)
        {
            data.Add(candidates[Random.Range(0, candidates.Count)]);
            UseItemOn(defaultAttack, data, false);
        }
        else
        {
            BlockForTurn();
        }
    }

    /// <summary>
    /// Routine for sleeping
    /// </summary>
    private IEnumerator Routine_Sleep()
    {
        string playerName = order[currentOrderIdx].characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(order[currentOrderIdx].characterData.GetData().ID + "_name");

        yield return new WaitForSeconds(0.25f);
        gui.GetActionText().SetParameters("", " ", " ", "");
        gui.GetActionText().SetValue(playerName, null, false);
        gui.GetActionText().SetNewKey("battle_action_sleep");
        gui.SetActionTextVisible(true);

        yield return new WaitForSeconds(1f);

        gui.SetActionTextVisible(false);

        yield return Routine_Status(false);
        routineAttack = null;
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
                foreach (CharacterData character in allies) if ((!character.dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (character.dead && item.damageType == RPGItem.DamageType.HEAL_DEAD)) current.Add(character);
                foreach (CharacterData character in foes) if ((!character.dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (character.dead && item.damageType == RPGItem.DamageType.HEAL_DEAD)) current.Add(character);
                if (current.Count > 0) result.Add(current);
                break;
            case RPGItem.TargetType.ONEALLY:
                foreach (CharacterData character in allies)
                {
                    if ((!character.dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (character.dead && item.damageType == RPGItem.DamageType.HEAL_DEAD))
                    {
                        current = new List<CharacterData>();
                        current.Add(character);
                        result.Add(current);
                    }
                }
                break;
            case RPGItem.TargetType.ALLALLY:
                current = new List<CharacterData>();
                foreach (CharacterData character in allies) if ((!character.dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (character.dead && item.damageType == RPGItem.DamageType.HEAL_DEAD)) current.Add(character);
                if (current.Count > 0) result.Add(current);
                break;
            case RPGItem.TargetType.ONEFOE:
                foreach (CharacterData character in foes)
                {
                    if ((!character.dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (character.dead && item.damageType == RPGItem.DamageType.HEAL_DEAD))
                    {
                        current = new List<CharacterData>();
                        current.Add(character);
                        result.Add(current);
                    }
                }
                break;
            case RPGItem.TargetType.ALLFOE:
                current = new List<CharacterData>();
                foreach (CharacterData character in foes) if ((!character.dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (character.dead && item.damageType == RPGItem.DamageType.HEAL_DEAD)) current.Add(character);
                if (current.Count > 0) result.Add(current);
                break;
            case RPGItem.TargetType.SELF:

                if ((!order[currentOrderIdx].dead && item.damageType != RPGItem.DamageType.HEAL_DEAD) ||
                    (order[currentOrderIdx].dead && item.damageType == RPGItem.DamageType.HEAL_DEAD))
                {
                    current = new List<CharacterData>();
                    current.Add(order[currentOrderIdx]);
                    result.Add(current);
                }

                break;
        }
        return result;
    }

    /// <summary>
    /// Routine for status effects at the end of a turn
    /// </summary>
    /// <param name="alreadyLookingAtTarget">True if the camera is already looking a the current player</param>
    private IEnumerator Routine_Status(bool alreadyLookingAtTarget)
    {
        string playerName = order[currentOrderIdx].characterData.GetData().ID.Equals("PLAYER") ? GameManager.GetSaveManager().GetItem("playerName") : Locals.GetLocal(order[currentOrderIdx].characterData.GetData().ID + "_name");


        CharacterData data = order[currentOrderIdx];
        int i = 0;
        bool poisoned = false;
        bool lookingAtTarget = alreadyLookingAtTarget;
        while (i < data.status.Count)
        {
            data.status[i].remainingTurns--;
            if (data.status[i].remainingTurns <= 0)
            {

                if (!lookingAtTarget)
                {
                    lookingAtTarget = true;
                    SetCameraTargetToCurrentPlayer();
                    yield return new WaitForSeconds(0.25f);
                }

                if (data.status[i].status == RPGCharacterData.StatusType.SLEEP) data.characterVisual.PlayAnimation("Idle");

                gui.GetActionText().SetParameters("", " ", " ", "");
                gui.GetActionText().SetValue(playerName, Locals.GetLocal("ailement_" + data.status[i].status.ToString().ToLower()), false);
                gui.GetActionText().SetNewKey("battle_action_cured");
                gui.SetActionTextVisible(true);

                yield return new WaitForSeconds(1.0f);

                data.status.RemoveAt(i);
            }
            else
            {
                if (data.status[i].status == RPGCharacterData.StatusType.POISON)
                {
                    poisoned = true;
                }
                i++;
            }
        }

        if (poisoned)
        {
            if (!lookingAtTarget)
            {
                lookingAtTarget = true;
                SetCameraTargetToCurrentPlayer();
                yield return new WaitForSeconds(0.25f);
            }

            gui.GetActionText().SetParameters("", " ", " ", "");
            gui.GetActionText().SetValue(playerName, null, false);
            gui.GetActionText().SetNewKey("battle_action_poison");
            gui.SetActionTextVisible(true);

            data.characterData.AddHealth(-Mathf.RoundToInt(data.characterData.maxHealth * 0.1f));
            if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateIcon();
            data.characterVisual.SetHealthBarVisible(true);
            data.characterVisual.setHealthBarFillAmount(data.characterData.currentHealth / (float)data.characterData.maxHealth, false);
            yield return new WaitForEndOfFrame();
            while (data.characterVisual.healthBarFilling)
            {
                yield return new WaitForEndOfFrame();
            }

            if (data.characterData.currentHealth == 0)
            {
                data.dead = true;
                data.blocking = false;
                data.status.Clear();
                data.characterVisual.SetBlocking(false);
                data.characterVisual.TriggerDeath();
                if (data.isPlayer) gui.GetPlayerIcon(data.characterData.GetData().ID).UpdateIcon();
            }

            yield return new WaitForSeconds(1f);

            data.characterVisual.SetHealthBarVisible(false);
        }

        gui.SetActionTextVisible(false);
    }

    /// <summary>
    /// Ends the current turn
    /// </summary>
    public void EndTurn()
    {


        gui.SetActionTextVisible(false);
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
        order[currentOrderIdx].characterVisual.SetBlocking(true);
        StopAllCoroutines();
        routineAttack = StartCoroutine(Routine_Block());
    }

    /// <summary>
    /// Handles an AI's turn
    /// </summary>
    /// <param name="data">The AI's data</param>
    private void HandleAI(CharacterData data)
    {
        if (Random.Range(0.0f, 1.0f) <= 0.1f)
        {
            BlockForTurn();
            return;
        }

        RPGItem selectedItem = null;
        List<CharacterData> targets = null;
        List<RPGItem> skills = new List<RPGItem>();
        skills.Add(defaultAttack);
        if (!string.IsNullOrEmpty(data.characterData.GetData().weapon))
        {
            RPGItem weapon = GameManager.GetRPGManager().GetItem(data.characterData.GetData().weapon);
            RPGItem skillInstance;
            foreach (string skill in weapon.weaponSkills)
            {
                skillInstance = GameManager.GetRPGManager().GetItem(skill);
                if (data.characterData.currentSP >= skillInstance.costSP) skills.Add(skillInstance);
            }
        }

        selectedItem = skills[Random.Range(0, skills.Count)];
        List<List<CharacterData>> allTargets = GetAvailableTargets(selectedItem);

        if (allTargets.Count == 1) targets = allTargets[0];
        else
        {
            int healthScore = int.MaxValue;
            int currentScore;

            foreach (List<CharacterData> possibleTarget in allTargets)
            {
                currentScore = 0;
                foreach (CharacterData ch in possibleTarget)
                {
                    currentScore += ch.characterData.currentHealth;
                }
                print(currentScore + " " + healthScore);
                if (currentScore < healthScore || (currentScore == healthScore && Random.Range(0, 2) == 0))
                {
                    healthScore = currentScore;
                    targets = possibleTarget;
                }
            }
        }

        print("AI Selected " + selectedItem.ID);
        UseItemOn(selectedItem, targets, false);
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

        int avgLevel = 0;

        foreach (int index in playersIndex)
        {
            character = GameManager.GetRPGManager().GetCharacter(index);
            visual = Instantiate(Resources.Load<BattleCharacter>("RPG/Battles/Characters/" + character.GetData().ID));
            visual.setHealthBarFillAmount(character.currentHealth, true);
            if (!string.IsNullOrEmpty(character.GetData().weapon)) visual.SetWeapon(character.GetData().weapon);
            visual.transform.position = Vector3.Lerp(from, to, (i + 0.5f) / playersIndex.Count);
            visual.transform.rotation = rotation;

            avgLevel += character.GetData().level;

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

        avgLevel = Mathf.RoundToInt(avgLevel / (float)playersIndex.Count);

        from = new Vector3(posStart, 0, ennemyDistance);
        to = new Vector3(posEnd, 0, ennemyDistance);
        i = 0;
        rotation = Quaternion.Euler(0, 180, 0);

        foreach (RPGCharacterDataInterface dataInterface in currentData.ennemies)
        {
            character = new RPGCharacter(dataInterface.data.Clone());
            character.GetData().level = avgLevel;
            character.UpdateComputedStats();
            character.SetHealthToMax();
            character.SetSPToMax();
            visual = Instantiate(Resources.Load<BattleCharacter>("RPG/Battles/Characters/" + character.GetData().ID));
            if (!string.IsNullOrEmpty(character.GetData().weapon)) visual.SetWeapon(character.GetData().weapon);
            visual.transform.position = Vector3.Lerp(from, to, (i + 0.5f) / currentData.ennemies.Length);
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
        public List<StatusData> status;

        public CharacterData()
        {
            status = new List<StatusData>();
        }
    }

    public class StatusData
    {
        public RPGCharacterData.StatusType status;
        public int remainingTurns;
    }

    public class ItemData
    {
        public RPGItem item;
        public int amountInInventory;
    }
}
