using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class playerbox : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI playername;
    [SerializeField]
    private Toggle togglebutton;
    private string playerpath;
    private Button_test uiscript;
    private Gamedata gamedata;

    public void Initialize(Button_test ui, Gamedata data, string name, string path)
    {
        uiscript = ui;
        gamedata = data;
        playerpath = path;

        if (uiscript == null || gamedata == null || playername == null || togglebutton == null)
        {
            Debug.LogError("Player box is missing an initialization reference.");
            return;
        }

        playername.text = name;
        togglebutton.group = uiscript.PlayerField.GetComponent<ToggleGroup>();
    }

    public void callsaveload()
    {
        if (uiscript == null || gamedata == null || string.IsNullOrEmpty(playerpath))
        {
            Debug.LogError("Player box is not initialized correctly.");
            return;
        }

        uiscript.CleanSaveFiles();
        gamedata.getsaves(playerpath);
    }
}
