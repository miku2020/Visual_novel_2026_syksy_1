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

    private enum GreetingState
    {
        Default,
        HasItem,
        MissingItem,
        AllTasksCompleted
    }

    private void SetTaskHolderGreeting()
    {
        if (!useTaskHolderGreeting || TaskHolder.Instance == null || dialogue == null)
            return;

        GreetingState state = GetGreetingState();

        string greeting = state switch
        {
            GreetingState.HasItem => "moi, kyllä on, olehyvä.",
            GreetingState.MissingItem => "moi, ei ole",
            GreetingState.AllTasksCompleted => "moi, kaikki tehtävät on jo suoritettu.",
            _ => "moi."
        };

        if (dialogue.lines != null && dialogue.lines.Length > 0 && dialogue.lines[0] != null)
        {
            dialogue.lines[0].sentence = greeting;
        }
        else if (dialogue.sentences != null && dialogue.sentences.Length > 0)
        {
            dialogue.sentences[0] = greeting;
        }
    }

    private GreetingState GetGreetingState()
    {
        if (TaskHolder.Instance == null)
            return GreetingState.Default;

        if (TaskHolder.Instance.HasActiveTaskForActiveCharacter())
        {
            return TaskHolder.Instance.HasMcGuffinForActiveTask()
                ? GreetingState.HasItem
                : GreetingState.MissingItem;
        }

        if (TaskHolder.Instance.AreAllActiveCharacterTasksCompleted())
        {
            return GreetingState.AllTasksCompleted;
        }

        return GreetingState.Default;
    }

}
