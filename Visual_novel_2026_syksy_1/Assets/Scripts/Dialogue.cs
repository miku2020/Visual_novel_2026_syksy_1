using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueLine
{
    [TextArea(3, 10)]
    public string sentence;

    [Header("Before sentence")]
    public UnityEvent beforeEvent;

    [Header("Next event")]
    public UnityEvent nextEvent;
}

[System.Serializable]
public class Dialogue 
{
    public string name;

    [Tooltip("Uusi rakenne: lause ja tapahtumat samassa Inspector-kohdassa.")]
    public DialogueLine[] lines;

    [Tooltip("Vanha rakenne. Kaytetaan vain, jos Lines on tyhja.")]
    [TextArea(3, 10)] // (min , max)
    public string[] sentences;
}
