using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles dungeons
/// </summary>
public class DungeonManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 cameraOffset;
    [SerializeField] private float playerSpeed;

    [Header("Stairs")]
    [SerializeField] private DungeonStairs stairs;

    [Header("Encounters")]
    [SerializeField] private float averageMeterstoEncounter = 15f;
    public float currentMetersRemainingToEncounter;

    [Header("Components")]
    [SerializeField] private Behaviour[] disableOnBattle;
    [SerializeField] private Generator2D generator;
    [SerializeField] private DungeonGUI gui;
    private int currentFloor;

    public static DungeonManager instance;
    private bool inBattle = false;
    private DungeonData data;
    private Coroutine routineNextFloor;
    private Vector3 moveVector;
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
        stairs.active = false;
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
            stairs.active = true;
            ComputeMetersToNextEncounter();
            yield return new WaitForEndOfFrame();
            gui.FadeTo(0f);
        }
        routineNextFloor = null;
    }

    /// <summary>
    /// Computes the meters to the next encounter
    /// </summary>
    private void ComputeMetersToNextEncounter()
    {
        currentMetersRemainingToEncounter = Random.Range(averageMeterstoEncounter - 5f, averageMeterstoEncounter + 5f);
    }

    /// <summary>
    /// Starts a random encounter
    /// </summary>
    public void StartRandomEncounter()
    {
        inBattle = true;
        ComputeMetersToNextEncounter();
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

        cameraTransform.position = playerRigidbody.position + cameraOffset;
        moveVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        bool moving = moveVector != Vector3.zero;
        if (moving)
        {
            playerModel.forward = moveVector;
            currentMetersRemainingToEncounter -= (Mathf.Abs(moveVector.x) + Mathf.Abs(moveVector.z)) * Time.deltaTime;

            if (currentMetersRemainingToEncounter <= 0) StartRandomEncounter();
        }
        playerAnimator.SetBool("Move", moving);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gui.TogglePauseMenu();
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartRandomEncounter();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            NextFloor();
        }
#endif
    }

    void FixedUpdate()
    {
        if (inBattle || changingFloor)
        {
            playerRigidbody.velocity = Vector3.zero;
            return;
        }

        playerRigidbody.velocity = moveVector * playerSpeed;

    }
}
