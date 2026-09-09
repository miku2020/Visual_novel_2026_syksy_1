using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable] // serialisoitava Luokka
public class Dialogue 
{
    public string name;

    [TextArea(3, 10)] // (min , max)
    public string[] sentences;
     
}
