using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class BootstrapSceneManager : MonoBehaviour
{
    public static BootstrapSceneManager Instance { get; private set; }

    [SerializeField] private AssetReference[] roomScenes = new AssetReference[6];
    [SerializeField] private AssetReference saveLoadScene;
    [SerializeField] private AssetReference toriScene;

    private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> overlayHandles = new(); //O(1) lookup for overlay scenes
    private AsyncOperationHandle<SceneInstance> roomHandle;
    private Scene returnScene;
    private int returnRoomIndex = -1;
    private bool isBusy;

    public bool IsBusy => isBusy;
    public int CurrentRoomIndex { get; private set; } = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadRoom(int roomIndex)
    {
        if (roomIndex < 0 || roomIndex >= roomScenes.Length)
        {
            Debug.LogError($"Room index {roomIndex} is outside the configured room list.");
            return;
        }

        LoadRoomAsync(roomIndex);
    }

    public void CloseCurrentRoom()
    {
        if (isBusy || !roomHandle.IsValid())
            return;

        CloseCurrentRoomAsync();
    }

    public void LoadSaveLoadScreen()
    {
        LoadOverlayAsync(saveLoadScene, "SaveLoad");
    }

    public void CloseSaveLoadScreen()
    {
        UnloadOverlayAsync("SaveLoad");
    }

    public void LoadToriUI()
    {
        LoadOverlayAsync(toriScene, "Tori");
    }

    public void CloseToriUI()
    {
        UnloadOverlayAsync("Tori");
    }

    private async void LoadRoomAsync(int roomIndex)
    {
        if (isBusy || CurrentRoomIndex == roomIndex)
            return;

        AssetReference roomReference = roomScenes[roomIndex];
        if (!IsValidReference(roomReference))
        {
            Debug.LogError($"Room {roomIndex} does not have a valid Addressables scene reference.");
            return;
        }

        isBusy = true;

        if (roomHandle.IsValid())
        {
            returnRoomIndex = CurrentRoomIndex;
            await Addressables.UnloadSceneAsync(roomHandle).Task;
            roomHandle = default;
            CurrentRoomIndex = -1;
        }
        else
        {
            returnScene = SceneManager.GetActiveScene();
        }

        AsyncOperationHandle<SceneInstance> newRoomHandle =
            Addressables.LoadSceneAsync(roomReference, LoadSceneMode.Additive);

        await newRoomHandle.Task;

        if (newRoomHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Could not load room {roomIndex}: {newRoomHandle.OperationException}");
            isBusy = false;
            return;
        }

        roomHandle = newRoomHandle;
        CurrentRoomIndex = roomIndex;
        SceneManager.SetActiveScene(roomHandle.Result.Scene);
        isBusy = false;
    }

    private async void CloseCurrentRoomAsync()
    {
        int roomIndexToReturnTo = returnRoomIndex;
        returnRoomIndex = -1;
        isBusy = true;
        await Addressables.UnloadSceneAsync(roomHandle).Task;
        roomHandle = default;
        CurrentRoomIndex = -1;

        if (roomIndexToReturnTo >= 0)
        {
            isBusy = false;
            LoadRoom(roomIndexToReturnTo);
        }
        else if (returnScene.IsValid())
        {
            SceneManager.SetActiveScene(returnScene);
            isBusy = false;
        }
        else
        {
            isBusy = false;
        }
    }

    private async void LoadOverlayAsync(AssetReference sceneReference, string overlayId)
    {
        if (isBusy || overlayHandles.ContainsKey(overlayId))
            return;

        if (!IsValidReference(sceneReference))
        {
            Debug.LogError($"Overlay {overlayId} does not have a valid Addressables scene reference.");
            return;
        }

        isBusy = true;
        AsyncOperationHandle<SceneInstance> overlayHandle =
            Addressables.LoadSceneAsync(sceneReference, LoadSceneMode.Additive);

        await overlayHandle.Task;

        if (overlayHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Could not load overlay {overlayId}: {overlayHandle.OperationException}");
            isBusy = false;
            return;
        }

        overlayHandles.Add(overlayId, overlayHandle);
        SceneManager.SetActiveScene(overlayHandle.Result.Scene);
        isBusy = false;
    }

    private async void UnloadOverlayAsync(string overlayId)
    {
        if (isBusy || !overlayHandles.TryGetValue(overlayId, out AsyncOperationHandle<SceneInstance> overlayHandle))
            return;

        isBusy = true;
        await Addressables.UnloadSceneAsync(overlayHandle).Task;
        overlayHandles.Remove(overlayId);
        isBusy = false;

        if (roomHandle.IsValid())
            SceneManager.SetActiveScene(roomHandle.Result.Scene);
    }

    private static bool IsValidReference(AssetReference sceneReference)
    {
        return sceneReference != null && sceneReference.RuntimeKeyIsValid();
    }

    private async void OnDestroy()
    {
        if (Instance != this)
            return;

        if (roomHandle.IsValid())
            await Addressables.UnloadSceneAsync(roomHandle).Task;

        foreach (AsyncOperationHandle<SceneInstance> overlayHandle in overlayHandles.Values)
        {
            if (overlayHandle.IsValid())
                Addressables.UnloadSceneAsync(overlayHandle);
        }

        overlayHandles.Clear();
        Instance = null;
    }
}