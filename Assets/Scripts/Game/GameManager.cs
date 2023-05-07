using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum Players
    {
        Player1,
        Player2,
        None,
    }

    [SerializeField] int WinScore = 10;

    [SerializeField] PlayerMovement p1;
    [SerializeField] PlayerMovement p2;
    [SerializeField] BallManager ball;

    [SerializeField] GameObject serveBorderL;
    [SerializeField] GameObject serveBorderR;

    [SerializeField] GameObject GameoverPanel;
    [SerializeField] HUDPanel HUD;

    [SerializeField] RectTransform gameStartPanel;
    [SerializeField] Toggle p1BotToggle;
    [SerializeField] Toggle p2BotToggle;
    [SerializeField] TMP_Dropdown scoreToWin;
    [SerializeField] TMP_InputField player1NameInput;
    [SerializeField] TMP_InputField player2NameInput;

    [SerializeField] AudioSource PlayerOneWinSound;
    [SerializeField] AudioSource PlayerTwoWinSound;
    [SerializeField] AudioSource GameoverCheeringSound;

    [SerializeField] bool Player1Serve;
    [SerializeField] bool Player2Serve;

    [SerializeField] Transform Player1HatPoint;
    [SerializeField] Transform Player2HatPoint;

    public string Player1Name { get; private set; } = "Player1";
    public string Player2Name { get; private set; } = "Player2";

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

    public Players winner { get; private set; } = Players.None;

    public static GameManager instance { get; private set; }

    bool isGameover = false;

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
        //Time.timeScale = 0.0f;
        player1Score = 0;
        player2Score = 0;

        p1.PrepareServe = Player1Serve;
        p2.PrepareServe = Player2Serve;

    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameover && (player1Score >= WinScore || player2Score >= WinScore))
        {
            isGameover = true;
            GameOver();
        }

        if(!p1.PrepareServe && !p2.PrepareServe)
        {
            ServeBorderActive(false);
        }
    }

    public void p1GetPoint()
    {
        player1Score++;

        p1.PrepareServe = true;
        p2.PrepareServe = false;

        HUD.ScorePanelUpdate(player1Score, player2Score);
        HUD.SetServeHint(true, false);

        playerPositionReset();
        ServeBorderActive(true);
        StartCoroutine(PlayerMovementDisableForAWhile(0.2f));
    }

    public void p2GetPoint()
    {
        player2Score++;

        p1.PrepareServe = false;
        p2.PrepareServe = true;

        HUD.ScorePanelUpdate(player1Score, player2Score);
        HUD.SetServeHint(false, true);

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
        p1.transform.position = new Vector3(-3, 1.25f, 0);
        p2.transform.position = new Vector3(3, 1.25f, 0);
    }

    public void GameOver()
    {
        p1.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        p2.animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        if (player1Score >= WinScore)
        {
            PlayerOneWinSound.Play();
            winner = Players.Player1;
            p1.animator.SetTrigger("Dancing1");
            p2.animator.SetTrigger("Lose");

        }
        else
        {
            PlayerTwoWinSound.Play();
            winner = Players.Player2;
            p1.animator.SetTrigger("Lose");
            p2.animator.SetTrigger("Dancing1");
        }

        GameoverCheeringSound.Play();
        Time.timeScale = 0.0f;
        HUD.gameObject.SetActive(false);
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

    public void OnStartClick()
    {
        Time.timeScale = 1.0f;

        // Get Info From Game Start Setting
        if (p1BotToggle.isOn)
            p1.GetComponent<BotManager>().enabled = true;
        if (p2BotToggle.isOn)
            p2.GetComponent<BotManager>().enabled = true;

        Player1Name = player1NameInput.text;
        Player2Name = player2NameInput.text;


        if(CharacterSlot.HatList[CharacterSlot.player1currentIdx].hatData.HatPrefab != null)
        {
            GameObject tmpHatPrefab = GameObject.Instantiate(CharacterSlot.HatList[CharacterSlot.player1currentIdx].hatData.HatPrefab);
            tmpHatPrefab.transform.SetParent(Player1HatPoint, false);
        }

        if (CharacterSlot.HatList[CharacterSlot.player2currentIdx].hatData.HatPrefab != null)
        {
            GameObject tmpHatPrefab = GameObject.Instantiate(CharacterSlot.HatList[CharacterSlot.player2currentIdx].hatData.HatPrefab);
            if (tmpHatPrefab != null)
                tmpHatPrefab.transform.SetParent(Player2HatPoint, false);
        }

        p1.gameObject.SetActive(true);
        p2.gameObject.SetActive(true);
        ball.gameObject.SetActive(true);

        int.TryParse(scoreToWin.options.ToArray()[scoreToWin.value].text, out WinScore);
        gameStartPanel.gameObject.SetActive(false);
    }
}
