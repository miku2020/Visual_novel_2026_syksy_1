using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Text_Switch : MonoBehaviour
{

    public TMP_Text Text;
    public Dialogue dialogue;



    public void TriggerDialogue()
    {
        FindObjectOfType<Dialogue_Manager>().StartDialogue(dialogue);

        }

}
