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

    [Header("Kirjaimen fade-in")]
    [Min(0f)] public float fadeDuration = .03f;
    [Min(1f)] public float spaceFadeSpeed = 4f;
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
        currentNextEvent = line.nextEvent;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeSentence(currentSentence));

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

    private IEnumerator TypeSentence(string sentence)
    {
        if (dialogueText == null)
        {
            Debug.LogError("Dialogue Text is not assigned!");
            yield break;
        }

        isTyping = true;
        sentenceComplete = false;
        dialogueText.text = sentence;
        dialogueText.alpha = 1f;
        dialogueText.ForceMeshUpdate();

        TMP_TextInfo textInfo = dialogueText.textInfo;
        int characterCount = textInfo.characterCount;
        float totalDuration = characterCount * fadeDuration;
        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            bool spaceHeld = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
            float speed = spaceHeld ? spaceFadeSpeed : 1f;
            elapsed += Time.deltaTime * speed;

            for (int characterIndex = 0; characterIndex < characterCount; characterIndex++)
            {
                TMP_CharacterInfo characterInfo = textInfo.characterInfo[characterIndex];
                if (!characterInfo.isVisible)
                {
                    continue;
                }

                float characterProgress = fadeDuration <= 0f
                    ? 1f
                    : (elapsed - characterIndex * fadeDuration) / fadeDuration;
                byte alpha = (byte)(Mathf.Clamp01(characterProgress) * 255f);
                TMP_MeshInfo meshInfo = textInfo.meshInfo[characterInfo.materialReferenceIndex];
                int vertexIndex = characterInfo.vertexIndex;

                for (int vertexOffset = 0; vertexOffset < 4; vertexOffset++)
                {
                    meshInfo.colors32[vertexIndex + vertexOffset].a = alpha;
                }
            }

            dialogueText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }

        SetAllCharactersAlpha(255);
        dialogueText.alpha = 1f;
        isTyping = false;
        sentenceComplete = true;
        typingCoroutine = null;
    }

    private void SetAllCharactersAlpha(byte alpha)
    {
        dialogueText.ForceMeshUpdate();
        TMP_TextInfo textInfo = dialogueText.textInfo;

        for (int characterIndex = 0; characterIndex < textInfo.characterCount; characterIndex++)
        {
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[characterIndex];
            if (!characterInfo.isVisible)
            {
                continue;
            }

            TMP_MeshInfo meshInfo = textInfo.meshInfo[characterInfo.materialReferenceIndex];
            int vertexIndex = characterInfo.vertexIndex;
            for (int vertexOffset = 0; vertexOffset < 4; vertexOffset++)
            {
                meshInfo.colors32[vertexIndex + vertexOffset].a = alpha;
            }
        }

        dialogueText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
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
    SetAllCharactersAlpha(255);
        dialogueText.alpha = 1f;
        isTyping = false;
        sentenceComplete = true;
    }

    private void EndDialogue()
    {
        sentenceComplete = false;
    }

}
