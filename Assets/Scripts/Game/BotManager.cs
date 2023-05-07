using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*

    jump height: 2.7
    jump and swin up height: 4.5
    Ground swin up height: 2.8
    Ground swin down height: 1 ~ 0.4
 
*/


public class BotManager : MonoBehaviour
{
    [SerializeField] PlayerMovement enemyPlayer;
    [SerializeField] PlayerMovement botPlayer;
    [SerializeField] BallManager ball;

    [SerializeField] bool swinUpHeightRandom;
    [SerializeField] float swinUpHeightDefault;
    [SerializeField] float swinUpHeightRange;
    [SerializeField] float swinUpHeight;

    [SerializeField] bool swinDownHeightRandom;
    [SerializeField] float swinDownHeightDefault;
    [SerializeField] float swinDownHeightRange;
    [SerializeField] float swinDownHeight;

    [SerializeField] float SmashProbability;
    [SerializeField] bool SmashHeightRandom;
    [SerializeField] float SmashHeightDefault;
    [SerializeField] float SmashHeightRange;
    [SerializeField] float SmashHeight;

    bool isJumpLocked = false;
    [SerializeField] float jumpDelay;
    float jumpDelayCounter = 0;

    [SerializeField] float DefensePositionX;
    [SerializeField] float CenterPositionX;

    [SerializeField] float hitDelay;
    float hitDelayCounter = 0;

    [SerializeField] bool isRightSidePlayer = false;

    bool newPrepareServe = false;
    bool canJump = false;
    bool isHitAfterServeLocked = false;

    RaycastHit DropPointInfo;
    [SerializeField] LayerMask whatIsDropPoint;

    void Start()
    {
        swinUpHeight = swinUpHeightDefault;
        swinDownHeight = swinDownHeightDefault;
        SmashHeight = SmashHeightDefault;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f);
        Gizmos.DrawCube(new Vector3(DropPointInfo.point.x, DropPointInfo.point.y, 0), new Vector3(0.05f, 0.05f, 0.05f));
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f);
        Gizmos.DrawLine(ball.transform.position, ball.transform.position + ball.transform.right.normalized * 10);
    }

    // Update is called once per frame
    void Update()
    {
        // Drop Point Compute
        Physics.Raycast(ball.transform.position, ball.transform.right, out DropPointInfo, 100, whatIsDropPoint);

        hitDelayCounter -= Time.deltaTime;
        jumpDelayCounter -= Time.deltaTime;

        if (botPlayer.PrepareServe)
        {
            Serve();
        }
        else
        {
            if(isHitAfterServeLocked)
            {
                if (isRightSidePlayer && ball.BallInLeftSide || !isRightSidePlayer && !ball.BallInLeftSide)
                {
                    isHitAfterServeLocked = false;
                }
            }

            newPrepareServe = false;

            // Ball is in enemy side
            if (isRightSidePlayer && ball.BallInLeftSide ||
                !isRightSidePlayer && !ball.BallInLeftSide)
            {
                BallInEnemySideMovement();
            }
            else
            // Ball is in bot side: find ball
            {
                Movement();
                Jump();

                if(!isHitAfterServeLocked)
                    HitTheBall();
            }
        }
    }

    private void HitTheBall()
    {
        if (canSwin())
        {
            // SwinDown
            if (ball.transform.position.y - botPlayer.transform.position.y <= swinDownHeight &&
                Mathf.Abs(ball.transform.position.x - botPlayer.transform.position.x) <= 0.6f)
            {
                hitDelayReset();
                setRandomValue();
                botPlayer.swinDown = true;
            }

            // SwinUp
            if (ball.transform.position.y - botPlayer.transform.position.y <= swinUpHeight &&
                Mathf.Abs(ball.transform.position.x - botPlayer.transform.position.x) <= 0.6f)
            {
                hitDelayReset();
                setRandomValue();
                botPlayer.swinUp = true;
            }
        }
    }

    private void Jump()
    {
        if (!canJump && ball.transform.position.y >= SmashHeight)
        {
            canJump = true;
            if (Random.Range(0f, 1f) <= SmashProbability)
            {
                isJumpLocked = false;
            }
            else
            {
                isJumpLocked = true;
            }
        }
        else
        {
            if (jumpDelayCounter <= 0f && canJump && Mathf.Abs(ball.transform.position.x - botPlayer.transform.position.x) <= 0.6f)
            {
                setRandomValue();
                jumpDelayReset();

                if(!isJumpLocked)
                    botPlayer.jump = true;

                isJumpLocked = false;
                canJump = false;
            }
        }
    }

    private void Movement()
    {
        // Smash Liner

        if (ball.isSmashBall && Mathf.Abs(ball.body.velocity.y) < 1f)
        {
            if (3.5f > DropPointInfo.point.y && DropPointInfo.point.y > 0)
            {
                MoveBotTo(DropPointInfo.point.x, 0.01f);
            }
            else
            {
                if (isRightSidePlayer)
                {
                    // Fast Liner
                    if (ball.body.velocity.x > 10f)
                    {
                        if (ball.transform.position.x < botPlayer.transform.position.x)
                        {
                            MoveBotTo(5f, 0.1f);
                        }
                        else
                        {
                            MoveBotTo(ball.transform.position.x - 0.2f, 0.1f);
                        }
                    }
                    else
                    {
                        MoveBotTo(ball.transform.position.x - 0.2f, 0.1f);
                    }
                }
                else
                {
                    // Fast Liner
                    if (ball.body.velocity.x < -10f)
                    {
                        if (ball.transform.position.x > botPlayer.transform.position.x)
                        {
                            MoveBotTo(-5f, 0.1f);
                        }
                        else
                        {
                            MoveBotTo(ball.transform.position.x + 0.2f, 0.1f);
                        }
                    }
                    else
                    {
                        MoveBotTo(ball.transform.position.x + 0.2f, 0.1f);
                    }
                }
            }
        }
        else
        {
            if (isRightSidePlayer)
                MoveBotTo(ball.transform.position.x - 0.2f, 0.1f);
            else
                MoveBotTo(ball.transform.position.x + 0.2f, 0.1f);
        }
    }

    private void Serve()
    {
        if (newPrepareServe == false)
        {
            StartCoroutine(ServeCoroutine(Random.Range(0.5f, 1.5f)));
            newPrepareServe = true;
            isHitAfterServeLocked = true;
        }
    }

    private void BallInEnemySideMovement()
    {
        if (isRightSidePlayer)
        {
            if (isBallFlyingToYou())
            {
                if (DropPointInfo.collider != null)
                {
                    MoveBotTo(DropPointInfo.point.x, 0.01f);
                }
                else
                {
                    MoveBotTo(ball.transform.position.x, 0.01f);
                }
            }
            else if((3.5 >= ball.body.velocity.x && ball.body.velocity.x > 0))
            {
                MoveBotTo(2.6f, 0.1f);
            }
            else
            {
                if (enemyPlayer.onGround)
                {
                    if (ball.body.velocity.magnitude <= 18f)
                    {
                        // back to court center
                        MoveBotTo(CenterPositionX, 0.1f);
                    }
                    else
                    {
                        MoveBotTo(DefensePositionX, 0.1f);
                    }

                }
                else
                {
                    // Player Smash Predict
                    MoveBotTo(DefensePositionX, 0.1f);
                }
            }
        }
        else
        {

            if (isBallFlyingToYou())
            {
                if (DropPointInfo.collider != null)
                {
                    MoveBotTo(DropPointInfo.point.x, 0.01f);
                }
                else
                {
                    MoveBotTo(ball.transform.position.x, 0.01f);
                }
            }
            else if ((-3.5 >= ball.body.velocity.x && ball.body.velocity.x > 0))
            {
                MoveBotTo(-2.6f, 0.1f);
            }
            else
            {
                if (enemyPlayer.onGround)
                {
                    // back to court center
                    MoveBotTo(-CenterPositionX, 0.1f);
                }
                else
                {
                    // Player Smash Predict
                    MoveBotTo(-DefensePositionX, 0.1f);
                }
            }
        }
    }

    private void MoveBotTo(float x, float stopRange)
    {
        if (botPlayer.transform.position.x > x + stopRange)
        {
            botPlayer.move = -1;
        }
        else if (botPlayer.transform.position.x < x - stopRange)
        {
            botPlayer.move = 1;
        }
        else
        {
            botPlayer.move = 0;
        }
    }

    void hitDelayReset()
    {
        hitDelayCounter = hitDelay;
    }
    void jumpDelayReset()
    {
        jumpDelayCounter = jumpDelay;
    }

    bool canSwin()
    {
        return hitDelayCounter <= 0.0f;
    }
    void setRandomValue()
    {
        if (swinUpHeightRandom)
        {
            if(Random.Range(-swinUpHeightRange, swinUpHeightRange) > 0f)
                swinUpHeight += swinUpHeightRange;
            else
                swinUpHeight -= swinUpHeightRange;

        }
        if(swinDownHeightRandom)
        {
            if (Random.Range(-swinDownHeightRange, swinDownHeightRange) > 0f)
                swinDownHeight += swinDownHeightRange;
            else
                swinDownHeight -= swinDownHeightRange;
        }
        if(SmashHeightRandom)
        {
            if (Random.Range(-SmashHeightRange, SmashHeightRange) > 0f)
                SmashHeight += SmashHeightRange;
            else
                SmashHeight -= SmashHeightRange;
        }
    }

    bool isBallFlyingToYou()
    {
        return isRightSidePlayer ? (ball.body.velocity.x > 0) : (ball.body.velocity.x < 0);
    }

    IEnumerator ServeCoroutine(float delay)
    {
        float destinationX;

        if (isRightSidePlayer)
        {
            destinationX = Random.Range(3f, 5f);
            while (!(destinationX + 0.2f >= botPlayer.transform.position.x && botPlayer.transform.position.x >= destinationX - 0.2f))
            {
                MoveBotTo(destinationX, 0.0f);
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            destinationX = Random.Range(-5f, -3f);
            while (!(destinationX + 0.2f >= botPlayer.transform.position.x && botPlayer.transform.position.x >= destinationX - 0.2f))
            {
                MoveBotTo(destinationX, 0.0f);
                yield return new WaitForFixedUpdate();
            }
        }

        MoveBotTo(destinationX, 0.2f);

        yield return new WaitForSeconds(delay);

        botPlayer.swinDown = true;
        hitDelayCounter = hitDelay * 2.5f;
    }
}
