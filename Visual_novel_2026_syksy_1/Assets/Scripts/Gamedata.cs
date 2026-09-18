using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;


public class Gamedata : MonoBehaviour
{

    public PlayerData playerdata;
    PlayerData loadedData;

    private string JSON_FILE_NAME = "TestSave.json";
    private string JsonDirectory;
    [SerializeField]
    private Button_test UIscript;

    [SerializeField]
    private TaskHolder taskHolder;

    public string loadedname;
    public int loadedscore;
    public DateTime loadedtime;
    public string loadeddir;

    [SerializeField]
    TMP_Text scorebox;

    private void Awake()
    {
        if (playerdata == null)
        {
            playerdata = new PlayerData();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            playerdata.Score++;
            Debug.Log("Space key was pressed.");
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SaveJsonData("/TestSave.json");
        }
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            LoadJsonData();
        }




    }
    // V1:
    // public void SaveJsonData(string save_input)
    // {
    //     JSON_FILE_NAME = "/" + playerdata.playerName + "/" + save_input + ".json";
    //     JsonDirectory = "/" + playerdata.playerName;
    //     string Json = JsonUtility.ToJson(playerdata);
    //     string filepath = Application.persistentDataPath + JSON_FILE_NAME;
    //     string jsondir = Application.persistentDataPath + JsonDirectory;
    //     if (Directory.Exists(filepath))
    //     {
    //         File.WriteAllText(filepath, Json);
    //     }
    //     else
    //     {
    //         Directory.CreateDirectory(jsondir);
    //         File.WriteAllText(filepath, Json);
    //     }
    // }

    // V2: build paths with Path.Combine and treat the save path as a file path.
    public void SaveJsonData(string save_input)
    {
        if (playerdata == null)
        {
            Debug.LogWarning("Cannot save because playerdata has not been initialized.");
            return;
        }

        if (string.IsNullOrWhiteSpace(playerdata.playerName))
        {
            Debug.LogWarning("Cannot save because the player name is empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(save_input))
        {
            Debug.LogWarning("Cannot save because the save name is empty.");
            return;
        }

        string fileName = Path.GetFileName(save_input);
        if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".json";
        }

        JsonDirectory = Path.Combine(Application.persistentDataPath, playerdata.playerName);
        JSON_FILE_NAME = fileName;
        string filepath = Path.Combine(JsonDirectory, fileName);
        Directory.CreateDirectory(JsonDirectory);

        if (taskHolder != null)
        {
            playerdata.taskData = taskHolder.CreateSaveData();
        }

        File.WriteAllText(filepath, JsonUtility.ToJson(playerdata));

        Debug.Log("save created: " + filepath);
    }

    // V1:
    // public void LoadJsonData()
    // {
    //     string filepath = Application.persistentDataPath + JSON_FILE_NAME;
    //     string json = System.IO.File.ReadAllText(filepath);
    //     playerdata = JsonUtility.FromJson<PlayerData>(json);
    // }

    // V2: verify that the selected save exists before reading it.
    public void LoadJsonData()
    {
        string filepath = Path.Combine(JsonDirectory ?? Application.persistentDataPath, JSON_FILE_NAME);
        if (!File.Exists(filepath))
        {
            Debug.LogWarning("Save file not found: " + filepath);
            return;
        }

        string json = File.ReadAllText(filepath);
        playerdata = JsonUtility.FromJson<PlayerData>(json);

        if (taskHolder != null)
        {
            taskHolder.LoadSaveData(playerdata.taskData);
        }

        Debug.Log("my Json Data is " + playerdata);
    }

    public void getDirectiories()
    {
        if (UIscript == null)
        {
            Debug.LogError("Gamedata is missing its UI script reference.");
            return;
        }

        string[] dirs = Directory.GetDirectories(Application.persistentDataPath);
        foreach (string dir in dirs)
        {
            Debug.Log("dir on "+dir);
            string dirname = Path.GetFileName(Path.GetFileName(dir));
            //Debug.Log(dir);
            UIscript.InstantiatePlayerdirs(dirname, dir);
        }

    }

    public void getsaves(string path)
    {
        if (UIscript == null)
        {
            Debug.LogError("Gamedata is missing its UI script reference.");
            return;
        }

        //string[] savedirs = Directory.GetDirectories(path + "/");
        if (!Directory.Exists(path))
        {
            Debug.LogWarning("Save directory not found: " + path);
            return;
        }

        string[] savedirs = Directory.GetFiles(path, "*.json");
        foreach (string dir in savedirs)
        {
            try
            {
                var filedate = File.GetLastWriteTime(dir);
                string json = File.ReadAllText(dir);
                loadedData = JsonUtility.FromJson<PlayerData>(json);
                if (loadedData == null)
                {
                    continue;
                }

                loadeddir = dir;
                loadedname = loadedData.playerName;
                loadedscore = loadedData.Score;
                loadedtime = filedate;
                string saveName = Path.GetFileNameWithoutExtension(dir);
                UIscript.InstantiateSaveDirs(saveName, loadedname, loadedscore, loadedtime, loadeddir);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not read save file " + dir + ": " + exception.Message);
            }
        }
    }

    public void makeHighscore()
    {
        if (scorebox == null)
        {
            Debug.LogError("Gamedata is missing its highscore text reference.");
            return;
        }

        string[] dirs = Directory.GetDirectories(Application.persistentDataPath);

        List<(string Name, int Score, DateTime filedate)> data = new List<(string Name, int Score, DateTime filedate)>();

        foreach (string dir in dirs) 
        {
            string[] saveFiles = Directory.GetFiles(dir, "*.json");
            foreach (string file in saveFiles)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    PlayerData loadedData = JsonUtility.FromJson<PlayerData>(json);
                    if (loadedData == null || string.IsNullOrWhiteSpace(loadedData.playerName))
                    {
                        continue;
                    }

                    DateTime filedate = File.GetLastWriteTime(file);
                    data.Add((loadedData.playerName, loadedData.Score, filedate));
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Could not read highscore file " + file + ": " + exception.Message);
                }
            }
        }
        var scoreQuery = from scores in data
                         orderby scores.Score descending, scores.filedate descending
                         select new { scores.Name, scores.Score };
        scorebox.text = string.Join("\n", scoreQuery.Select(entry => $"{entry.Name}: {entry.Score}"));
    }

   

    public void SetPlayerData(string playername, int playerscore)
    {
        if (playerdata == null)
        {
            playerdata = new PlayerData();
        }

        playerdata.playerName = playername;
        playerdata.Score = playerscore;
        Debug.Log("data set");
    }
}