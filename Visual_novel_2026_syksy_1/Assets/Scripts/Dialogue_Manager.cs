using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class Dialogue_Manager : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text dialogueText;
public Text_Switch initial;

    [Header("säädä tekstin kirjotus nopeutta")]
    public float typespeed = .1f;
    public float typespeedfast = .05f;
    private float typeWait = 0;
    private bool SentenceComplete = false;

    private Queue<string> Sentences; //FIFO 

    // Start is called before the first frame update
    void Start()
    {       
        Sentences = new Queue<string>();
        initial.TriggerDialogue();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        Debug.Log(dialogue);


        Debug.Log("Starting Conversation with " + dialogue.name);

        
        nameText.text = dialogue.name;

        Sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            Sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (Sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = Sentences.Dequeue();
        StopAllCoroutines();

        StartCoroutine(TypeSentence(sentence));

    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && SentenceComplete == false) // Input for typespeed.
        {
            typeWait = typespeedfast;
            Debug.Log("faster");
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame && SentenceComplete == true)
        {
            DisplayNextSentence();
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        if (dialogueText == null)
        {
            Debug.LogError("Dialogue Text is not assigned!");
            yield break;
        } 
        SentenceComplete = false;
        dialogueText.text = "";
        typeWait = typespeed;
        foreach (char letter in sentence.ToCharArray())
        {

            dialogueText.text += letter;
            yield return new WaitForSeconds(typeWait);

        }
        Debug.Log("sentence completed");
        SentenceComplete = true;


    }

    void EndDialogue()
    {                
        Debug.Log("end of conversation");
    }

}
