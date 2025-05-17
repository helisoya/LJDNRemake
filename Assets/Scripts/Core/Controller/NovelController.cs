using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovelController : MonoBehaviour
{

    public static NovelController instance;

    public bool isReadyForSaving
    {
        get
        {
            return waitingForUserToEndDialog || currentChoice != null || inInteractionMode;
        }
    }

    private List<string> data = new List<string>();
    private string activeChapterFile = "";
    private bool next = false;
    private Coroutine handlingChapterFile = null;
    private int chapterProgress = 0;
    private Choice currentChoice;
    private bool waitingForUserToEndDialog;
    private bool inInteractionMode;
    private bool loadedAutoSave;

    private List<StackEntry> stack;

    /// <summary>
    /// Represents an entry in the chapters stack
    /// </summary>
    [System.Serializable]
    public class StackEntry
    {
        public string chapterName = "";
        public int currentChapterProgress = 0;
    }



    public void Next()
    {
        next = true;
    }

    public void EnableLock()
    {
        handlingChapterFile = null;
    }

    public void ClearStack()
    {
        while (stack.Count > 1) stack.RemoveAt(0);
    }

    void Start()
    {
        instance = this;
        stack = new List<StackEntry>();
        print("Is Loading Save : " + GameManager.instance.IsLoadingSave());
        if (GameManager.instance.IsLoadingSave())
        {
            LoadGameFile(GameManager.instance.GetSaveToLoad());
        }
        else
        {
            LoadChapterFile(GameManager.instance.GetNextChapter());
        }
    }

    public void LoadGameFile(string saveName = "save")
    {
        GameManager.instance.SetSaveToLoad(saveName);
        GAMEFILE activeGameFile = GameManager.GetSaveManager().Load(saveName);

        VNGUI.instance.ForceBgTo(activeGameFile.fadeBg);
        VNGUI.instance.ForceFgTo(activeGameFile.fadeFg);
        VNGUI.instance.SetBgColor(activeGameFile.colorBg);
        VNGUI.instance.SetFgColor(activeGameFile.colorFg);
        InteractionManager.instance.SetActive(false);

        CameraController.instance.SetPosition(activeGameFile.cameraPosition, true);
        CameraController.instance.SetRotation(activeGameFile.cameraRotation, true);

        LightingManager.instance.ChangeData(activeGameFile.skyData);

        stack = activeGameFile.stack;
        StackEntry currentChapter = stack[stack.Count - 1];

        data = FileManager.ReadTextAsset(Resources.Load<TextAsset>($"Story/{currentChapter.chapterName}"));
        activeChapterFile = currentChapter.chapterName;

        CharacterManager.instance.RemoveAllCharacters();
        List<GAMEFILE.CHARACTERDATA> characters = activeGameFile.characterInScene;
        foreach (GAMEFILE.CHARACTERDATA character in characters)
        {
            CharacterManager.instance.AddCharacterFromData(character);
        }

        if (activeGameFile.background != null)
        {
            BackgroundManager.instance.ReplaceBackground(activeGameFile.background, false);
        }

        if (activeGameFile.music != null)
            AudioManager.instance.PlaySong(activeGameFile.music);

        currentChoice = activeGameFile.currentChoice;
        if (currentChoice.answers.Count == 0) currentChoice = null;

        inInteractionMode = activeGameFile.interactionMode;
        InteractionManager.instance.Load(activeGameFile.interactables);

        if (inInteractionMode)
        {
            DialogSystem.instance.Close();
        }
        else
        {
            DialogSystem.instance.Open(activeGameFile.currentTextSystemSpeakerDisplayText,
                activeGameFile.currentTextsIds);
        }

        GameManager.GetRPGManager().LoadCharactersFromList(activeGameFile.rpgCharacters);
        GameManager.GetRPGManager().SetFollowers(activeGameFile.followers);
        GameManager.GetRPGManager().SetInventory(activeGameFile.inventory);

        loadedAutoSave = saveName.Equals("auto");

        LoadChapterFile(currentChapter.chapterName, currentChapter.currentChapterProgress);
    }

    public void SaveGameFile(string saveName)
    {
        GAMEFILE activeGameFile = GameManager.GetSaveManager().saveFile;

        activeGameFile.stack = stack;

        activeGameFile.currentTextsIds = DialogSystem.instance.currentTextsIds;
        activeGameFile.currentTextSystemSpeakerDisplayText = DialogSystem.instance.speakerNameText.text;

        activeGameFile.characterInScene = CharacterManager.instance.SaveCharacters();

        activeGameFile.background = BackgroundManager.instance.currentBackground ?
            BackgroundManager.instance.currentBackground.backgroundName : null;

        activeGameFile.music = AudioManager.activeSong != null ? AudioManager.activeSong.clipName : "";

        activeGameFile.currentChoice = currentChoice;

        activeGameFile.fadeBg = VNGUI.instance.fadeBgAlpha;
        activeGameFile.fadeFg = VNGUI.instance.fadeFgAlpha;

        activeGameFile.colorBg = VNGUI.instance.fadeBgColor;
        activeGameFile.colorFg = VNGUI.instance.fadeFgColor;

        activeGameFile.interactionMode = inInteractionMode;
        activeGameFile.interactables = InteractionManager.instance.GetSaveData();

        activeGameFile.cameraPosition = CameraController.instance.targetPosition;
        activeGameFile.cameraRotation = CameraController.instance.targetRotation;

        activeGameFile.skyData = LightingManager.instance.GetCurrentDataName();

        activeGameFile.rpgCharacters = GameManager.GetRPGManager().GetCharacters();
        activeGameFile.inventory = GameManager.GetRPGManager().GetInventory();
        activeGameFile.followers = GameManager.GetRPGManager().GetFollowers();

        GameManager.GetSaveManager().Save(saveName);
    }

    public void LoadChapterFile(string filename, int chapterProgress = 0)
    {
        StopAllCoroutines();
        handlingChapterFile = null;

        if (!GameManager.instance.IsLoadingSave())
        {
            inInteractionMode = false;
            stack.Add(new StackEntry { chapterName = filename, currentChapterProgress = chapterProgress });
            while (stack.Count > 10)
            {
                stack.RemoveAt(0);
            }
        }
        print("Current Stack :");
        foreach (StackEntry entry in stack)
        {
            print("- " + entry.chapterName + " : " + entry.currentChapterProgress);
        }
        activeChapterFile = filename;
        this.chapterProgress = chapterProgress;

        print("Loading chapter : " + $"Story/{filename}");
        data = FileManager.ReadTextAsset(Resources.Load<TextAsset>($"Story/{filename}"));


        handlingChapterFile = StartCoroutine(HandlingChapterFile());
    }




    IEnumerator HandlingChapterFile()
    {
        while (handlingChapterFile == null)
        {
            yield return new WaitForEndOfFrame();
            // Wait for the game to actually register the routine
        }

        if (GameManager.instance.IsLoadingSave())
        {
            GameManager.instance.SetSaveToLoad(null);

            if (currentChoice != null)
            {
                yield return HandleChoice(currentChoice);
            }
            else if (inInteractionMode)
            {
                yield return HandleInteraction();
            }
            else if (!loadedAutoSave)
            {
                waitingForUserToEndDialog = true;
                next = false;
                while (!next)
                {
                    yield return new WaitForEndOfFrame();
                }
                next = false;
                waitingForUserToEndDialog = false;
            }
            chapterProgress++;
            stack[stack.Count - 1].currentChapterProgress++;
        }

        ChoiceScreen.instance.Hide();

        while (chapterProgress < data.Count)
        {

            string line = data[chapterProgress];

            if (line.Equals("interact"))
            {
                yield return HandleInteraction();
            }
            else if (line.StartsWith("choice"))
            {
                yield return HandlingChoiceLine(line);
            }
            else if (line.StartsWith("if"))
            {
                yield return HandlingIf(line);
            }
            else
            {
                yield return HandlingLine(line);
            }

            if (handlingChapterFile == null) yield break;

            chapterProgress++;
            stack[stack.Count - 1].currentChapterProgress++;
        }

        if (handlingChapterFile == null) yield break;

        print("Removign from stack");
        stack.RemoveAt(stack.Count - 1);
        if (stack.Count > 0)
        {
            print("Reload previous entry");
            StackEntry entry = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            LoadChapterFile(entry.chapterName, entry.currentChapterProgress);
        }

        handlingChapterFile = null;
    }

    IEnumerator HandleInteraction()
    {
        print("Starting interaction mode");
        ClearStack();
        inInteractionMode = true;
        stack[stack.Count - 1].currentChapterProgress++;
        InteractionManager.instance.SetActive(true);
        while (InteractionManager.instance.active)
        {
            yield return new WaitForEndOfFrame();
        }
        if (!GameManager.instance.IsLoadingSave()) inInteractionMode = false;
    }


    IEnumerator HandlingIf(string line)
    {
        yield return new WaitForEndOfFrame();

        // if(KEY = VALUE & KEY = VALUE) NEW_CHAPTER
        string[] split = line.Split(new char[] { '(', ')' });

        bool ok = true;

        string[] splitAnds = split[1].Split(" & ");

        foreach (string splitAnd in splitAnds)
        {
            string[] parametersSplit = splitAnd.Split(' ');
            string key = parametersSplit[0];
            string oper = parametersSplit[1];
            string value = parametersSplit[2];

            if (!IsCheckOkay(key, value, oper))
            {
                ok = false;
                break;
            }
        }

        if (ok)
        {
            split[2] = split[2].Replace(" ", "");
            if (split.Length > 3)
            {
                yield return HandlingLine(split[2] + "(" + split[3] + ")");
            }
            else
            {
                handlingChapterFile = null;
                stack[stack.Count - 1].currentChapterProgress++;
                LoadChapterFile(split[2]);
            }
        }
    }

    bool IsCheckOkay(string key, string value, string oper)
    {
        string currentValue = GameManager.GetSaveManager().GetItem(key);

        switch (oper)
        {
            case "=":
                return currentValue.Equals(value);
            case ">":
                return int.Parse(currentValue) > int.Parse(value);
            case "<":
                return int.Parse(currentValue) < int.Parse(value);
            case ">=":
                return int.Parse(currentValue) >= int.Parse(value);
            case "<=":
                return int.Parse(currentValue) <= int.Parse(value);
            case "!=":
                return int.Parse(currentValue) != int.Parse(value);
        }

        return false;
    }


    IEnumerator HandlingChoiceLine(string line)
    {
        // Choice ID_QUESTION
        // ID_REP NEXTCHAPTER
        currentChoice = new Choice(line.Split(' ')[1]);

        int i = chapterProgress + 1;
        while (i < data.Count && !string.IsNullOrEmpty(data[i]) && !string.IsNullOrWhiteSpace(data[i]) && data[i].StartsWith("\t"))
        {
            string choiceLine = data[i].Replace("\t", "");
            string[] split = choiceLine.Split(' ');
            print(choiceLine + " -> " + split[0] + " " + split[1] + " " + split.Length);
            currentChoice.answers.Add(new Choice.ChoiceAnswer(split[0], split[1]));
            i++;
        }

        stack[stack.Count - 1].currentChapterProgress = i;

        if (currentChoice.answers.Count > 0)
        {
            yield return HandleChoice(currentChoice);
        }
    }

    IEnumerator HandleChoice(Choice choice)
    {
        ChoiceScreen.instance.Show(choice);

        yield return new WaitForEndOfFrame();

        while (ChoiceScreen.instance.isWaitingForChoiceToBeMade)
        {
            yield return new WaitForEndOfFrame();
        }

        string action = choice.answers[ChoiceScreen.instance.chosenIndex].action;
        currentChoice = null;

        if (action.StartsWith("Map"))
        {
            string[] parameters = action.Split(new char[] { '(', ')' })[1].Split(';');
            Map.instance.OpenMap(parameters[0], parameters[1]);
        }
        else
        {
            handlingChapterFile = null;
            LoadChapterFile(action);
        }
    }

    public IEnumerator HandlingLine(string line, bool isQuickCommand = false)
    {
        if (string.IsNullOrEmpty(line) || line.StartsWith('#')) yield break;

        string[] data = line.Split(new char[] { '(', ')' });
        if (data.Length < 2) yield break;

        string[] parameters = data[1].Split(";");

        print(line);

        switch (data[0])
        {
            case "dialog":
                if (isQuickCommand) break;
                next = false;

                // Speaker - CharacterModel - additive - dialog
                DialogSystem.instance.OpenAllRequirementsForDialogueSystemVisibility(true);
                DialogSystem.instance.Say(parameters[3], parameters[0], parameters[1].Equals("_") ? null : parameters[1], bool.Parse(parameters[2]));

                TextArchitect architect = DialogSystem.instance.textArchitect;

                while (architect.isConstructing)
                {
                    if (next)
                    {
                        next = false;
                        architect.skip = true;
                    }
                    yield return new WaitForEndOfFrame();
                }

                yield return new WaitForEndOfFrame();

                waitingForUserToEndDialog = true;
                while (!next)
                {
                    yield return new WaitForEndOfFrame();
                }
                waitingForUserToEndDialog = false;

                break;

            case "setBackground":
                BackgroundManager.instance.ReplaceBackground(parameters[0], parameters.Length > 1 ? bool.Parse(parameters[1]) : true);
                break;

            case "playSound":
                AudioClip clip = Resources.Load("Audio/SFX/" + parameters[0]) as AudioClip;
                if (clip != null)
                {
                    AudioManager.instance.PlaySFX(clip);
                }
                break;

            case "playMusic":
                AudioManager.instance.PlaySong(parameters[0]);
                break;

            case "clearStack":
                ClearStack();
                break;

            case "removeAllCharacters":
                CharacterManager.instance.RemoveAllCharacters();
                break;

            case "addCharacter":
                CharacterManager.instance.AddCharacter(parameters[0], bool.Parse(parameters[1]));
                break;

            case "removeCharacter":
                CharacterManager.instance.RemoveCharacter(parameters[0]);
                break;

            case "setCharacterPosition":
                CharacterManager.instance.SetCharacterPosition(
                    parameters[0],
                    BackgroundManager.instance.GetMarkerPosition(parameters[1])
                );
                break;

            case "setCharacterRotation":
                float angle;
                if (!float.TryParse(parameters[1], out angle))
                {
                    angle = BackgroundManager.instance.GetMarkerRotation(parameters[1]);
                }
                CharacterManager.instance.SetCharacterRotation(
                    parameters[0],
                    angle
                );
                break;

            case "setCharacterMouth":
                CharacterManager.instance.SetCharacterMouthAnimation(
                    parameters[0],
                    parameters[1]
                );
                break;

            case "setCharacterEye":
                CharacterManager.instance.SetCharacterEyeAnimation(
                    parameters[0],
                    parameters[1]
                );
                break;

            case "setCharacterBody":
                CharacterManager.instance.SetCharacterBodyAnimation(
                    parameters[0],
                    parameters[1],
                    bool.Parse(parameters[2])
                );
                break;

            case "setInteractionChapter":
                InteractionManager.instance.ChangeObjectChapter(parameters[0], parameters[1]);
                break;

            case "setInteractionHidden":
                InteractionManager.instance.SetObjectHidden(parameters[0], bool.Parse(parameters[1]));
                break;

            case "setCharacterAlpha":
                // Character - Target - Wait for end ?

                CharacterManager.instance.TransitionCharacterAlpha(parameters[0],
                    float.Parse(parameters[1], System.Globalization.CultureInfo.InvariantCulture));

                if (bool.Parse(parameters[2]))
                {
                    yield return new WaitForEndOfFrame();
                    while (CharacterManager.instance.IsCharacterFading(parameters[0]))
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }

                break;

            case "autosave":
                SaveGameFile("auto");
                break;

            case "flash":

                VNGUI.instance.FlashTo(1, 10);

                yield return new WaitForSeconds(float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture));

                VNGUI.instance.FlashTo(0, 10);
                break;

            case "setBgColor":

                VNGUI.instance.SetBgColor(new Color(
                    float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(parameters[1], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture)
                ));
                break;

            case "setFgColor":

                VNGUI.instance.SetFgColor(new Color(
                    float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(parameters[1], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture)
                ));
                break;

            case "fadeBg":
                float speedBg = parameters.Length > 2 ? float.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture) : 2;
                VNGUI.instance.FadeBgTo(float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture), speedBg);
                if (bool.Parse(parameters[1]))
                {
                    yield return new WaitForEndOfFrame();
                    while (VNGUI.instance.fadingBg)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                break;

            case "fadeFg":
                float speedFg = parameters.Length > 2 ? float.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture) : 2;
                VNGUI.instance.FadeFgTo(float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture), speedFg);
                if (bool.Parse(parameters[1]))
                {
                    yield return new WaitForEndOfFrame();
                    while (VNGUI.instance.fadingFg)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                break;

            case "shake":
                CameraController.instance.SetShaking(
                    int.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture)
                );

                if (bool.Parse(parameters[1]))
                {
                    yield return new WaitForEndOfFrame();
                    while (CameraController.instance.isShaking)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                break;

            case "cameraPosition":
                // x ; y ; z ; immediate ; waitForEnd
                // default ; immediate ; waitForEnd

                int idx = 1;
                Vector3 position = new Vector3(0, 0, -10);
                if (!parameters[0].Equals("default"))
                {
                    position = new Vector3(
                        float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(parameters[1], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture)
                    );
                    idx = 3;
                }

                CameraController.instance.SetPosition(
                    position,
                    bool.Parse(parameters[idx])
                );

                if (bool.Parse(parameters[idx + 1]))
                {
                    yield return new WaitForEndOfFrame();
                    while (!CameraController.instance.atTargetPosition)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                break;

            case "cameraRotation":
                // x ; y ; z ; immediate ; waitForEnd
                // default ; immediate ; waitForEnd

                int idxR = 1;
                Vector3 rotaiton = new Vector3(0, 0, 0);
                if (!parameters[0].Equals("default"))
                {
                    rotaiton = new Vector3(
                        float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(parameters[1], System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(parameters[2], System.Globalization.CultureInfo.InvariantCulture)
                    );
                    idxR = 3;
                }

                CameraController.instance.SetRotation(
                    rotaiton,
                    bool.Parse(parameters[idxR])
                );

                if (bool.Parse(parameters[idxR + 1]))
                {
                    yield return new WaitForEndOfFrame();
                    while (!CameraController.instance.atTargetRotation)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                break;

            case "load":
                if (isQuickCommand) break;
                stack[stack.Count - 1].currentChapterProgress++;
                LoadChapterFile(parameters[0]);
                break;

            case "mainMenu":
                if (isQuickCommand) break;
                VNGUI.instance.FadeFgTo(1);
                yield return new WaitForEndOfFrame();
                while (VNGUI.instance.fadingFg)
                {
                    yield return new WaitForEndOfFrame();
                }

                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                break;

            case "loadScene":
                if (isQuickCommand) break;
                VNGUI.instance.FadeFgTo(1);
                yield return new WaitForEndOfFrame();
                while (VNGUI.instance.fadingFg)
                {
                    yield return new WaitForEndOfFrame();
                }

                UnityEngine.SceneManagement.SceneManager.LoadScene(parameters[0]);
                break;

            case "voice":
                clip = Resources.Load("Audio/Voice/" + parameters[0]) as AudioClip;
                if (clip != null)
                {
                    AudioManager.instance.PlayVoice(clip);
                }
                break;

            case "map":
                if (isQuickCommand) break;
                Map.instance.OpenMap(parameters[0], parameters[1]);
                yield return new WaitForEndOfFrame();
                while (Map.instance.open)
                {
                    yield return new WaitForEndOfFrame();
                }
                break;

            case "variable":
                GameManager.GetSaveManager().EditVariable(parameters[0], parameters[1]);
                break;

            case "random":
                GameManager.GetSaveManager().EditVariable("random",
                Random.Range(int.Parse(parameters[0]), int.Parse(parameters[1])).ToString());
                break;

            case "changeSkybox":
                LightingManager.instance.ChangeData(parameters[0]);
                break;

            case "wait":
                yield return new WaitForSeconds(float.Parse(parameters[0], System.Globalization.CultureInfo.InvariantCulture));
                break;
        }
    }

}
