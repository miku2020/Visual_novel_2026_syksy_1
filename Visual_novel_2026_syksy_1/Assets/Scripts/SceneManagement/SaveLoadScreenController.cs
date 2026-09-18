using UnityEngine;

public class SaveLoadScreenController : MonoBehaviour
{
    public void Close()
    {
        if (BootstrapSceneManager.Instance == null)
        {
            Debug.LogError("BootstrapSceneManager is not available.");
            return;
        }

        BootstrapSceneManager.Instance.CloseSaveLoadScreen();
    }
}