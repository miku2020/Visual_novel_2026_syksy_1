using UnityEngine;
using UnityEngine.InputSystem;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private Text_Switch textSwitch;

    public static FadeInOut FadeController => FadeInOut.Instance;

    public static void FadeIn()
    {
        if (FadeController == null)
        {
            Debug.LogError("FadeInOut is not available from RoomManager.");
            return;
        }

        FadeController.FadeIn();
    }

    public static void FadeOut()
    {
        if (FadeController == null)
        {
            Debug.LogError("FadeInOut is not available from RoomManager.");
            return;
        }

        FadeController.FadeOut();
    }

    private void Start()
    {
        if (textSwitch != null)
        {
            textSwitch.TriggerDialogue();
        }
    }

    private void Update()
    {
        if (Keyboard.current == null || BootstrapSceneManager.Instance == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            if (BootstrapSceneManager.Instance.IsDetailWindowOpen)
            {
                return;
            }

            if (BootstrapSceneManager.Instance.IsToriOpen)
            {
                BootstrapSceneManager.Instance.CloseToriUI();
                return;
            }

            BootstrapSceneManager.Instance.CloseCurrentRoom();
        }
    }

    public void AddMcGuffin(string mcGuffinName)
    {
        Debug.Log($"RoomManager.AddMcGuffin called with '{mcGuffinName}'.");

        if (TaskHolder.Instance == null)
        {
            Debug.LogError("TaskHolder is not available.");
            return;
        }

        TaskHolder.Instance.AddMcGuffin(mcGuffinName);
    }

    public void SelectCharacterAndOpenMiscRoom(string characterName)
    {
        if (TaskHolder.Instance == null)
        {
            Debug.LogError("TaskHolder is not available.");
            return;
        }

        TaskHolder.Instance.OpenMiscRoomForCharacter(characterName);
    }

    public void CompleteActiveTask(string mcGuffinName)
    {
        Debug.Log($"RoomManager.CompleteActiveTask called with '{mcGuffinName}'.");

        if (TaskHolder.Instance == null)
        {
            Debug.LogError("TaskHolder is not available.");
            return;
        }

        TaskHolder.Instance.TryCompleteActiveTask(mcGuffinName);
    }

    public void OpenActiveTaskMiscRoom()
    {
        Debug.Log("RoomManager.OpenActiveTaskMiscRoom called.");

        if (TaskHolder.Instance == null)
        {
            Debug.LogError("TaskHolder is not available.");
            return;
        }

        TaskHolder.Instance.OpenActiveTaskMiscRoom();
    }

    public void OpenActiveCharacterMiscRoom()
    {
        if (TaskHolder.Instance == null)
        {
            Debug.LogError("TaskHolder is not available.");
            return;
        }

        TaskHolder.Instance.OpenActiveCharacterMiscRoom();
    }

    public void OpenDetailsForCharacter(int characterIndex)
    {
        if (BootstrapSceneManager.Instance == null)
        {
            Debug.LogError("BootstrapSceneManager is not available.");
            return;
        }

        BootstrapSceneManager.Instance.OpenDetailWindow(characterIndex);
    }

    public void OpenToriForCharacter(int characterIndex)
    {
        if (BootstrapSceneManager.Instance == null)
        {
            Debug.LogError("BootstrapSceneManager is not available.");
            return;
        }

        BootstrapSceneManager.Instance.LoadToriUI(characterIndex);
    }
}