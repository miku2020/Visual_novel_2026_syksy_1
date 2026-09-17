using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highscore_read : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject gameDataObject = GameObject.Find("GameData");
        if (gameDataObject == null)
        {
            Debug.LogError("Highscore_read could not find a GameData object.");
            return;
        }

        Gamedata gamedata = gameDataObject.GetComponent<Gamedata>();
        if (gamedata == null)
        {
            Debug.LogError("Highscore_read found GameData without a Gamedata component.");
            return;
        }

        gamedata.makeHighscore();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
