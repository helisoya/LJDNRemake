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
    [SerializeField] private Generator2D generator;
    [SerializeField] private DungeonGUI gui;
    private int currentFloor;

    public static DungeonManager instance;
    private bool inBattle = false;
    private DungeonData data;
    private Coroutine routineNextFloor;
    public bool changingFloor { get { return routineNextFloor != null; } }




    [Header("Debug")]
    [SerializeField] private bool useDebug = true;
    [SerializeField] private DungeonData debugData;

    void Awake()
    {
        instance = this;
    }

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
        currentFloor = -1;
        NextFloor();
    }

    /// <summary>
    /// Loads the next floor
    /// </summary>
    public void NextFloor()
    {
        if (routineNextFloor != null)
        {
            StopCoroutine(routineNextFloor);
        }
        routineNextFloor = StartCoroutine(Routine_TransitionToNextFloor());
    }

    /// <summary>
    /// Routine for loading the next floor
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator Routine_TransitionToNextFloor()
    {
        gui.FadeTo(1f);
        yield return new WaitForEndOfFrame();
        while (gui.fading) yield return new WaitForEndOfFrame();

        currentFloor++;
        if (currentFloor == data.floorsAmount)
        {
            GameManager.instance.SetNextChapter(data.endChapter);
            SceneManager.LoadScene("VN");
        }
        else
        {
            generator.Generate(data);
            yield return new WaitForEndOfFrame();
            gui.FadeTo(0f);
        }
        routineNextFloor = null;
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
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Dungeon"));
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
        if (inBattle || changingFloor) return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            StartRandomEncounter();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            NextFloor();
        }
    }
}
