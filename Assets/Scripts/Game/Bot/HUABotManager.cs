using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class HUABotManager : MonoBehaviour
{
    [SerializeField] PlayerMovement enemyPlayer;
    [SerializeField] PlayerMovement botPlayer;
    [SerializeField] BallManager ball;

    [Serializable]
    public struct HitPointInfo
    {
        public Transform transform;
        public float time;
    };

    [SerializeField] HitPointInfo[] UnderHandBack;
    [SerializeField] HitPointInfo[] UnderHandFront;
    [SerializeField] HitPointInfo[] OverHand;
    [SerializeField] HitPointInfo[] Smash;

    public BallTrackDraw.TrackPointInfo[] trackPointInfos;

    Vector3 preBallVel;

    bool botMove = false;
    bool botSwing = false;

    bool botServed = false;
    HitPointInfo botSwingStyle;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        ResetInputflag();
        if (botPlayer.PrepareServe)
        {
            Serve();
        }
        else
        {
            
            //ball turn around
            if (preBallVel.x * ball.rb.velocity.x <= 0)
            {
                botSwingStyle = RandomChooseSwingStyle();
                Vector3[] track = BallTrackDraw.getTrack(ball.rb, ball.transform.position);

                //if ball fall position is in bot side then move and swing
                if (track[track.Length - 1].x * botPlayer.transform.position.x >= 0)
                {
                    if (BallTrackDraw.isBallReachHeight(ball.rb, ball.transform.position, botSwingStyle.transform.position.y , Time.time, ref trackPointInfos))
                    {
                        botMove = true;
                    }
                }
            }

            if(trackPointInfos.Length > 0)
            {
               
                if (botMove)
                {
                    MoveHitPointTo(trackPointInfos[trackPointInfos.Length - 1].position.x, 0.09f, botSwingStyle);
                    if (botPlayer.moveInputFlag == 0)
                    {
                        botSwing = true;
                        botMove = false;
                    }
                }

                if (botSwing)
                {
                    if (Swing(botSwingStyle))
                        botSwing = false;
                }
                else
                {
                    botPlayer.swinDownInputFlag = false;
                }
            }
        }

        preBallVel = ball.rb.velocity;

    }

    private void MoveHitPointTo(float x, float moveRange, HitPointInfo hitPoint)
    {
        if (hitPoint.transform.position.x > x + moveRange)
        {
            botPlayer.moveInputFlag = -1;
        }
        else if (hitPoint.transform.position.x < x - moveRange)
        {
            botPlayer.moveInputFlag = 1;
        }
        else
        {
            botPlayer.moveInputFlag = 0;
        }
    }

    private bool Swing(HitPointInfo hitPoint)
    {
        float offsetRange = 0.00f;
        float offsetTime = trackPointInfos[trackPointInfos.Length - 1].time - Time.time;
        if (offsetTime <= hitPoint.time + offsetRange)
        {
            botPlayer.swinDownInputFlag = true;
            return true;
        }
        else
        {
            return false;
        }
    }
    private void Serve()
    {
        botPlayer.swinUpInputFlag = true;
    }
    private void ResetInputflag()
    {
        botPlayer.swinUpInputFlag = false;
        botPlayer.swinDownInputFlag = false;
        botPlayer.moveInputFlag = 0;
    }
    private HitPointInfo RandomChooseSwingStyle()
    {
        return UnderHandBack[UnityEngine.Random.Range(0, 2)];
    }
}
