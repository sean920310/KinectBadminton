using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum GameStates
    {
        GamePreparing,
        InGame,
        GameOver,
    }

    public enum Players
    {
        Player1,
        Player2,
        None,
    }

    [Serializable]
    public struct PlayerInfo
    {
        public string name;
        public int score;
        public int smashCount;
        public int defenceCount;
        public int overhandCount;
        public int underhandCount;

        public PlayerInfo(string Name)
        {
            this.name = Name;
            this.score = 0;
            this.smashCount = 0;
            this.defenceCount = 0;
            this.overhandCount = 0;
            this.underhandCount = 0;
        }
    }
    public static GameManager instance { get; private set; }

    GameStates gameState;

    [SerializeField] int winScore;
    [SerializeField] bool neverFinish; // Endless if true

    [SerializeField] PlayerMovement Player1Movement;
    [SerializeField] PlayerMovement Player2Movement;
    [SerializeField] BallManager Ball;

    [SerializeField] GameObject ServeBorderL;
    [SerializeField] GameObject ServeBorderR;

    [SerializeField] GameObject GameoverPanel;
    [SerializeField] HUDPanel HUD;

    [SerializeField] RectTransform GameStartPanel;
    [SerializeField] Toggle P1BotToggle;
    [SerializeField] Toggle P2BotToggle;
    [SerializeField] TMP_Dropdown scoreToWin;
    [SerializeField] TMP_InputField Player1NameInput;
    [SerializeField] TMP_InputField Player2NameInput;

    [SerializeField] AudioSource PlayerOneWinSound;
    [SerializeField] AudioSource PlayerTwoWinSound;
    [SerializeField] AudioSource GameoverCheeringSound;

    [SerializeField] Transform Player1HatPoint;
    [SerializeField] Transform Player2HatPoint;

    public PlayerInfo Player1Info = new PlayerInfo("Player1");
    public PlayerInfo Player2Info = new PlayerInfo("Player2");

    public Players Winner { get; private set; } = Players.None;

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
        gameState = GameStates.GamePreparing;
        neverFinish = false;
        SetServePlayer(Players.Player1);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Player1Movement.PrepareServe && !Player2Movement.PrepareServe)
        {
            ServeBorderActive(false);
        }
    }

    private bool CheckIsGameover()
    {
        return !neverFinish && gameState != GameStates.GameOver && (Player1Info.score >= winScore || Player2Info.score >= winScore);
    }

    public void p1GetPoint()
    {
        Player1Info.score++;

        // UI Update
        HUD.ScorePanelUpdate(Player1Info.score, Player2Info.score);
        HUD.SetServeHint(true, false);

        // Set Player State 
        SetServePlayer(Players.Player1);
        playerPositionReset();
        StartCoroutine(PlayerMovementDisableForAWhile(0.2f));

        // Set ball Serve State to true
        Ball.ballStates = BallManager.BallStates.Serving;

        ServeBorderActive(true);

        // Check if the game over condition has been satisfied.
        if (CheckIsGameover())
        {
            GameOver();
        }
    }

    public void p2GetPoint()
    {
        Player2Info.score++;

        // UI Update
        HUD.ScorePanelUpdate(Player1Info.score, Player2Info.score);
        HUD.SetServeHint(false, true);

        // Set Player State 
        SetServePlayer(Players.Player2);
        playerPositionReset();
        StartCoroutine(PlayerMovementDisableForAWhile(0.2f));

        // Set ball Serve State to true
        Ball.ballStates = BallManager.BallStates.Serving;

        ServeBorderActive(true);

        // Check if the game over condition has been satisfied.
        if (CheckIsGameover())
        {
            GameOver();
        }
    }

    private void SetServePlayer(Players ServePlayer)
    {
        if(ServePlayer == Players.Player1)
        {
            Player1Movement.PrepareServe = true;
            Player2Movement.PrepareServe = false;
        }
        else
        {
            Player1Movement.PrepareServe = false;
            Player2Movement.PrepareServe = true;
        }
    }

    private void ServeBorderActive(bool active)
    {
        ServeBorderL.SetActive(active);
        ServeBorderR.SetActive(active);
    }

    public void playerPositionReset()
    {
        Player1Movement.transform.localPosition = new Vector3(-3, 1.25f, 0);
        Player2Movement.transform.localPosition = new Vector3(3, 1.25f, 0);
    }

    public void GameOver()
    {
        gameState = GameStates.GameOver;

        // Set Animator UpdateMode to UnscaledTime inorder to play dance animation.
        Player1Movement.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        Player2Movement.animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        if (Player1Info.score >= winScore)
        {
            PlayerOneWinSound.Play();
            Winner = Players.Player1;
            Player1Movement.animator.SetTrigger("Dancing1");
            Player2Movement.animator.SetTrigger("Lose");

        }
        else
        {
            PlayerTwoWinSound.Play();
            Winner = Players.Player2;
            Player1Movement.animator.SetTrigger("Lose");
            Player2Movement.animator.SetTrigger("Dancing1");
        }

        GameoverCheeringSound.Play();
        Time.timeScale = 0.0f;
        HUD.gameObject.SetActive(false);
        GameoverPanel.SetActive(true);

        Player1Movement.enabled = false;
        Player2Movement.enabled = false;
    }

    IEnumerator PlayerMovementDisableForAWhile(float delay)
    {
        Player1Movement.enabled = Player2Movement.enabled = false;

        yield return new WaitForSeconds(delay);
        Player1Movement.ResetInputFlag();
        Player2Movement.ResetInputFlag();
        Player1Movement.enabled = Player2Movement.enabled = true;
    }

    #region Button_Event
    public void OnQuitClick()
    {
        Application.Quit();
    }
    public void OnRematchClick()
    {
        SceneManager.LoadScene(0);
    }

    public void OnStartClick()
    {
        // Get Bot Enable.
        if (P1BotToggle.isOn)
            Player1Movement.GetComponent<BotManager>().enabled = true;
        if (P2BotToggle.isOn)
            Player2Movement.GetComponent<BotManager>().enabled = true;

        // Get Info From Game Start Setting.
        Player1Info.name = Player1NameInput.text;
        Player2Info.name = Player2NameInput.text;

        // Set Hat.
        if (CharacterSlot.HatList[CharacterSlot.player1currentIdx].hatData.HatPrefab != null)
        {
            GameObject tmpHatPrefab = GameObject.Instantiate(CharacterSlot.HatList[CharacterSlot.player1currentIdx].hatData.HatPrefab);
            tmpHatPrefab.transform.SetParent(Player1HatPoint, false);
        }
        if (CharacterSlot.HatList[CharacterSlot.player2currentIdx].hatData.HatPrefab != null)
        {
            GameObject tmpHatPrefab = GameObject.Instantiate(CharacterSlot.HatList[CharacterSlot.player2currentIdx].hatData.HatPrefab);
            tmpHatPrefab.transform.SetParent(Player2HatPoint, false);
        }

        // Ser Winning Score.
        if (scoreToWin.options.ToArray()[scoreToWin.value].text != "Endless")
            int.TryParse(scoreToWin.options.ToArray()[scoreToWin.value].text, out winScore);
        else
            neverFinish = true;

        GameStartPanel.gameObject.SetActive(false);

        Player1Movement.gameObject.SetActive(true);
        Player2Movement.gameObject.SetActive(true);
        Ball.gameObject.SetActive(true);

        Time.timeScale = 1.0f;

        gameState = GameStates.InGame;
    }
    #endregion
}
