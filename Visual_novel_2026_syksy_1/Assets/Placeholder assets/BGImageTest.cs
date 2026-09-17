using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BGImageTest : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private AssetReferenceSprite[] backgrounds;
    [SerializeField] private AssetReference sceneToLoad;

    private readonly List<AsyncOperationHandle<Sprite>> handles = new();
    private readonly List<Sprite> loadedBackgrounds = new();
    private AsyncOperationHandle<SceneInstance> sceneHandle;
    private bool isLoadingOrUnloadingScene;

    private async void Start()
    {
        foreach (AssetReferenceSprite background in backgrounds)
        {
            AsyncOperationHandle<Sprite> handle =
                background.LoadAssetAsync<Sprite>();

            handles.Add(handle);
            loadedBackgrounds.Add(await handle.Task);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            //ShowBackground(0);
            LoadTestRoom();
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            //ShowBackground(1);
            ;
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            //ShowBackground(2);
            ;
        else if (Keyboard.current.digit4Key.wasPressedThisFrame)
            //ShowBackground(3);
            ;
        else if (Keyboard.current.escapeKey.wasPressedThisFrame)
            UnloadTestRoom();
    }

    private async void LoadTestRoom()
    {
        if (isLoadingOrUnloadingScene || sceneHandle.IsValid())
            return;

        if (sceneToLoad == null || !sceneToLoad.RuntimeKeyIsValid())
            return;

        isLoadingOrUnloadingScene = true;
        sceneHandle = Addressables.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        await sceneHandle.Task;
        isLoadingOrUnloadingScene = false;

        if (sceneHandle.Status != AsyncOperationStatus.Succeeded)
            sceneHandle = default;
    }

    private async void UnloadTestRoom()
    {
        if (isLoadingOrUnloadingScene || !sceneHandle.IsValid())
            return;

        isLoadingOrUnloadingScene = true;
        await Addressables.UnloadSceneAsync(sceneHandle).Task;
        sceneHandle = default;
        isLoadingOrUnloadingScene = false;
    }

    private void ShowBackground(int index)
    {
        if (backgroundImage == null || index >= loadedBackgrounds.Count)
            return;

        backgroundImage.sprite = loadedBackgrounds[index];
    }

    public Sprite GetBackground(int index)
    {
        return loadedBackgrounds[index];
    }

    private void OnDestroy()
    {
        foreach (AsyncOperationHandle<Sprite> handle in handles)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        if (sceneHandle.IsValid())
            Addressables.UnloadSceneAsync(sceneHandle);
    }
}

/*public class AdditiveSceneLoader : MonoBehaviour
{
    [SerializeField] private AssetReference sceneToLoad;

    private AsyncOperationHandle<SceneInstance> sceneHandle;

    private async void Start()
    {
        sceneHandle = Addressables.LoadSceneAsync(
            sceneToLoad,
            LoadSceneMode.Additive
        );

        await sceneHandle.Task;
    }

    private void OnDestroy()
    {
        if (sceneHandle.IsValid())
            Addressables.UnloadSceneAsync(sceneHandle);
    }
}*/

/*
 * Projektin nykyinen tavoite:
 *
 * BG1-BG4 ovat Addressables Sprite-kuvia. Ne ladataan Start-metodissa
 * etukateen, jotta kuvan vaihtaminen numeroilla 1-4 ei aiheuta uutta
 * latausta. Ladatut kuvat pysyvat muistissa niin kauan kuin tama objekti
 * on olemassa, ja ne vapautetaan OnDestroy-metodissa.
 *
 * Tuleva scene-rakenne:
 * SelectionRoom pysyy ladattuna, kun huone ladataan additive-tilassa.
 * Ladatusta huoneesta taytyy sailyttaa AsyncOperationHandle<SceneInstance>
 * ja kutsua Addressables.UnloadSceneAsync(handle), kun palataan takaisin
 * SelectionRoomiin. Muuten sama huone voi latautua useaksi instanssiksi.
 *
 * Kommentoitu AdditiveSceneLoader on vain esimerkki. Varsinainen loader
 * kannattaa sijoittaa SelectionRoomiin tai erilliseen manageriin, ei
 * ladattavan huoneen sisaan. Nain loader pysyy olemassa unloadauksen ajan.
 */