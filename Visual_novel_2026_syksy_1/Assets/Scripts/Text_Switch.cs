using UnityEngine;
using TMPro;

public class Text_Switch : MonoBehaviour
{

    public TMP_Text Text;
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
