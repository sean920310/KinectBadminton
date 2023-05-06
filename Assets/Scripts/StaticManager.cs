using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StaticManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI winText;

    [SerializeField] private TextMeshProUGUI P1ScoreText;
    [SerializeField] private TextMeshProUGUI P2ScoreText;

    [SerializeField] private TextMeshProUGUI P1SmashText;
    [SerializeField] private TextMeshProUGUI P2SmashText;

    [SerializeField] private TextMeshProUGUI P1DefenseText;
    [SerializeField] private TextMeshProUGUI P2DefenseText;

    [SerializeField] private TextMeshProUGUI P1OverhandText;
    [SerializeField] private TextMeshProUGUI P2OverhandText;

    [SerializeField] private TextMeshProUGUI P1UnderhandText;
    [SerializeField] private TextMeshProUGUI P2UnderhandText;

    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.instance;

        P1ScoreText.text = gameManager.player1Score.ToString();
        P2ScoreText.text = gameManager.player2Score.ToString();
        if (gameManager.player1Score > gameManager.player2Score)
            P1ScoreText.color = Color.yellow;
        else if (gameManager.player1Score < gameManager.player2Score)
            P2ScoreText.color = Color.yellow;

        P1SmashText.text = gameManager.player1Smash.ToString();
        P2SmashText.text = gameManager.player2Smash.ToString();
        if (gameManager.player1Smash > gameManager.player2Smash)
            P1SmashText.color = Color.yellow;
        else if (gameManager.player1Smash < gameManager.player2Smash)
            P2SmashText.color = Color.yellow;

        P1DefenseText.text = gameManager.player1Defence.ToString();
        P2DefenseText.text = gameManager.player2Defence.ToString();
        if (gameManager.player1Defence > gameManager.player2Defence)
            P1DefenseText.color = Color.yellow;
        else if (gameManager.player1Defence < gameManager.player2Defence)
            P2DefenseText.color = Color.yellow;

        P1OverhandText.text = gameManager.player1Overhand.ToString();
        P2OverhandText.text = gameManager.player2Overhand.ToString();

        if (gameManager.player1Overhand > gameManager.player2Overhand)
            P1OverhandText.color = Color.yellow;
        else if (gameManager.player1Overhand < gameManager.player2Overhand)
            P2OverhandText.color = Color.yellow;

        P1UnderhandText.text = gameManager.player1Underhand.ToString();
        P2UnderhandText.text = gameManager.player2Underhand.ToString();
        if (gameManager.player1Underhand > gameManager.player2Underhand)
            P1UnderhandText.color = Color.yellow;
        else if (gameManager.player1Underhand < gameManager.player2Underhand)
            P2UnderhandText.color = Color.yellow;

        winText.text = gameManager.winner.ToString() + " Win!";
    }

    // Update is called once per frame
    void Update()
    {
    }
}
