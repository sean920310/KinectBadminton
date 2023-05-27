using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class HUABotManager : MonoBehaviour
{
    [SerializeField] PlayerMovement enemyPlayer;
    [SerializeField] PlayerMovement botPlayer;
    [SerializeField] BallManager ball;

    [Header("Ball")]
    [SerializeField] Vector2 ballPos;
    [SerializeField] Vector2 ballVel;

    bool preBallInLeftSide;
    float preRangeInBotAndBall;

    bool detectBallIsBack;


    const float RightBottomLine = 7.0f;
    const float LeftBottomLine = -7.0f;
    const float RightSiteCenter = 3.5f;
    const float LeftSiteCenter = -3.5f;
    const float SiteCener = -3.5f;

    bool isJump = false;

    void Start()
    {
        preRangeInBotAndBall = 0;

        ballPos = ball.transform.position;
        ballVel = ball.rb.velocity;
        preBallInLeftSide = ball.BallInLeftSide;
    }

    // Update is called once per frame
    void Update()
    {
        ballPos = ball.transform.position;
        ballVel = ball.rb.velocity;


        if (preBallInLeftSide != ball.BallInLeftSide)
        {
            if (Mathf.Abs(ballVel.x) > 0.000001 && ballPos.y != 0)
            {
                detectBallIsBack = Mathf.Abs(ballVel.x) > 7;
                isJump = false;
                //Debug.Log(ballPos.y + " " + ballVel.x + " " + ballVel.y);
            }
        }

        //ball is not in bot's side
        if ((ball.BallInLeftSide && botPlayer.transform.position.x > 0) ||
            (!ball.BallInLeftSide && botPlayer.transform.position.x < 0))
        {
            preRangeInBotAndBall = 0;
            MoveBotTo(3.5f, 0.05f);
        }
        else
        {
            Movement();
            
        }
        preBallInLeftSide = ball.BallInLeftSide;

    }

    private void MoveBotTo(float x, float moveRange)
    {
        if (botPlayer.transform.position.x > x + moveRange)
        {
            botPlayer.moveInputFlag = -1;
        }
        else if (botPlayer.transform.position.x < x - moveRange)
        {
            botPlayer.moveInputFlag = 1;
        }
        else
        {
            botPlayer.moveInputFlag = 0;
        }
    }

    private void Movement()
    {
        float rangeInBotAndBall = Mathf.Abs(ball.transform.position.x - botPlayer.transform.position.x);
        if (rangeInBotAndBall < 0.5)
        {
            MoveBotTo(ball.transform.position.x, 0.2f);

            Jump();
            botPlayer.jumpInputFlag = true;
            isJump = true;
        }
        else
        {

            if (rangeInBotAndBall > preRangeInBotAndBall)
                detectBallIsBack = !detectBallIsBack;
            if (detectBallIsBack)
                MoveBotTo(RightBottomLine, 0.2f);
            else
                MoveBotTo(SiteCener, -0.2f);
        }
        preRangeInBotAndBall = rangeInBotAndBall;
    }

    private void Jump()
    {

        if (
            !isJump &&
            (ball.transform.position.y - botPlayer.transform.position.y > 1) && 
            (ball.transform.position.y - botPlayer.transform.position.y < 3)
           )
        {
            botPlayer.jumpInputFlag = true;
        }
        
    }
}
