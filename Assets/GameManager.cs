using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum Winner
    {
        Player1,
        Player2,
        None,
    }

    [SerializeField] int WinScore = 10;

    public int player1Score { get; private set; }
    public int player2Score { get; private set; }
    public int player1Smash = 0;
    public int player2Smash = 0;
    public int player1Defence = 0;
    public int player2Defence = 0;
    public int player1Overhand = 0;
    public int player2Overhand = 0;
    public int player1Underhand = 0;
    public int player2Underhand = 0;

    [SerializeField] PlayerMovement p1;
    [SerializeField] PlayerMovement p2;

    [SerializeField] TextMeshProUGUI p1ScoreText;
    [SerializeField] TextMeshProUGUI p2ScoreText;

    [SerializeField] GameObject p1ServeHint;
    [SerializeField] GameObject p2ServeHint;

    [SerializeField] GameObject serveBorderL;
    [SerializeField] GameObject serveBorderR;

    [SerializeField] GameObject GameoverPanel;
    [SerializeField] GameObject HUD;

    [SerializeField] AudioSource GameoverCheeringSound;

    public Winner winner { get; private set; } = Winner.None;

    public static GameManager instance { get; private set; }

    bool isGameover = false;

    private void Awake()
    {
        Time.timeScale = 1.0f;
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

        p1.PrepareServe = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameover && (player1Score >= WinScore || player2Score >= WinScore))
        {
            isGameover = true;
            GameOver();
        }

        p1ServeHint.SetActive(p1.PrepareServe);
        p2ServeHint.SetActive(p2.PrepareServe);

        if(!p1.PrepareServe && !p2.PrepareServe)
        {
            serveBorderL.SetActive(false);
            serveBorderR.SetActive(false);
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
        serveBorderL.SetActive(true);
        serveBorderR.SetActive(true);
    }
    public void p2GetPoint()
    {
        player2Score++;

        p1.PrepareServe = false;
        p2.PrepareServe = true;

        playerPositionReset();
        serveBorderL.SetActive(true);
        serveBorderR.SetActive(true);
    }

    public void playerPositionReset()
    {
        p1.transform.position = new Vector3(-3, p1.transform.position.y, p1.transform.position.z);
        p2.transform.position = new Vector3(3, p1.transform.position.y, p1.transform.position.z);
    }

    public void GameOver()
    {
        GameoverCheeringSound.Play();
        Time.timeScale = 0.0f;
        HUD.SetActive(false);
        if (player1Score >= WinScore)
        {
            winner = Winner.Player1;
        }
        else
        {
            winner= Winner.Player2;
        }

        GameoverPanel.SetActive(true);
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
    public void OnRematchClick()
    {
        SceneManager.LoadScene(0);
    }
}
