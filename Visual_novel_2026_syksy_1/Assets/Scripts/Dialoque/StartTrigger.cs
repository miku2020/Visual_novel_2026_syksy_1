using UnityEngine;

public class StartTrigger : MonoBehaviour
{
    public Text_Switch textSwitch;
    private Dialogue_Manager dialogueManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      textSwitch.TriggerDialogue();
    }

    
}
