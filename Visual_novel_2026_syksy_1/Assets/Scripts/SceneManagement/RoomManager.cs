using UnityEngine;
using UnityEngine.InputSystem;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private Text_Switch textSwitch;

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
            BootstrapSceneManager.Instance.CloseCurrentRoom();
        }
    }

    public void AddMcGuffin(string mcGuffinName)
    {
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
        if (TaskHolder.Instance == null)
        {
            Debug.LogError("TaskHolder is not available.");
            return;
        }

        TaskHolder.Instance.TryCompleteActiveTask(mcGuffinName);
    }

    public void OpenActiveTaskMiscRoom()
    {
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
}