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

        P1SmashText.text = gameManager.player1Smash.ToString();
        P2SmashText.text = gameManager.player2Smash.ToString();

        P1DefenseText.text = gameManager.player1Defence.ToString();
        P2DefenseText.text = gameManager.player2Defence.ToString();

        P1OverhandText.text = gameManager.player1Overhand.ToString();
        P2OverhandText.text = gameManager.player2Overhand.ToString();

        P1UnderhandText.text = gameManager.player1Underhand.ToString();
        P2UnderhandText.text = gameManager.player2Underhand.ToString();

        winText.text = gameManager.winner.ToString() + " Win!";
    }

    // Update is called once per frame
    void Update()
    {
    }
}
