using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class HUABotManager : MonoBehaviour
{
    [SerializeField] PlayerMovement enemyPlayer;
    [SerializeField] PlayerMovement botPlayer;
    [SerializeField] BallManager ball;


    public BallTrackDraw.TrackPointInfo[] trackPointInfos;

    Vector3 preBallVel;

    bool botMove = false;


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        botPlayer.moveInputFlag = 0;
        //ball is fly to bot's sides
        if (ball.BallInLeftSide && botPlayer.transform.position.x > 0 
            && preBallVel.x * ball.rb.velocity.x <= 0)
        {
            Vector3[] track = BallTrackDraw.getTrack(ball.rb, ball.transform.position);
            if (BallTrackDraw.isBallReachHeight(ball.rb, ball.transform.position, 0.5f, Time.time, ref trackPointInfos))
                botMove = true;

        }
        else
        {
            //Movement();

        }
        if(botMove)
        {
            MoveBotTo(trackPointInfos[trackPointInfos.Length - 1].position.x, 0.1f);
            if (botPlayer.moveInputFlag == 0)
                botMove = false;
        }
            
        preBallVel = ball.rb.velocity;

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
        
        MoveBotTo(ball.transform.position.x, 0.2f);
    }

    private void Jump()
    {

        if (
            (ball.transform.position.y - botPlayer.transform.position.y > 1) &&
            (ball.transform.position.y - botPlayer.transform.position.y < 3)
           )
        {
            botPlayer.jumpInputFlag = true;
        }

    }
}
