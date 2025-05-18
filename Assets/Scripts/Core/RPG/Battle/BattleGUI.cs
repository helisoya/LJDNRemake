using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the GUI in a battle
/// </summary>
public class BattleGUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private BattleManager manager;

    [Header("Skills")]
    [SerializeField] private BattleSkillButton prefabSkillButton;
    [SerializeField] private Transform skillButtonsRoot;

    [Header("Items")]
    [SerializeField] private BattleItemButton prefabItemButton;
    [SerializeField] private Transform itemsButtonsRoot;

    [Header("Targets")]
    [SerializeField] private BattleTargetButton prefabTargetButton;
    [SerializeField] private Transform targetButtonsRoot;

    [Header("Screens")]
    [SerializeField] private GameObject playerScreen;
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject targetScreen;
    [SerializeField] private GameObject skillsScreen;
    [SerializeField] private GameObject itemsScreen;

    private RPGItem currentItem;
    private bool usingSkill;

    /// <summary>
    /// Sets if the player menu is visible or not
    /// </summary>
    /// <param name="value">True if the menu is visible</param>
    public void SetPlayerScreenActive(bool value)
    {
        playerScreen.SetActive(value);
    }

    /// <summary>
    /// Opens the player's main screen
    /// </summary>
    public void OpenMainScreen()
    {
        mainScreen.SetActive(true);
        skillsScreen.SetActive(false);
        itemsScreen.SetActive(false);
        targetScreen.SetActive(false);
    }

    /// <summary>
    /// Opens the inventory UI
    /// </summary>
    public void OpenInventory()
    {
        mainScreen.SetActive(false);

        // TODO

        itemsScreen.SetActive(true);
    }

    /// <summary>
    /// Callback for blocking incoming attacks
    /// </summary>
    public void Block()
    {
        manager.BlockForTurn();
    }

    /// <summary>
    /// Callback for opening the skills menu
    /// </summary>
    public void OpenSkills()
    {
        mainScreen.SetActive(false);
        skillsScreen.SetActive(true);
    }

    /// <summary>
    /// Callback for selecting a skill
    /// </summary>
    /// <param name="skill">The selected skill</param>
    public void SelectSkill(RPGItem skill)
    {
        currentItem = skill;
        usingSkill = true;

        skillsScreen.SetActive(false);
        OpenTargetScreen();
    }

    /// <summary>
    /// Callback for selecting an item
    /// </summary>
    /// <param name="skill">The selected item</param>
    public void SelectItem(RPGItem item)
    {
        currentItem = item;
        usingSkill = false;

        itemsScreen.SetActive(false);
        OpenTargetScreen();
    }

    /// <summary>
    /// Opens the target screen
    /// </summary>
    public void OpenTargetScreen()
    {
        targetScreen.SetActive(true);

        foreach (Transform child in targetButtonsRoot)
        {
            Destroy(child.gameObject);
        }

        List<List<BattleManager.CharacterData>> targets = manager.GetAvailableTargets(currentItem);

        foreach (List<BattleManager.CharacterData> target in targets)
        {
            Instantiate(prefabTargetButton, targetButtonsRoot).Init(target, this);
        }

        targetButtonsRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(
            targetButtonsRoot.GetComponent<RectTransform>().sizeDelta.x,
            (prefabTargetButton.GetComponent<RectTransform>().sizeDelta.y + 5) * targets.Count
        );
    }

    /// <summary>
    /// Callback for selecting a target
    /// </summary>
    /// <param name="target">The target</param>
    public void SelectTarget(List<BattleManager.CharacterData> target)
    {
        manager.UseItemOn(currentItem, target, !usingSkill);
    }

    /// <summary>
    /// Callback for closing the target screen
    /// </summary>
    public void CloseTargetScreen()
    {
        targetScreen.SetActive(false);
        if (usingSkill) skillsScreen.SetActive(true);
        else itemsScreen.SetActive(true);
    }

    /// <summary>
    /// Sets the currently displayed skills
    /// </summary>
    /// <param name="skills">The skills</param>
    public void SetCurrentSkills(List<RPGItem> skills)
    {
        foreach (Transform child in skillButtonsRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (RPGItem skill in skills)
        {
            Instantiate(prefabSkillButton, skillButtonsRoot).Init(skill, this);
        }

        skillButtonsRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(
            skillButtonsRoot.GetComponent<RectTransform>().sizeDelta.x,
            (prefabSkillButton.GetComponent<RectTransform>().sizeDelta.y + 5) * skills.Count
        );
    }

    /// <summary>
    /// Sets the currently displayed items
    /// </summary>
    /// <param name="items">The items</param>
    public void SetCurrentItems(List<RPGItem> items)
    {
        foreach (Transform child in itemsButtonsRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (RPGItem item in items)
        {
            Instantiate(prefabItemButton, itemsButtonsRoot).Init(item, this);
        }

        itemsButtonsRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(
            itemsButtonsRoot.GetComponent<RectTransform>().sizeDelta.x,
            (prefabItemButton.GetComponent<RectTransform>().sizeDelta.y + 5) * items.Count
        );
    }


}
