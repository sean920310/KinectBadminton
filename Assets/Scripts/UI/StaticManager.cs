using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StaticManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI winText;

    [SerializeField] private TextMeshProUGUI P1NameHeader;
    [SerializeField] private TextMeshProUGUI P2NameHeader;

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

        P1NameHeader.text = gameManager.Player1Info.name;
        P2NameHeader.text = gameManager.Player2Info.name;

        P1ScoreText.text = gameManager.Player1Info.score.ToString();
        P2ScoreText.text = gameManager.Player2Info.score.ToString();
        if (gameManager.Player1Info.score > gameManager.Player2Info.score)
            P1ScoreText.color = Color.yellow;
        else if (gameManager.Player1Info.score < gameManager.Player2Info.score)
            P2ScoreText.color = Color.yellow;

        P1SmashText.text = gameManager.Player1Info.smashCount.ToString();
        P2SmashText.text = gameManager.Player2Info.smashCount.ToString();
        if (gameManager.Player1Info.smashCount > gameManager.Player2Info.smashCount)
            P1SmashText.color = Color.yellow;
        else if (gameManager.Player1Info.smashCount < gameManager.Player2Info.smashCount)
            P2SmashText.color = Color.yellow;

        P1DefenseText.text = gameManager.Player1Info.defenceCount.ToString();
        P2DefenseText.text = gameManager.Player2Info.defenceCount.ToString();
        if (gameManager.Player1Info.defenceCount > gameManager.Player2Info.defenceCount)
            P1DefenseText.color = Color.yellow;
        else if (gameManager.Player1Info.defenceCount < gameManager.Player2Info.defenceCount)
            P2DefenseText.color = Color.yellow;

        P1OverhandText.text = gameManager.Player1Info.overhandCount.ToString();
        P2OverhandText.text = gameManager.Player2Info.overhandCount.ToString();

        if (gameManager.Player1Info.overhandCount > gameManager.Player2Info.overhandCount)
            P1OverhandText.color = Color.yellow;
        else if (gameManager.Player1Info.overhandCount < gameManager.Player2Info.overhandCount)
            P2OverhandText.color = Color.yellow;

        P1UnderhandText.text = gameManager.Player1Info.underhandCount.ToString();
        P2UnderhandText.text = gameManager.Player2Info.underhandCount.ToString();
        if (gameManager.Player1Info.underhandCount > gameManager.Player2Info.underhandCount)
            P1UnderhandText.color = Color.yellow;
        else if (gameManager.Player1Info.underhandCount < gameManager.Player2Info.underhandCount)
            P2UnderhandText.color = Color.yellow;

        winText.text = ((gameManager.Winner == GameManager.Players.Player1) ? gameManager.Player1Info.name : gameManager.Player2Info.name) + " Win!";

    }



    // Update is called once per frame
    void Update()
    {
    }
}
