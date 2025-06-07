using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles dungeons
/// </summary>
public class DungeonManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Behaviour[] disableOnBattle;

    public static DungeonManager instance;
    private bool inBattle = false;
    private DungeonData data;





    [Header("Debug")]
    [SerializeField] private bool useDebug = true;
    [SerializeField] private DungeonData debugData;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (useDebug) LoadDungeon(debugData);
    }

    /// <summary>
    /// Loads a new dungeon
    /// </summary>
    /// <param name="data">The dungeon's data</param>
    void LoadDungeon(DungeonData data)
    {
        this.data = data;
    }

    /// <summary>
    /// Starts a random encounter
    /// </summary>
    public void StartRandomEncounter()
    {
        inBattle = true;
        GameManager.GetRPGManager().SetNextBattleEncounter(
            data.encounters[Random.Range(0, data.encounters.Length)],
            data.battleBackground,
            BattleData.CloseType.UNLOAD,
            null
        );
        SceneManager.LoadScene("Battle", LoadSceneMode.Additive);
        EnableBattleRequirements(false);
    }

    /// <summary>
    /// Ends the current battle
    /// </summary>
    public void EndBattle()
    {

        SceneManager.UnloadSceneAsync("Battle").completed += _ =>
        {
            inBattle = false;
            EnableBattleRequirements(true);
        };
    }

    /// <summary>
    /// Enables the battle requirements
    /// </summary>
    /// <param name="active">True if they must be active</param>
    public void EnableBattleRequirements(bool active)
    {
        foreach (Behaviour requirement in disableOnBattle)
        {
            requirement.enabled = active;
        }
    }

    void Update()
    {
        if (inBattle) return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            StartRandomEncounter();
        }
    }
}
