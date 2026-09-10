using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

public class Dialogue_Manager : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Text_Switch initial;

    [Header("Tekstin kirjoitusnopeus")]
    [Min(0f)] public float typespeed = .03f;
    [Min(0f)] public float typespeedfast = .005f;
    [Min(0f)] public float spaceHoldThreshold = .15f;
    private bool sentenceComplete;
    private bool isTyping;
    private string currentSentence;
    private Coroutine typingCoroutine;
    private float spaceHoldTime;
    private bool spacePressActive;
    private bool spaceWasHeld;
    private UnityEvent currentNextEvent;
    private int dialogueSession;

    private readonly Queue<DialogueLine> dialogueLines = new Queue<DialogueLine>();

    // Start is called before the first frame update
    void Start()
    {
        if (initial != null)
        {
            initial.TriggerDialogue();
        }
    }


    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogError("Dialogue is not assigned.");
            return;
        }

        dialogueSession++;

        if (nameText != null)
        {
            nameText.text = dialogue.name;
        }

        dialogueLines.Clear();

        if (dialogue.lines != null && dialogue.lines.Length > 0)
        {
            foreach (DialogueLine line in dialogue.lines)
            {
                if (line != null && !string.IsNullOrEmpty(line.sentence))
                {
                    dialogueLines.Enqueue(line);
                }
            }
        }
        else if (dialogue.sentences != null)
        {
            foreach (string sentence in dialogue.sentences)
            {
                if (!string.IsNullOrEmpty(sentence))
                {
                    dialogueLines.Enqueue(new DialogueLine { sentence = sentence });
                }
            }
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (dialogueLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueLines.Dequeue();
        int sessionBeforeEvent = dialogueSession;
        line.beforeEvent?.Invoke();
        if (sessionBeforeEvent != dialogueSession)
        {
            return;
        }

        currentSentence = line.sentence;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeSentence(currentSentence, line.nextEvent));

    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        var spaceKey = Keyboard.current.spaceKey;

        if (spaceKey.wasPressedThisFrame)
        {
            spacePressActive = true;
            spaceHoldTime = 0f;
            spaceWasHeld = false;
        }

        if (spacePressActive && spaceKey.isPressed)
        {
            spaceHoldTime += Time.deltaTime;
            if (spaceHoldTime >= spaceHoldThreshold)
            {
                spaceWasHeld = true;
            }
        }

        if (spacePressActive && spaceKey.wasReleasedThisFrame)
        {
            if (!spaceWasHeld)
            {
                if (isTyping)
                {
                    ShowCurrentSentenceImmediately();
                }
                else if (sentenceComplete)
                {
                    int sessionBeforeEvent = dialogueSession;
                    currentNextEvent?.Invoke();
                    currentNextEvent = null;
                    if (sessionBeforeEvent == dialogueSession)
                    {
                        DisplayNextSentence();
                    }
                }
            }

            spacePressActive = false;
        }
    }

    private IEnumerator TypeSentence(string sentence, UnityEvent nextEvent)
    {
        if (dialogueText == null)
        {
            Debug.LogError("Dialogue Text is not assigned!");
            yield break;
        }

        isTyping = true;
        sentenceComplete = false;
        dialogueText.text = "";

        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            float wait = Keyboard.current != null && Keyboard.current.spaceKey.isPressed
                ? typespeedfast
                : typespeed;
            if (wait > 0f)
            {
                yield return new WaitForSeconds(wait);
            }
        }

        isTyping = false;
        sentenceComplete = true;
        typingCoroutine = null;
        currentNextEvent = nextEvent;
    }

    private void ShowCurrentSentenceImmediately()
    {
        if (dialogueText == null)
        {
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = currentSentence;
        isTyping = false;
        sentenceComplete = true;
    }

    private void EndDialogue()
    {
        sentenceComplete = false;
    }

}
