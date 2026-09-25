using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TaskHolderSaveData
{
    public string activeCharacterName;
    public string activeTaskCharacterName;
    public CharacterTaskSaveData[] characters;
    public string[] collectedMcGuffins;
}

[Serializable]
public class CharacterTaskSaveData
{
    public string characterName;
    public bool[] completedTasks;
    public bool activeTaskAccepted;
}

/*
=== TASKHOLDER README ===

Hätänen lisäys
RoomManager.CompleteActiveTask("Big Mick") ennen kuin uusi tehtävä kutsutaan. ja viimeiseen kenttään ei tartte lisätä uutta misc huonetta.





TaskHolder on Bootstrap-scenessa oleva pysyva pelitilan manageri. Se sailyttaa
aktiivisen hahmon, tehtavien etenemisen ja pelaajan keratyt mcguffinit.

BootstrapSceneManager pysyy ladattuna, kun Misc-huone avataan additive-tilassa.
Siksi Misc-huoneen dialogi voi kutsua TaskHolderia RoomManagerin kautta, vaikka
TaskHolder ei ole Misc-huoneen scenessa.

PELIKULKU

1. Pelaaja valitsee hahmon kutsulla SelectCharacter("Franky").
2. Kutsu OpenActiveCharacterMiscRoom().
3. TaskHolder avaa hahmon ensimmaisen keskeneraisen tehtavan Room Index -huoneen.
4. Hahmo antaa tehtavan. Dialogin viimeisessa UnityEventissa kutsu
   RoomManager.OpenActiveTaskMiscRoom().
5. TaskHolder avaa yhteisen McGuffin Room Index -huoneen, jossa on item-logiikka.
6. Pelaaja saa mcguffinin huoneen tapahtumasta.
7. Kutsu AddMcGuffin("Teippi") RoomManagerin kautta.
8. Pelaaja palaa hahmon luo ja dialogi kutsuu CompleteActiveTask("Teippi").
9. TaskHolder tarkistaa, etta mcguffin on oikea, kuluttaa sen ja merkitsee
     tehtavan suoritetuksi.
10. Esc- tai Backspace-nappain sulkee huoneen. Kun hahmon huone
     avataan uudelleen, seuraava keskenerainen tehtava avaa seuraavan huoneen.

INSPECTOR-ASETUKSET

Bootstrap-scenessa:
- TaskHolder.characters: lisaa jokainen hahmo omaksi Character Routeksi.
- Character Name: hahmon nimi, jota SelectCharacter kayttaa.
- McGuffin Room Index: yhteinen Misc-huone, jossa item-tarkistus tapahtuu.
- Tasks: tehtavat jarjestyksessa 0, 1, 2, 3.

Jokaiselle Task Routelle:
- Task Name: tehtavan nimi.
- Is Completed: tehtavan alkutila, normaalisti pois paalta.
- Room Index: hahmon tehtavahuone, jossa tehtava annetaan.
- Required McGuffin: esineen nimi, esimerkiksi Teippi.

HUONEINDEKSIT

Room Index viittaa suoraan BootstrapSceneManagerin Room Scenes -listan
elementtiin. Se ei ole enum-arvo eika Project-ikkunan jarjestysnumero.

Esimerkki:
- BootstrapSceneManager -> Room Scenes -> Element 1 = Franky tehtava 1
- Franky -> Tasks -> Element 0 -> Room Index = 1
- Franky -> Tasks -> Element 0 -> Required McGuffin = Teippi

MISC-HUONEEN DIALOGI-EVENTIT

Misc-huoneen RoomManager toimii UnityEvent-siltana Bootstrapin TaskHolderiin:
- RoomManager.AddMcGuffin("Teippi") lisaa mcguffinin pelaajalle.
- RoomManager.OpenActiveTaskMiscRoom() avaa yhteisen Misc-huoneen tehtavan
    antamisen jalkeen.
- RoomManager.CompleteActiveTask("Teippi") yrittää suorittaa aktiivisen
    hahmon seuraavan tehtavan.

CompleteActiveTask onnistuu vain, jos:
- aktiivinen hahmo ja keskenerainen tehtava loytyvat;
- tehtavan Required McGuffin vastaa annettua nimea;
- pelaajalla on kyseinen mcguffin.

Onnistumisen jalkeen mcguffin poistetaan ja tehtava merkitään valmiiksi.
Pelaaja jaa nykyiseen huoneeseen, jotta han voi palata huoneesta normaalisti.

KAYTETTAVAT METODIT

- SelectCharacter("Franky"): valitsee aktiivisen hahmon.
- OpenActiveCharacterMiscRoom(): avaa seuraavan keskeneraisen tehtavan huoneen.
- OpenActiveTaskMiscRoom(): aktivoi tehtavan ja avaa yhteisen Misc-huoneen.
- OpenMiscRoomForCharacter("Franky"): valitsee hahmon ja avaa sen huoneen.
- AddMcGuffin("Teippi"): lisaa mcguffinin, jos sita ei viela ole.
- HasMcGuffin("Teippi"): tarkistaa, loytyyko mcguffin pelaajalta.
- TryCompleteActiveTask("Teippi"): tarkistaa mcguffinin ja suorittaa tehtavan.
- IsActiveCharacterTaskCompleted(0): kertoo tietyn tehtavan tilan.
- AreAllActiveCharacterTasksCompleted(): kertoo, ovatko kaikki tehtavat valmiit.
- HasActiveCharacterTasksUnderway(): kertoo, onko tehtavia viela kesken.
- CompleteTask(0): merkitsee tehtavan valmiiksi ilman mcguffin-tarkistusta.

TALLENNUS

Gamedata tallentaa TaskHolderin TaskHolderSaveData-tiedon PlayerDataan.
Tallennukseen kuuluvat:
- aktiivinen hahmo;
- jokaisen hahmon tehtavien valmistumistila;
- pelaajan keratyt mcguffinit.

CreateSaveData() muodostaa JsonUtilitylle sopivan datan.
LoadSaveData(data) palauttaa tallennetut tehtavat ja mcguffinit.

=== END TASKHOLDER README ===
*/
public class TaskHolder : MonoBehaviour
{
    public static TaskHolder Instance { get; private set; }

    [Serializable]
    private class TaskRoute
    {
        [SerializeField] private string taskName;
        [SerializeField] private bool isCompleted;
        [SerializeField] private int roomIndex = -1;
        [SerializeField] private string requiredMcGuffin;

        public bool IsCompleted => isCompleted;
        public int RoomIndex => roomIndex;
        public string RequiredMcGuffin => requiredMcGuffin;

        public void SetCompleted(bool completed)
        {
            isCompleted = completed;
        }
    }

    [Serializable]
    private class CharacterRoute
    {
        [SerializeField] private string characterName;
        [SerializeField] private TaskRoute[] tasks;
        [SerializeField] private bool activeTaskAccepted;
        [SerializeField] private int miscRoomIndex = -1;

        public string CharacterName => characterName;
        public int MiscRoomIndex => miscRoomIndex;
        public TaskRoute[] Tasks => tasks;
        public bool ActiveTaskAccepted => activeTaskAccepted;

        public void SetActiveTaskAccepted(bool accepted)
        {
            activeTaskAccepted = accepted;
        }

        public CharacterTaskSaveData CreateSaveData()
        {
            bool[] completedTasks = new bool[tasks == null ? 0 : tasks.Length];
            for (int taskIndex = 0; taskIndex < completedTasks.Length; taskIndex++)
                completedTasks[taskIndex] = tasks[taskIndex] != null && tasks[taskIndex].IsCompleted;

            return new CharacterTaskSaveData
            {
                characterName = characterName,
                completedTasks = completedTasks,
                activeTaskAccepted = activeTaskAccepted
            };
        }
    }

    [SerializeField] private CharacterRoute[] characters;
    [SerializeField] private int activeCharacterIndex;
    [SerializeField] private string[] collectedMcGuffins = Array.Empty<string>();
    private int activeTaskCharacterIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

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

        if (activeTaskCharacterIndex >= 0 && activeTaskCharacterIndex != activeCharacterIndex)
        {
            OpenCharacterMiscRoom(character);
            return;
        }

        int taskIndex = GetNextTaskIndex(character);
        if (taskIndex < 0)
        {
            OpenCharacterMiscRoom(character);
            return;
        }

        if (BootstrapSceneManager.Instance == null)
        {
            Debug.LogError("BootstrapSceneManager is not available.");
            return;
        }

        if (character.ActiveTaskAccepted && !HasRequiredMcGuffin(character, taskIndex))
        {
            OpenCharacterMiscRoom(character);
            return;
        }

        int roomIndex = GetTaskRoomIndex(character, taskIndex);
        if (roomIndex < 0)
        {
            Debug.LogError($"Character '{character.CharacterName}' does not have a valid task room index.");
            return;
        }

        BootstrapSceneManager.Instance.LoadRoom(roomIndex);
    }

    public void OpenActiveTaskMiscRoom()
    {
        Debug.Log($"OpenActiveTaskMiscRoom called. activeCharacterIndex={activeCharacterIndex}, activeTaskCharacterIndex={activeTaskCharacterIndex}.");

        if (!TryGetActiveCharacter(out CharacterRoute character))
        {
            Debug.LogError("No active character is configured.");
            return;
        }

        if (TryGetAcceptedTaskOwner(out CharacterRoute acceptedOwner))
        {
            if (!string.Equals(acceptedOwner.CharacterName, character.CharacterName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"Task already accepted for '{acceptedOwner.CharacterName}'. Active task owner takes precedence.");
                return;
            }
        }

        int taskIndex = GetNextTaskIndex(character);
        if (taskIndex < 0)
        {
            Debug.Log($"All tasks for '{character.CharacterName}' are completed.");
            return;
        }

        if (character.ActiveTaskAccepted && activeTaskCharacterIndex == activeCharacterIndex)
        {
            Debug.LogWarning($"Task for '{character.CharacterName}' is already active. Complete it with CompleteActiveTask before starting another task.");
            return;
        }

        if (string.IsNullOrWhiteSpace(character.Tasks[taskIndex].RequiredMcGuffin))
            character.Tasks[taskIndex].SetCompleted(true);

        taskIndex = GetNextTaskIndex(character);
        if (taskIndex < 0)
            return;

        if (activeTaskCharacterIndex < 0 || activeTaskCharacterIndex != activeCharacterIndex || collectedMcGuffins.Length > 0)
        {
            ClearCollectedMcGuffins();
        }

        character.SetActiveTaskAccepted(true);
        activeTaskCharacterIndex = activeCharacterIndex;
        Debug.Log($"Task accepted for '{character.CharacterName}' at index {taskIndex}. Inventory cleared.");
    }

    public void AddMcGuffin(string mcGuffinName)
    {
        if (string.IsNullOrWhiteSpace(mcGuffinName))
            return;

        string trimmedName = mcGuffinName.Trim();
        Debug.Log($"AddMcGuffin: '{trimmedName}'. Current inventory: {string.Join(", ", collectedMcGuffins ?? Array.Empty<string>())}");

        if (HasMcGuffin(trimmedName))
            return;

        List<string> items = new List<string>(collectedMcGuffins ?? Array.Empty<string>());
        items.Add(trimmedName);
        collectedMcGuffins = items.ToArray();
        Debug.Log($"Inventory updated: {string.Join(", ", collectedMcGuffins)}");
    }

    public bool HasMcGuffin(string mcGuffinName)
    {
        return !string.IsNullOrWhiteSpace(mcGuffinName) &&
            Array.Exists(collectedMcGuffins ?? Array.Empty<string>(), item =>
                string.Equals(item, mcGuffinName.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public bool HasActiveTaskForActiveCharacter()
    {
        return TryGetActiveCharacter(out CharacterRoute character) &&
            activeTaskCharacterIndex == activeCharacterIndex &&
            character.ActiveTaskAccepted &&
            GetNextTaskIndex(character) >= 0;
    }

    public bool HasMcGuffinForActiveTask()
    {
        if (!HasActiveTaskForActiveCharacter() ||
            !TryGetActiveCharacter(out CharacterRoute character))
        {
            return false;
        }

        string requiredMcGuffin = GetRequiredMcGuffinForActiveTask(character);
        return !string.IsNullOrWhiteSpace(requiredMcGuffin) && HasMcGuffin(requiredMcGuffin);
    }

    public bool TryCompleteActiveTask(string mcGuffinName)
    {
        Debug.Log($"TryCompleteActiveTask called with '{mcGuffinName}'. activeTaskCharacterIndex={activeTaskCharacterIndex}, activeCharacterIndex={activeCharacterIndex}.");

        if (!TryGetTaskOwner(out CharacterRoute character))
            return false;

        int taskIndex = GetNextTaskIndex(character);
        if (taskIndex < 0 || !character.ActiveTaskAccepted)
        {
            Debug.LogWarning($"No active task is currently accepted for '{character.CharacterName}'.");
            return false;
        }

        string requiredMcGuffin = GetRequiredMcGuffinForActiveTask(character);
        if (string.IsNullOrWhiteSpace(requiredMcGuffin))
        {
            Debug.LogWarning($"Task for '{character.CharacterName}' has no required item.");
            return false;
        }

        Debug.Log($"Checking completion for '{character.CharacterName}': required='{requiredMcGuffin}', given='{mcGuffinName}', hasItem={HasMcGuffin(requiredMcGuffin)}.");

        if (!string.Equals(requiredMcGuffin, mcGuffinName, StringComparison.OrdinalIgnoreCase) ||
            !HasMcGuffin(requiredMcGuffin))
        {
            Debug.LogWarning($"Only '{requiredMcGuffin}' can complete the active task for '{character.CharacterName}'.");
            return false;
        }

        character.Tasks[taskIndex].SetCompleted(true);
        character.SetActiveTaskAccepted(false);
        activeTaskCharacterIndex = -1;
        ClearCollectedMcGuffins();
        Debug.Log($"Task completed for '{character.CharacterName}'. Inventory cleared for next task.");
        return true;
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
            activeTaskCharacterName = characters != null && activeTaskCharacterIndex >= 0 && activeTaskCharacterIndex < characters.Length
                ? characters[activeTaskCharacterIndex]?.CharacterName
                : string.Empty,
            characters = saveCharacters,
            collectedMcGuffins = collectedMcGuffins
        };
    }

    public void LoadSaveData(TaskHolderSaveData saveData)
    {
        if (saveData == null || characters == null)
            return;

        if (!string.IsNullOrEmpty(saveData.activeCharacterName))
            SelectCharacter(saveData.activeCharacterName);

        activeTaskCharacterIndex = -1;
        if (!string.IsNullOrEmpty(saveData.activeTaskCharacterName) &&
            TryGetCharacterIndex(saveData.activeTaskCharacterName, out int taskCharacterIndex))
        {
            activeTaskCharacterIndex = taskCharacterIndex;
        }

        if (saveData.characters != null)
        {
            foreach (CharacterTaskSaveData savedCharacter in saveData.characters)
            {
                if (savedCharacter == null || !TryGetCharacter(savedCharacter.characterName, out CharacterRoute character))
                    continue;

                character.SetActiveTaskAccepted(savedCharacter.activeTaskAccepted);
                if (activeTaskCharacterIndex < 0 && savedCharacter.activeTaskAccepted &&
                    TryGetCharacterIndex(savedCharacter.characterName, out int legacyTaskCharacterIndex))
                {
                    activeTaskCharacterIndex = legacyTaskCharacterIndex;
                }

                if (savedCharacter.completedTasks == null || character.Tasks == null)
                    continue;

                for (int taskIndex = 0; taskIndex < character.Tasks.Length && taskIndex < savedCharacter.completedTasks.Length; taskIndex++)
                {
                    if (character.Tasks[taskIndex] != null)
                        character.Tasks[taskIndex].SetCompleted(savedCharacter.completedTasks[taskIndex]);
                }
            }
        }

        collectedMcGuffins = saveData.collectedMcGuffins ?? Array.Empty<string>();
    }

    private int GetNextTaskIndex(CharacterRoute character)
    {
        if (character.Tasks == null)
            return -1;

        for (int taskIndex = 0; taskIndex < character.Tasks.Length; taskIndex++)
        {
            if (character.Tasks[taskIndex] != null && !character.Tasks[taskIndex].IsCompleted)
                return taskIndex;
        }

        return -1;
    }

    private int GetTaskRoomIndex(CharacterRoute character, int taskIndex)
    {
        if (character.Tasks == null || taskIndex < 0 || taskIndex >= character.Tasks.Length ||
            character.Tasks[taskIndex] == null)
        {
            return -1;
        }

        return character.Tasks[taskIndex].RoomIndex;
    }

    private bool HasRequiredMcGuffin(CharacterRoute character, int taskIndex)
    {
        return character.Tasks != null && taskIndex >= 0 && taskIndex < character.Tasks.Length &&
            character.Tasks[taskIndex] != null && HasMcGuffin(character.Tasks[taskIndex].RequiredMcGuffin);
    }

    private string GetRequiredMcGuffinForActiveTask(CharacterRoute character)
    {
        if (character == null || character.Tasks == null || !character.ActiveTaskAccepted)
            return string.Empty;

        int taskIndex = GetNextTaskIndex(character);
        if (taskIndex < 0 || taskIndex >= character.Tasks.Length || character.Tasks[taskIndex] == null)
            return string.Empty;

        return character.Tasks[taskIndex].RequiredMcGuffin ?? string.Empty;
    }

    private void OpenCharacterMiscRoom(CharacterRoute character)
    {
        if (character.MiscRoomIndex < 0)
        {
            Debug.LogError($"Misc Room Index is not configured for '{character.CharacterName}'.");
            return;
        }

        if (BootstrapSceneManager.Instance == null)
        {
            Debug.LogError("BootstrapSceneManager is not available.");
            return;
        }

        BootstrapSceneManager.Instance.LoadRoom(character.MiscRoomIndex);
    }

    public void ClearCollectedMcGuffins()
    {
        collectedMcGuffins = Array.Empty<string>();
    }

    private void RemoveMcGuffin(string mcGuffinName)
    {
        List<string> items = new List<string>(collectedMcGuffins ?? Array.Empty<string>());
        items.RemoveAll(item => string.Equals(item, mcGuffinName.Trim(), StringComparison.OrdinalIgnoreCase));
        collectedMcGuffins = items.ToArray();
    }

    private bool TryGetActiveCharacter(out CharacterRoute character)
    {
        character = null;
        if (characters == null || activeCharacterIndex < 0 || activeCharacterIndex >= characters.Length)
            return false;

        character = characters[activeCharacterIndex];
        return character != null;
    }

    private bool TryGetTaskOwner(out CharacterRoute character)
    {
        character = null;
        if (characters == null)
            return false;

        if (activeTaskCharacterIndex >= 0 && activeTaskCharacterIndex < characters.Length)
        {
            character = characters[activeTaskCharacterIndex];
            if (character != null && character.ActiveTaskAccepted && GetNextTaskIndex(character) >= 0)
                return true;
        }

        if (TryGetAcceptedTaskOwner(out character))
            return true;

        return TryGetActiveCharacter(out character);
    }

    private bool TryGetAcceptedTaskOwner(out CharacterRoute character)
    {
        character = null;
        if (characters == null)
            return false;

        for (int index = 0; index < characters.Length; index++)
        {
            CharacterRoute configuredCharacter = characters[index];
            if (configuredCharacter != null && configuredCharacter.ActiveTaskAccepted && GetNextTaskIndex(configuredCharacter) >= 0)
            {
                character = configuredCharacter;
                activeTaskCharacterIndex = index;
                return true;
            }
        }

        return false;
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

    private bool TryGetCharacterIndex(string characterName, out int characterIndex)
    {
        characterIndex = -1;
        if (characters == null)
            return false;

        for (int index = 0; index < characters.Length; index++)
        {
            if (characters[index] != null &&
                string.Equals(characters[index].CharacterName, characterName, StringComparison.OrdinalIgnoreCase))
            {
                characterIndex = index;
                return true;
            }
        }

        return false;
    }

    private bool TryGetTask(int taskIndex, out TaskRoute task)
    {
        task = null;
        if (!TryGetTaskOwner(out CharacterRoute character) ||
            character.Tasks == null ||
            taskIndex < 0 ||
            taskIndex >= character.Tasks.Length)
        {
            Debug.LogError($"Task index {taskIndex} is outside the active task owner's task list.");
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