using UnityEngine;
using TMPro;

public class Text_Switch : MonoBehaviour
{

    //public TMP_Text Text;
    public Dialogue dialogue;
    [SerializeField] private bool useTaskHolderGreeting;
    /*[SerializeField]*/ private Dialogue_Manager dialogueManager;

    public void NameChange(string newName)
    {
        dialogueManager.nameText.text = newName;
    }

    private void Awake()
    {
        if (dialogueManager == null)
        {
            dialogueManager = FindAnyObjectByType<Dialogue_Manager>();
        }
    }


    public void TriggerDialogue()
    {
        if (dialogueManager != null)
        {
            SetTaskHolderGreeting();
            dialogueManager.StartDialogue(dialogue);
        }
    }

    private void SetTaskHolderGreeting()
    {
        if (!useTaskHolderGreeting || TaskHolder.Instance == null || dialogue == null)
            return;

        string greeting = TaskHolder.Instance.HasActiveTaskForActiveCharacter()
            ? TaskHolder.Instance.HasMcGuffinForActiveTask()
                ? "moi, kyllä on, olehyvä."
                : "moi, ei ole"
            : "moi.";

        if (dialogue.lines != null && dialogue.lines.Length > 0 && dialogue.lines[0] != null)
        {
            dialogue.lines[0].sentence = greeting;
        }
        else if (dialogue.sentences != null && dialogue.sentences.Length > 0)
        {
            dialogue.sentences[0] = greeting;
        }
    }

}
