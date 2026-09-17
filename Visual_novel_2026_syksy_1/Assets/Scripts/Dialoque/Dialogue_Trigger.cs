using UnityEngine;

public class Dialogue_Trigger : MonoBehaviour
{
    public Dialogue dialogue;
    [SerializeField] private Dialogue_Manager dialogueManager;

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
            dialogueManager.StartDialogue(dialogue);
        }
    }

}

