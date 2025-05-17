using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the game's battles
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("DEBUG")]
    [SerializeField] private BattleData data;

    private List<CharacterData> players;
    private List<CharacterData> ennemies;

    void Start()
    {
        players = new List<CharacterData>();
        ennemies = new List<CharacterData>();
        LoadBattle(data);
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

        GenerateCharacters();
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
                characterVisual = visual
            });
        }

        from = new Vector3(0, 0, 10);
        to = new Vector3(40, 0, 10);
        i = 0;
        rotation = Quaternion.Euler(0, 180, 0);

        foreach (RPGCharacterDataInterface dataInterface in data.ennemies)
        {
            character = new RPGCharacter(dataInterface.data.Clone());
            visual = Instantiate(Resources.Load<BattleCharacter>("RPG/Battles/Characters/" + character.GetData().ID));
            if (!string.IsNullOrEmpty(character.GetData().weapon)) visual.SetWeapon(character.GetData().weapon);
            visual.transform.position = Vector3.Lerp(from, to, (i + 0.5f) / data.ennemies.Length);
            visual.transform.rotation = rotation;

            i++;

            ennemies.Add(new CharacterData
            {
                characterData = character,
                characterVisual = visual
            });
        }
    }

    public struct CharacterData
    {
        public RPGCharacter characterData;
        public BattleCharacter characterVisual;
    }

}
