using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatesPanel : MonoBehaviour
{
    Animator animator;
    TextMeshProUGUI text;
    void Start()
    {
        animator = GetComponent<Animator>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ShowMessageRight(string message)
    {
        text.text = message;
        animator.SetTrigger("ShowRight");
    }
    public void ShowMessageLeft(string message)
    {
        text.text = message;
        animator.SetTrigger("ShowLeft");
    }
}
