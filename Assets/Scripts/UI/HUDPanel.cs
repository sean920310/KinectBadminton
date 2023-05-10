using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDPanel : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI p1ScoreText;
    [SerializeField] TextMeshProUGUI p2ScoreText;
    [SerializeField] Color MatchPointColor;
    [SerializeField] Color NormalColor;
    public bool P1IsAboutToWin = false;
    public bool P2IsAboutToWin = false;

    [SerializeField] GameObject p1ServeHint;
    [SerializeField] GameObject p2ServeHint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScorePanelUpdate(int Player1Score, int Player2Score)
    {
        p1ScoreText.text = Player1Score.ToString();
        p2ScoreText.text = Player2Score.ToString();

        p1ScoreText.color = P1IsAboutToWin ? MatchPointColor : NormalColor;
        p2ScoreText.color = P2IsAboutToWin ? MatchPointColor : NormalColor;
    }
    public void SetServeHint(bool Player1Serve, bool Player2Serve)
    {
        p1ServeHint.SetActive(Player1Serve);
        p2ServeHint.SetActive(Player2Serve);
    }
}
