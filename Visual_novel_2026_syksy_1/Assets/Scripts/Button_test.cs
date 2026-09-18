using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Button_test : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI input;
    [SerializeField]
    private Gamedata gamedata;


    public GameObject PlayerField;
    [SerializeField]
    private GameObject playerboxPrefab;

    [SerializeField]
    GameObject saveField;
    [SerializeField]
    GameObject SaveBoxPrefab;

    public string playerpath;
    public string playersname;
    public void callbutton()
    {
        if (gamedata == null || input == null)
        {
            Debug.LogError("Save button is missing its references.");
            return;
        }

        gamedata.SaveJsonData(input.text);
    }

    public void refreshButton()
    {
        if (gamedata == null || PlayerField == null)
        {
            Debug.LogError("Refresh button is missing its references.");
            return;
        }

        cleanPlayerDirs();
        gamedata.getDirectiories();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.enterKey.wasReleasedThisFrame)
        {
            if (gamedata == null || input == null)
            {
                Debug.LogError("Save input is missing its references.");
                return;
            }

            gamedata.SaveJsonData(input.text);
            input.text = "";
        }
    }

    public void InstantiatePlayerdirs(string name, string directory)
    {        
        if (playerboxPrefab == null || PlayerField == null)
        {
            Debug.LogError("Player directory list is missing its prefab or parent reference.");
            return;
        }

        playerpath = directory;
        playersname = name;
        GameObject playerObject = Instantiate(playerboxPrefab, PlayerField.transform);
        playerbox playerBox = playerObject.GetComponent<playerbox>();
        if (playerBox == null)
        {
            Debug.LogError("Player directory prefab is missing its playerbox component.");
            Destroy(playerObject);
            return;
        }

        playerBox.Initialize(this, gamedata, name, directory);
    }

    public void cleanPlayerDirs()
    {
        if (PlayerField == null)
        {
            Debug.LogError("Player directory list is missing its parent reference.");
            return;
        }

        foreach (Transform child in PlayerField.transform) { GameObject.Destroy(child.gameObject); }
    }

    public void CleanSaveFiles()
    {
        if (saveField == null)
        {
            Debug.LogError("Save list is missing its parent reference.");
            return;
        }

        foreach (Transform child in saveField.transform) { GameObject.Destroy(child.gameObject); }
    }
    public void InstantiateSaveDirs(string saveName, string playerName, int score, System.DateTime date, string directory)
    {        
        if (SaveBoxPrefab == null || saveField == null)
        {
            Debug.LogError("Save list is missing its prefab or parent reference.");
            return;
        }

        GameObject saveObject = Instantiate(SaveBoxPrefab, saveField.transform);
        savebox saveBox = saveObject.GetComponent<savebox>();
        if (saveBox == null)
        {
            Debug.LogError("Save box prefab is missing its savebox component.");
            Destroy(saveObject);
            return;
        }

        saveBox.Initialize(gamedata, saveName, playerName, score, date, directory);
    }


}
