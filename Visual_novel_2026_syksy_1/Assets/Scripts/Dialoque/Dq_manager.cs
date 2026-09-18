using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dq_manager : MonoBehaviour
{
    [SerializeField]
    private Image text_window;
    [SerializeField]
    private Vector2 TextWindowOffScreenPos;
    private Vector2 TextWindowOrgPos;
    [SerializeField]
    private float TextWindowLerpTime = 0f;


    // Start is called before the first frame update
    void Start()
    {
        TextWindowOrgPos = text_window.GetComponent<RectTransform>().anchoredPosition;
        text_window.GetComponent<RectTransform>().anchoredPosition = TextWindowOffScreenPos;
        
    }


    public void InitializeDQ()
    {

        StartCoroutine(TextWindowLerp());
    }

    public void closeDQ()
    {
        StartCoroutine(TextWindowLerpDown());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }

    private IEnumerator TextWindowLerp()
    {
        Debug.Log("yes DQ initialized");
        float elapsedTime = 0f;

        for (float t = 0f; t <1f; t += Time.deltaTime / TextWindowLerpTime)
        {
            text_window.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(TextWindowOffScreenPos, TextWindowOrgPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //text_window.transform.position = TextWindowOrgPos;

        
    }

    private IEnumerator TextWindowLerpDown()
    {
        Debug.Log("yes DQ initialized");
        float elapsedTime = 0f;

        for (float t = 0f; t < 1f; t += Time.deltaTime / TextWindowLerpTime)
        {
            text_window.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(TextWindowOrgPos, TextWindowOffScreenPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

}
