using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int player1Score = 0;
    public static int player2Score = 0;

    [SerializeField] TextMeshProUGUI scoreText;
    // Start is called before the first frame update
    void Start()
    {
        player1Score = 0;
        player2Score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = player1Score.ToString() + " : " + player2Score.ToString();
    }
}
