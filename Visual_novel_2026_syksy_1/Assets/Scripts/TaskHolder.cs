using System;
using UnityEngine;

[Serializable]
public class TaskHolderSaveData
{
    public string activeCharacterName;
    public CharacterTaskSaveData[] characters;
}

[Serializable]
public class CharacterTaskSaveData
{
    public string characterName;
    public bool[] completedTasks;
}

/*
=== README ===

TaskHolder sailottaa hahmojen tehtavien tilat ja avaa aktiivisen hahmon oman
Misc-huoneen. Misc-huone voi kysya TaskHolderilta tehtavien tilan ja nayttaa
sen Details-ikkunassa.

Inspector:
- Characters: lisaa Franky, Mirai, Wyod ja muut hahmot.
- Character Name: hahmon nimi.
- Misc Room Index: taman hahmon oman Misc-huoneen indeksi.
- Tasks: hahmon tehtavat jarjestyksessa 1-4.
- Task Name: tehtavan nimi.
- Is Completed: tehtavan alkutila.

Huoneindeksin tarkistus:
1. Valitse Hierarchysta BootstrapSceneManager.
2. Katso Inspectorista Room Scenes -lista.
3. Element 0 tarkoittaa listan ensimmaista huonetta, Element 1 toista jne.
4. Aseta hahmon Misc Room Index samaksi numeroksi kuin huoneen Element-numero.

Esimerkki:
- BootstrapSceneManager -> Room Scenes -> Element 0 = Franky Misc
- Franky -> Misc Room Index = 0

Misc Room Index ei ole enum-arvo eika Project-ikkunan jarjestys. Se viittaa
suoraan BootstrapSceneManagerin Room Scenes -listan elementtiin.

Metodit:
- SelectCharacter("Franky"): valitsee hahmon Misc-huonetta varten.
- OpenActiveCharacterMiscRoom(): avaa aktiivisen hahmon oman Misc-huoneen.
- OpenMiscRoomForCharacter("Franky"): valitsee hahmon ja avaa sen Misc-huoneen.
- IsActiveCharacterTaskCompleted(0): kertoo tehtavan tilan.
- AreAllActiveCharacterTasksCompleted(): kertoo, ovatko kaikki tehtavat valmiit.
- HasActiveCharacterTasksUnderway(): kertoo, onko jokin tehtava kesken.
- CompleteTask(0): merkitsee aktiivisen hahmon tehtavan valmiiksi.

Tallennus:
- CreateSaveData() tekee JsonUtilitylle sopivan datan.
- LoadSaveData(data) palauttaa tallennetut tilat.
- Gamedata tallentaa TaskHolderSaveData-tiedon PlayerDataan.

=== END README ===
*/
public class TaskHolder : MonoBehaviour
{
    [Serializable]
    private class TaskRoute
    {
        [SerializeField] private string taskName;
        [SerializeField] private bool isCompleted;

        public bool IsCompleted => isCompleted;

        public void SetCompleted(bool completed)
        {
            isCompleted = completed;
        }
    }

    [Serializable]
    private class CharacterRoute
    {
        [SerializeField] private string characterName;
        [SerializeField] private int miscRoomIndex = -1;
        [SerializeField] private TaskRoute[] tasks;

        public string CharacterName => characterName;
        public int MiscRoomIndex => miscRoomIndex;
        public TaskRoute[] Tasks => tasks;

        public CharacterTaskSaveData CreateSaveData()
        {
            bool[] completedTasks = new bool[tasks == null ? 0 : tasks.Length];
            for (int taskIndex = 0; taskIndex < completedTasks.Length; taskIndex++)
                completedTasks[taskIndex] = tasks[taskIndex] != null && tasks[taskIndex].IsCompleted;

            return new CharacterTaskSaveData
            {
                characterName = characterName,
                completedTasks = completedTasks
            };
        }
    }

    [SerializeField] private CharacterRoute[] characters;
    [SerializeField] private int activeCharacterIndex;

    public string ActiveCharacterName
    {
        get
        {
            return TryGetActiveCharacter(out CharacterRoute character)
                ? character.CharacterName
                : string.Empty;
        }
    }

    public bool SelectCharacter(string characterName)
    {
        if (characters == null)
            return false;

        for (int characterIndex = 0; characterIndex < characters.Length; characterIndex++)
        {
            if (characters[characterIndex] != null &&
                string.Equals(characters[characterIndex].CharacterName, characterName, StringComparison.OrdinalIgnoreCase))
            {
                activeCharacterIndex = characterIndex;
                return true;
            }
        }

        Debug.LogError($"Character '{characterName}' is not configured.");
        return false;
    }

    public void CompleteTask(int taskIndex)
    {
        SetTaskCompleted(taskIndex, true);
    }

    public void OpenActiveCharacterMiscRoom()
    {
        if (!TryGetActiveCharacter(out CharacterRoute character))
        {
            Debug.LogError("No active character is configured.");
            return;
        }

        if (character.MiscRoomIndex < 0)
        {
            Debug.LogError($"Character '{character.CharacterName}' does not have a valid Misc room index.");
            return;
        }

        if (BootstrapSceneManager.Instance == null)
        {
            Debug.LogError("BootstrapSceneManager is not available.");
            return;
        }

        BootstrapSceneManager.Instance.LoadRoom(character.MiscRoomIndex);
    }

    public void OpenMiscRoomForCharacter(string characterName)
    {
        if (!SelectCharacter(characterName))
            return;

        OpenActiveCharacterMiscRoom();
    }

    public void SetTaskCompleted(int taskIndex, bool completed)
    {
        if (!TryGetTask(taskIndex, out TaskRoute task))
            return;

        task.SetCompleted(completed);
    }

    public bool IsActiveCharacterTaskCompleted(int taskIndex)
    {
        return TryGetTask(taskIndex, out TaskRoute task) && task.IsCompleted;
    }

    public bool AreAllActiveCharacterTasksCompleted()
    {
        if (!TryGetActiveCharacter(out CharacterRoute character) || character.Tasks == null || character.Tasks.Length == 0)
            return false;

        foreach (TaskRoute task in character.Tasks)
        {
            if (task == null || !task.IsCompleted)
                return false;
        }

        return true;
    }

    public bool HasActiveCharacterTasksUnderway()
    {
        return TryGetActiveCharacter(out CharacterRoute character) &&
            character.Tasks != null &&
            Array.Exists(character.Tasks, task => task != null && !task.IsCompleted);
    }

    public TaskHolderSaveData CreateSaveData()
    {
        CharacterTaskSaveData[] saveCharacters = new CharacterTaskSaveData[characters == null ? 0 : characters.Length];
        for (int characterIndex = 0; characterIndex < saveCharacters.Length; characterIndex++)
            saveCharacters[characterIndex] = characters[characterIndex]?.CreateSaveData();

        return new TaskHolderSaveData
        {
            activeCharacterName = ActiveCharacterName,
            characters = saveCharacters
        };
    }

    public void LoadSaveData(TaskHolderSaveData saveData)
    {
        if (saveData?.characters == null || characters == null)
            return;

        if (!string.IsNullOrEmpty(saveData.activeCharacterName))
            SelectCharacter(saveData.activeCharacterName);

        foreach (CharacterTaskSaveData savedCharacter in saveData.characters)
        {
            if (savedCharacter == null || !TryGetCharacter(savedCharacter.characterName, out CharacterRoute character))
                continue;

            if (savedCharacter.completedTasks == null || character.Tasks == null)
                continue;

            for (int taskIndex = 0; taskIndex < character.Tasks.Length && taskIndex < savedCharacter.completedTasks.Length; taskIndex++)
            {
                if (character.Tasks[taskIndex] != null)
                    character.Tasks[taskIndex].SetCompleted(savedCharacter.completedTasks[taskIndex]);
            }
        }
    }

    private bool TryGetActiveCharacter(out CharacterRoute character)
    {
        character = null;
        if (characters == null || activeCharacterIndex < 0 || activeCharacterIndex >= characters.Length)
            return false;

        character = characters[activeCharacterIndex];
        return character != null;
    }

    private bool TryGetCharacter(string characterName, out CharacterRoute character)
    {
        character = null;
        if (characters == null)
            return false;

        foreach (CharacterRoute configuredCharacter in characters)
        {
            if (configuredCharacter != null &&
                string.Equals(configuredCharacter.CharacterName, characterName, StringComparison.OrdinalIgnoreCase))
            {
                character = configuredCharacter;
                return true;
            }
        }

        return false;
    }

    private bool TryGetTask(int taskIndex, out TaskRoute task)
    {
        task = null;
        if (!TryGetActiveCharacter(out CharacterRoute character) ||
            character.Tasks == null ||
            taskIndex < 0 ||
            taskIndex >= character.Tasks.Length)
        {
            Debug.LogError($"Task index {taskIndex} is outside the active character's task list.");
            return false;
        }

        task = character.Tasks[taskIndex];
        if (task == null)
        {
            Debug.LogError($"Task {taskIndex} is not configured for '{character.CharacterName}'.");
            return false;
        }

        return true;
    }
}