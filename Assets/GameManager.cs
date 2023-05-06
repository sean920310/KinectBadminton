using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] int WinScore = 10;

    public int player1Score { get; private set; }
    public int player2Score { get; private set; }

    [SerializeField] PlayerMovement p1;
    [SerializeField] PlayerMovement p2;

    [SerializeField] TextMeshProUGUI p1ScoreText;
    [SerializeField] TextMeshProUGUI p2ScoreText;

    public static GameManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Found more than one GameManager in the scene. Destroying the newest one.");
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        player1Score = 0;
        player2Score = 0;

    }

    // Update is called once per frame
    void Update()
    {
        if (player1Score >= WinScore || player2Score >= WinScore)
        {
            GameOver();
        }

        p1ScoreText.text = player1Score.ToString();
        p2ScoreText.text = player2Score.ToString();
    }

    public void p1GetPoint()
    {
        player1Score++;

        p1.PrepareServe = true;
        p2.PrepareServe = false;

        playerPositionReset();
    }
    public void p2GetPoint()
    {
        player2Score++;

        p1.PrepareServe = false;
        p2.PrepareServe = true;

        playerPositionReset();
    }

    public void playerPositionReset()
    {
        p1.transform.position = new Vector3(-3, p1.transform.position.y, p1.transform.position.z);
        p2.transform.position = new Vector3(3, p1.transform.position.y, p1.transform.position.z);
    }

    public void GameOver()
    {

    }
}
