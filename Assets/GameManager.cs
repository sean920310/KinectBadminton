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
            ServeBorderActive(false);
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
        ServeBorderActive(true);
        StartCoroutine(PlayerMovementDisableForAWhile(0.2f));
    }

    public void p2GetPoint()
    {
        player2Score++;

        p1.PrepareServe = false;
        p2.PrepareServe = true;

        playerPositionReset();
        ServeBorderActive(true);
        StartCoroutine(PlayerMovementDisableForAWhile(0.01f));
    }

    private void ServeBorderActive(bool active)
    {
        serveBorderL.SetActive(active);
        serveBorderR.SetActive(active);
    }

    public void playerPositionReset()
    {
        p1.transform.position = new Vector3(-3, p1.transform.position.y, p1.transform.position.z);
        p2.transform.position = new Vector3(3, p2.transform.position.y, p2.transform.position.z);
    }

    public void GameOver()
    {
        if (player1Score >= WinScore)
        {
            winner = Winner.Player1;
            p1.animator.SetTrigger("Dancing1");
            p2.animator.SetTrigger("Lose");

        }
        else
        {
            winner = Winner.Player2;
            p1.animator.SetTrigger("Lose");
            p2.animator.SetTrigger("Dancing1");
        }

        GameoverCheeringSound.Play();
        Time.timeScale = 0.0f;
        HUD.SetActive(false);
        GameoverPanel.SetActive(true);

        p1.enabled = false;
        p2.enabled = false;
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
    public void OnRematchClick()
    {
        SceneManager.LoadScene(0);
    }

    IEnumerator PlayerMovementDisableForAWhile(float delay)
    {
        p1.enabled = p2.enabled = false;

        yield return new WaitForSeconds(delay);
        p1.ResetInputFlag();
        p2.ResetInputFlag();
        p1.enabled = p2.enabled = true;
    }
}
