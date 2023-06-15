using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PositioningManager : MonoBehaviour
{
    [Serializable]
    public enum PlayerCount
    {
        Solo,
        Dual,
        AllBots
    }
    [Serializable]
    public enum PositioningStates
    {
        Init,
        Positioning, // Player Position Incorrect State.
        PositioningCorrect, // Player Position Correct State.
        PositioningEnd,
    }

    [Serializable]
    public enum PlayerPosState
    {
        Correct     = 1 << 0,
        TooLeft     = 1 << 1,
        TooRight    = 1 << 2,
        TooClose    = 1 << 3,
        TooFar      = 1 << 4,
    }

    [Serializable]
    public struct PlayerInfo
    {
        [ReadOnly]
        [SerializeField]
        private PlayerPosState playerPosState;

        public void resetPlayerPosState()
        {
            playerPosState = 0;
        }

        public void setState(PlayerPosState state, bool isOpen)
        {
            if (isOpen)
            {
                playerPosState = playerPosState | state;

            }
            else
            {
                PlayerPosState tmp = state;
                tmp = ~tmp;
                playerPosState = playerPosState & tmp;
            }
        }

        public bool getState(PlayerPosState state)
        {
            PlayerPosState tmp = state;
            return ((playerPosState & tmp) != 0);
        }
    }

    public static readonly string[] instruction = {
        "Great",
        "Too Left",
        "Too Right",
        "Too Close",
        "Too Far"
    };

    public static PositioningManager instance { get; private set; }
    private bool isPositioningEnd = false;
    public PlayerCount playerCount;
    public PositioningStates positioningStates = PositioningStates.Init;

    public float CorrectTime;
    private float CorrectTimeCounter;


    [SerializeField] GameObject KinectOBJ;
    [SerializeField] RectTransform KinectCameraOutputPanel;
    [SerializeField] RectTransform PositioningPanel;

    [SerializeField] RectTransform CorrectCountDownPanel;
    [SerializeField] TextMeshProUGUI CorrectCountDownText;

    // solo ui
    [SerializeField] RectTransform SoloPanel;
    [SerializeField] PositioningUI SoloPlayerPositioningUI;

    // dual ui
    [SerializeField] RectTransform DualPanel;
    [SerializeField] PositioningUI DualLeftPlayerPositioningUI;
    [SerializeField] PositioningUI DualRightPlayerPositioningUI;

    // full info
    [Header("Solo")]
    [SerializeField] public PlayerInfo SoloPlayerInfo;

    [Header("Dual")]
    [SerializeField] public PlayerInfo DualLeftPlayerInfo;
    [SerializeField] public PlayerInfo DualRightPlayerInfo;

    [SerializeField] Color BodyPosCorrectColor;
    [SerializeField] Color BodyPosIncorrectColor;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        // Initialize
        positioningStates = PositioningStates.Init;
    }

    // Use Init to Start Positioning.
    public void Init(PlayerCount playerCount) 
    {
        this.playerCount = playerCount;

        positioningStates = PositioningStates.Positioning;
        KinectOBJ.gameObject.SetActive(true);
        KinectCameraOutputPanel.gameObject.SetActive(true);
        switch (playerCount)
        {
            case PlayerCount.Solo:
                SoloPanel.gameObject.SetActive(true);
                DualPanel.gameObject.SetActive(false);
                break;
            case PlayerCount.Dual:
                SoloPanel.gameObject.SetActive(false);
                DualPanel.gameObject.SetActive(true);
                break;
            case PlayerCount.AllBots:
                SoloPanel.gameObject.SetActive(false);
                DualPanel.gameObject.SetActive(false);
                positioningStates = PositioningStates.PositioningEnd;
                break;
            default:
                break;
        }

        Time.timeScale = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPositioningEnd) return;

        if(positioningStates == PositioningStates.Positioning)
        {
            switch (playerCount)
            {
                case PlayerCount.Solo:
                    if (SoloPlayerInfo.getState(PlayerPosState.Correct) == true)
                    {
                        // switch state to PositioningCorrect
                        positioningStates = PositioningStates.PositioningCorrect;
                        CorrectTimeCounter = CorrectTime;
                        CorrectCountDownPanel.gameObject.SetActive(true);
                    }

                    UpdatePlayerStateUI(SoloPlayerInfo, SoloPlayerPositioningUI);
                    break;
                case PlayerCount.Dual:
                    if (DualLeftPlayerInfo.getState(PlayerPosState.Correct) == true && 
                        DualRightPlayerInfo.getState(PlayerPosState.Correct) == true)
                    {
                        // switch state to PositioningCorrect
                        positioningStates = PositioningStates.PositioningCorrect;
                        CorrectTimeCounter = CorrectTime;
                        CorrectCountDownPanel.gameObject.SetActive(true);

                    }
                    UpdatePlayerStateUI(DualLeftPlayerInfo, DualLeftPlayerPositioningUI);
                    UpdatePlayerStateUI(DualRightPlayerInfo, DualRightPlayerPositioningUI);
                    break;
                default:
                    break;
            }
        }
        else if (positioningStates == PositioningStates.PositioningCorrect)
        {
            // Player Position Correct.
            switch (playerCount)
            {
                case PlayerCount.Solo:
                    if (SoloPlayerInfo.getState(PlayerPosState.Correct) == true)
                    {
                        // if positioning correct: count down, when counter reach zero, switch state to "PositioningEnd"
                        CorrectTimeCounter -= Time.deltaTime;
                        if (CorrectTimeCounter <= 0.0f)
                        {
                            positioningStates = PositioningStates.PositioningEnd;
                        }
                    }
                    else
                    {
                        // else (if positioning incorrect): switch state back to "Positioning".
                        positioningStates = PositioningStates.Positioning;
                        CorrectCountDownPanel.gameObject.SetActive(false);
                    }
                    CorrectCountDownText.text = ((int)CorrectTimeCounter).ToString();
                    UpdatePlayerStateUI(SoloPlayerInfo, SoloPlayerPositioningUI);
                    break;
                case PlayerCount.Dual:
                    if (DualLeftPlayerInfo.getState(PlayerPosState.Correct) == true &&
                        DualRightPlayerInfo.getState(PlayerPosState.Correct) == true)
                    {
                        // if positioning correct: count down, when counter reach zero, switch state to "PositioningEnd"
                        CorrectTimeCounter -= Time.deltaTime;
                        if (CorrectTimeCounter <= 0.0f)
                        {
                            positioningStates = PositioningStates.PositioningEnd;
                        }
                    }
                    else
                    {
                        // else (if positioning incorrect): switch state back to "Positioning".
                        positioningStates = PositioningStates.Positioning;
                        CorrectCountDownPanel.gameObject.SetActive(false);
                    }
                    CorrectCountDownText.text = ((int)CorrectTimeCounter).ToString();
                    UpdatePlayerStateUI(DualLeftPlayerInfo, DualLeftPlayerPositioningUI);
                    UpdatePlayerStateUI(DualRightPlayerInfo, DualRightPlayerPositioningUI);
                    break;
                default:
                    break;
            }
        }
        else if (positioningStates == PositioningStates.PositioningEnd)
        {
            isPositioningEnd = true;
            KinectCameraOutputPanel.gameObject.SetActive(false);
            PositioningPanel.gameObject.SetActive(false);
            GameManager.instance.GameStart();
        }
    }

    private void UpdatePlayerStateUI(PlayerInfo playerInfo, PositioningUI positioningUI)
    {
        if (playerInfo.getState(PlayerPosState.Correct) == true)
        {
            // Update Correct UI
            positioningUI.BodyImage.color = BodyPosCorrectColor;
            positioningUI.messageText.text = instruction[0];
        }
        else
        {
            // Update Incorrect UI
            positioningUI.BodyImage.color = BodyPosIncorrectColor;
            for (int idx = 1; idx <= 4; idx++)
            {
                if (playerInfo.getState((PlayerPosState)(1 << idx)))
                {
                    positioningUI.messageText.text = instruction[idx];
                    break;
                }
            }
        }
    }
}
