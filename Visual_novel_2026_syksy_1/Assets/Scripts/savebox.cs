using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class savebox : MonoBehaviour
{
    private string savename;
    private int savescore;
    private System.DateTime savedate;
    private string dir;
    [SerializeField]
    private TextMeshProUGUI ScoreBox;
    [SerializeField]
    private TextMeshProUGUI DateBox;
    [SerializeField]
    private TextMeshProUGUI NameBox;

    private Gamedata gamedata;

    public void Initialize(Gamedata data, string saveName, int score, System.DateTime date, string directory)
    {
        gamedata = data;
        savename = saveName;
        savescore = score;
        savedate = date;
        dir = directory;

        if (gamedata == null || ScoreBox == null || DateBox == null || NameBox == null)
        {
            Debug.LogError("Save box is missing an initialization reference.");
            return;
        }

        ScoreBox.text = savescore.ToString();
        DateBox.text = savedate.ToString();
        NameBox.text = savename;
    }

    public void loadsave()
    {
        if (gamedata == null || string.IsNullOrWhiteSpace(dir))
        {
            Debug.LogError("Save box is not initialized correctly.");
            return;
        }

        gamedata.LoadSaveFile(dir);
    }
}
