using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental.GlobalIllumination;

public class GameManager : MonoBehaviour
{
    public enum GameStates
    {
        GamePreparing,
        InGame,
        GamePause,
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

    [Header("Game Information")]
    [SerializeField] GameStartManager gameStarManager;
    [ReadOnly] [SerializeField] GameStates gameState;
    [SerializeField] int winScore;
    [SerializeField] bool neverFinish; // Endless if true

    [Header("GameObject")]
    [SerializeField] PlayerMovement Player1Movement;
    [SerializeField] PlayerMovement Player2Movement;
    [SerializeField] BallManager Ball;

    [SerializeField] Transform Player1HatPoint;
    [SerializeField] Transform Player2HatPoint;

    [SerializeField] GameObject ServeBorderL;
    [SerializeField] GameObject ServeBorderR;

    public PlayerInfo Player1Info = new PlayerInfo("Player1");
    public PlayerInfo Player2Info = new PlayerInfo("Player2");

    [Header("UI")]
    [SerializeField] RectTransform GameoverPanel;
    [SerializeField] HUDPanel HUD;
    [SerializeField] RectTransform PausePanel;
    [SerializeField] RectTransform GameStartPanel;

    [Header("Audio")]
    [SerializeField] AudioSource PlayerOneWinSound;
    [SerializeField] AudioSource PlayerTwoWinSound;
    [SerializeField] AudioSource GameoverCheeringSound;

    [Header("Light")]
    [SerializeField] Light directionalLight;
    [SerializeField] Light spotLight;

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
        Time.timeScale = 0.0f;
        gameState = GameStates.GamePreparing;
        neverFinish = false;
        SetServePlayer(Players.Player1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(gameState == GameStates.GamePause) Resume();
            else Pause();
        }

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
        HUD.P1IsAboutToWin = (Player1Info.score == winScore - 1); 
        HUD.ScorePanelUpdate(Player1Info.score, Player2Info.score);
        HUD.SetServeHint(true, false);

        // Set Player State 
        SetServePlayer(Players.Player1);
        playerPosAndVelocityReset();
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
        HUD.P2IsAboutToWin = (Player2Info.score == winScore - 1);
        HUD.ScorePanelUpdate(Player1Info.score, Player2Info.score);
        HUD.SetServeHint(false, true);

        // Set Player State 
        SetServePlayer(Players.Player2);
        playerPosAndVelocityReset();
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

    // Serve Border will active whenever player is prepare to serve.
    private void ServeBorderActive(bool active)
    {
        ServeBorderL.SetActive(active);
        ServeBorderR.SetActive(active);
    }

    public void playerPosAndVelocityReset()
    {
        Player1Movement.transform.localPosition = new Vector3(-3, 1.25f, 0);
        Player2Movement.transform.localPosition = new Vector3(3, 1.25f, 0);

        Player1Movement.rb.velocity = new Vector3(0, 0f, 0);
        Player2Movement.rb.velocity = new Vector3(0, 0f, 0);
    }

    public void GameOver()
    {
        gameState = GameStates.GameOver;


        // Set Animator UpdateMode to UnscaledTime inorder to play dance animation.
        Player1Movement.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        Player2Movement.animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        if (Player1Info.score > Player2Info.score)
        {
            PlayerOneWinSound.Play();
            Winner = Players.Player1;
            Player1Movement.animator.SetTrigger("Dancing1");
            Player2Movement.animator.SetTrigger("Lose");
        }
        else if (Player1Info.score < Player2Info.score)
        {
            PlayerTwoWinSound.Play();
            Winner = Players.Player2;
            Player1Movement.animator.SetTrigger("Lose");
            Player2Movement.animator.SetTrigger("Dancing1");
        }
        else
        {
            Winner = Players.None;
            Player1Movement.animator.SetTrigger("Dancing1");
            Player2Movement.animator.SetTrigger("Dancing1");
        }

        // Set Light
        directionalLight.gameObject.SetActive(false);
        spotLight.gameObject.SetActive(true);
        switch (Winner)
        {
            case Players.Player1:
                spotLight.transform.position = new Vector3( Player1Movement.transform.position.x, spotLight.transform.position.y, spotLight.transform.position.z);
                break;
            case Players.Player2:
                spotLight.transform.position = new Vector3(Player2Movement.transform.position.x, spotLight.transform.position.y, spotLight.transform.position.z);
                break;
            case Players.None:
                break;
            default:
                break;
        }

        GameoverCheeringSound.Play();
        Time.timeScale = 0.0f;
        HUD.GetComponent<Animator>().SetTrigger("GameEnd");
        GameoverPanel.gameObject.SetActive(true);

        Player1Movement.enabled = false;
        Player2Movement.enabled = false;
    }

    public void Pause()
    {
        gameState = GameStates.GamePause;
        Time.timeScale = 0.0f;
        PausePanel.gameObject.SetActive(true);
    }

    private void Resume()
    {
        gameState = GameStates.InGame;
        Time.timeScale = 1.0f;
        PausePanel.gameObject.SetActive(false);
    }

    IEnumerator PlayerMovementDisableForAWhile(float delay)
    {
        Player1Movement.enabled = Player2Movement.enabled = false;
        Player1Movement.ResetInputFlag();
        Player2Movement.ResetInputFlag();

        yield return new WaitForSeconds(delay);
        Player1Movement.enabled = Player2Movement.enabled = true;
        Player1Movement.ResetInputFlag();
        Player2Movement.ResetInputFlag();
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
        GameStart();
    }

    private void GameStart()
    {
        // Get Bot Enable.
        if (gameStarManager.P1BotToggle.isOn)    
            Player1Movement.GetComponent<BotManager>().enabled = true;
        if (gameStarManager.P2BotToggle.isOn)
            Player2Movement.GetComponent<BotManager>().enabled = true;

        // Get Info From Game Start Setting.
        Player1Info.name = gameStarManager.Player1NameInput.text;
        Player2Info.name = gameStarManager.Player2NameInput.text;

        // Set Hat.
        if (CharacterSlot.HatList[CharacterSlot.player1currentHatIdx].hatData.HatPrefab != null)
        {
            GameObject tmpHatPrefab = GameObject.Instantiate(CharacterSlot.HatList[CharacterSlot.player1currentHatIdx].hatData.HatPrefab);
            tmpHatPrefab.transform.SetParent(Player1HatPoint, false);
        }
        if (CharacterSlot.HatList[CharacterSlot.player2currentHatIdx].hatData.HatPrefab != null)
        {
            GameObject tmpHatPrefab = GameObject.Instantiate(CharacterSlot.HatList[CharacterSlot.player2currentHatIdx].hatData.HatPrefab);
            tmpHatPrefab.transform.SetParent(Player2HatPoint, false);
        }

        // Ser Winning Score.
        if (gameStarManager.scoreToWin.options.ToArray()[gameStarManager.scoreToWin.value].text != "Endless")
            int.TryParse(gameStarManager.scoreToWin.options.ToArray()[gameStarManager.scoreToWin.value].text, out winScore);
        else
            neverFinish = true;

        GameStartPanel.gameObject.SetActive(false);

        Player1Movement.gameObject.SetActive(true);
        Player2Movement.gameObject.SetActive(true);
        Ball.gameObject.SetActive(true);

        Time.timeScale = 1.0f;

        gameState = GameStates.InGame;
    }

    // Pause Panel
    public void OnResumeClick()
    {
        Resume();
    }

    public void OnEndGameClick()
    {
        PausePanel.gameObject.SetActive(false);
        GameOver();
    }
    #endregion
}
