using UnityEngine;
using UnityEngine.InputSystem;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private Text_Switch textSwitch;

    private FadeInOut fadeInOut;
    private bool isClosingRoom;

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

            CloseRoomWithFade();
        }
    }


    public void CloseRoomCaller()
    {
        CloseRoomWithFade();
    }

    public void FadeIn()
    {
        FadeInOut fade = GetFadeInOut();
        if (fade != null)
        {
            fade.FadeIn();
        }
    }

    public void FadeOut()
    {
        FadeInOut fade = GetFadeInOut();
        if (fade != null)
        {
            fade.FadeOut();
        }
    }

    private void CloseRoomWithFade()
    {
        BootstrapSceneManager sceneManager = BootstrapSceneManager.Instance;
        if (sceneManager == null)
        {
            Debug.LogError("BootstrapSceneManager is not available.");
            return;
        }

        if (isClosingRoom || sceneManager.IsBusy || sceneManager.CurrentRoomIndex < 0)
        {
            return;
        }

        FadeInOut fade = GetFadeInOut();
        if (fade == null)
        {
            sceneManager.CloseCurrentRoom();
            return;
        }

        isClosingRoom = true;
        fade.FadeIn();
        StartCoroutine(CloseRoomAfterFade(sceneManager));
    }

    private FadeInOut GetFadeInOut()
    {
        if (fadeInOut == null)
        {
            fadeInOut = FindAnyObjectByType<FadeInOut>();
        }

        if (fadeInOut == null)
        {
            Debug.LogError("FadeInOut was not found in the bootstrap scene.", this);
        }

        return fadeInOut;
    }

    private System.Collections.IEnumerator CloseRoomAfterFade(BootstrapSceneManager sceneManager)
    {
        while (fadeInOut != null && fadeInOut.IsFading)
        {
            yield return null;
        }

        sceneManager.CloseCurrentRoom();
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