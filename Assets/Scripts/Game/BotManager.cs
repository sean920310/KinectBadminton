using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BotManager : MonoBehaviour
{
    [SerializeField] PlayerMovement enemyPlayer;
    [SerializeField] PlayerMovement botPlayer;
    [SerializeField] BallManager ball;

    [Header("SwinUp")]
    [SerializeField] Vector2 swinUpRangeDefault;
    [SerializeField] bool swinUpRangeXRandom;
    [DrawIf("swinUpRangeXRandom", true, ComparisonType.Equals)]
    [SerializeField] float swinUpRangeXRandomRange;

    [SerializeField] bool swinUpRangeYRandom;
    [DrawIf("swinUpRangeYRandom", true, ComparisonType.Equals)]
    [SerializeField] float swinUpRangeYRandomRange;

    [SerializeField] bool ballXVelImpactsSwinUpRange;
    [SerializeField] AnimationCurve VelocityXImpactsSwinUpRangeCurve;

    [SerializeField] Vector2 swinUpRange;

    [Header("SwinDown")]
    [SerializeField] Vector2 swinDownRangeDefault;
    [SerializeField] bool swinDownRangeXRandom;
    [DrawIf("swinDownRangeXRandom", true, ComparisonType.Equals)]
    [SerializeField] float swinDownRangeXRandomRange;

    [SerializeField] bool swinDownRangeYRandom;
    [DrawIf("swinDownRangeYRandom", true, ComparisonType.Equals)]
    [SerializeField] float swinDownRangeYRandomRange;

    [SerializeField] bool ballXVelImpactsSwinDownRange;
    [SerializeField] AnimationCurve VelocityXImpactsSwinDownRangeCurve;

    [SerializeField] Vector2 swinDownRange;

    [Header("Jump")]
    [SerializeField] float JumpProbability;
    [SerializeField] float JumpHeightDefault;
    [SerializeField] bool JumpHeightRandom;
    [DrawIf("JumpHeightRandom", true, ComparisonType.Equals)]
    [SerializeField] float JumpHeightRange;
    [SerializeField] float JumpHeight;

    [SerializeField] bool ballXVelImpactsJumpHeight;
    [SerializeField] AnimationCurve VelocityXImpactsJumpHeightCurve;

    [SerializeField] float jumpDelay;
    [ReadOnly] [SerializeField] float jumpDelayCounter = 0;
    bool isJumpLocked = false;

    [Header("Defense Position")]
    [SerializeField] float FrontPositionX;
    [SerializeField] float CenterPositionX;
    [SerializeField] float DefensePositionX;

    [SerializeField] float hitDelay;
    float hitDelayCounter = 0;

    [SerializeField] bool isRightSidePlayer = false;

    RaycastHit DropPointInfo;
    [SerializeField] LayerMask whatIsDropPoint;

    bool newPrepareServe = false;
    bool canJump = false;
    bool isHitAfterServeLocked = false;

    void Start()
    {
        swinUpRange = swinUpRangeDefault;
        swinDownRange = swinDownRangeDefault;
        JumpHeight = JumpHeightDefault;
    }
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
            // The bot is not allowed to hit the ball until it crosses over to the opponent's side of the court after the serve.
            if (isHitAfterServeLocked)
            {
                if (isRightSidePlayer && ball.BallInLeftSide || !isRightSidePlayer && !ball.BallInLeftSide)
                {
                    isHitAfterServeLocked = false;
                }
            }

            // Ball is in enemy side
            if (isRightSidePlayer && ball.BallInLeftSide ||
                !isRightSidePlayer && !ball.BallInLeftSide)
            {
                BallInEnemySideMovement();
                Jump();
                if (!isHitAfterServeLocked)
                    HitTheBall();
            }
            else // Ball is in bot side: find ball
            {
                Movement();
                Jump();

                if (!isHitAfterServeLocked)
                    HitTheBall();
            }
        }
    }

    private void Serve()
    {
        if (newPrepareServe == false)
        {
            newPrepareServe = true;
            isHitAfterServeLocked = true;
            StartCoroutine(ServeCoroutine(Random.Range(0.5f, 1.5f)));
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
            else if (Mathf.Abs(enemyPlayer.transform.position.x) <= 3)
            {
                MoveBotTo(DefensePositionX, 0.01f);
            }
            else if ((3.5 >= ball.rb.velocity.x && ball.rb.velocity.x > 0))
            {
                MoveBotTo(FrontPositionX, 0.01f);
            }
            else
            {
                if (enemyPlayer.onGround)
                {
                    if (ball.rb.velocity.magnitude <= 18f)
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
            else if (Mathf.Abs(enemyPlayer.transform.position.x) <= 3)
            {
                MoveBotTo(DefensePositionX, 0.01f);
            }
            else if ((-3.5 >= ball.rb.velocity.x && ball.rb.velocity.x > 0))
            {
                MoveBotTo(-FrontPositionX, 0.01f);
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
    private void Movement()
    {
        // Smash Liner
        if (!isBallFlyingToYou() && Mathf.Abs(enemyPlayer.transform.position.x) <= 5f && Mathf.Abs(ball.rb.velocity.x) >= 9f)
        {
            MoveBotTo(DropPointInfo.point.x, 0.01f);
        }
        if (ball.ballStates == BallManager.BallStates.Smash && Mathf.Abs(ball.rb.velocity.y) < 1f)
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
                    if (ball.rb.velocity.x > 10f)
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
                    if (ball.rb.velocity.x < -10f)
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
    private void Jump()
    {
        if (!canJump && ball.transform.position.y >= JumpHeight)
        {
            canJump = true;
            if (Random.Range(0f, 1f) <= JumpProbability)
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
            if (ball.rb.velocity.y < 0 && jumpDelayCounter <= 0f && canJump && 
                Mathf.Abs(ball.transform.position.x - botPlayer.transform.position.x) <= 0.5f)
            {
                setRandomValue();
                jumpDelayReset();

                if (!isJumpLocked)
                    botPlayer.jumpInputFlag = true;

                isJumpLocked = false;
                canJump = false;
            }
        }
    }
    private void HitTheBall()
    {
        if (canSwin())
        {
            // SwinDown
            if (ball.transform.position.y - botPlayer.transform.position.y <= swinDownRange.y &&
                Mathf.Abs(ball.transform.position.x - botPlayer.transform.position.x) <= swinDownRange.x)
            {
                hitDelayReset();
                setRandomValue();
                botPlayer.swinDownInputFlag = true;
            }
            else if (ball.transform.position.y - botPlayer.transform.position.y <= swinUpRange.y &&
                Mathf.Abs(ball.transform.position.x - botPlayer.transform.position.x) <= swinUpRange.x)
            {
                hitDelayReset();
                setRandomValue();
                botPlayer.swinUpInputFlag = true;
            }
        }
    }
    private void MoveBotTo(float x, float stopRange)
    {
        if (botPlayer.transform.position.x > x + stopRange)
        {
            botPlayer.moveInputFlag = -1;
        }
        else if (botPlayer.transform.position.x < x - stopRange)
        {
            botPlayer.moveInputFlag = 1;
        }
        else
        {
            botPlayer.moveInputFlag = 0;
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = new Color(1.0f, 0.0f, 0.0f);
    //    Gizmos.DrawCube(new Vector3(DropPointInfo.point.x, DropPointInfo.point.y, 0), new Vector3(0.05f, 0.05f, 0.05f));
    //    Gizmos.color = new Color(0.0f, 1.0f, 0.0f);
    //    Gizmos.DrawLine(ball.transform.position, ball.transform.position + ball.transform.right.normalized * 10);
    //}

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
    bool isBallFlyingToYou()
    {
        return isRightSidePlayer ? (ball.rb.velocity.x > 0) : (ball.rb.velocity.x < 0);
    }

    void setRandomValue()
    {
        if (swinUpRangeYRandom)
        {
            if (Random.Range(-swinUpRangeYRandomRange, swinUpRangeYRandomRange) > 0f)
                swinUpRange.y += swinUpRangeYRandomRange;
            else
                swinUpRange.y -= swinUpRangeYRandomRange;

            if(swinUpRange.y < 0)
                swinUpRange.y = swinUpRangeYRandomRange;
        }
        if (swinUpRangeXRandom)
        {
            if (Random.Range(-swinUpRangeXRandomRange, swinUpRangeXRandomRange) > 0f)
                swinUpRange.x += swinUpRangeXRandomRange;
            else
                swinUpRange.x -= swinUpRangeXRandomRange;
            if (swinUpRange.x < 0)
                swinUpRange.x = swinUpRangeXRandomRange;
        }

        if (swinDownRangeYRandom)
        {
            if (Random.Range(-swinDownRangeYRandomRange, swinDownRangeYRandomRange) > 0f)
                swinDownRange.y += swinDownRangeYRandomRange;
            else
                swinDownRange.y -= swinDownRangeYRandomRange;
            if (swinDownRange.y < 0)
                swinDownRange.y = swinDownRangeYRandomRange;
        }
        if (swinDownRangeXRandom)
        {
            if (Random.Range(-swinDownRangeXRandomRange, swinDownRangeXRandomRange) > 0f)
                swinDownRange.x += swinDownRangeXRandomRange;
            else
                swinDownRange.x -= swinDownRangeXRandomRange;
            if (swinDownRange.x < 0)
                swinDownRange.x = swinDownRangeXRandomRange;
        }

        if (JumpHeightRandom)
        {
            if (Random.Range(-JumpHeightRange, JumpHeightRange) > 0f)
                JumpHeight += JumpHeightRange;
            else
                JumpHeight -= JumpHeightRange;
            if (JumpHeight < 0)
                JumpHeight = JumpHeightDefault;
        }
    }

    IEnumerator ServeCoroutine(float delay)
    {
        float destinationX;

        if (isRightSidePlayer)
        {
            destinationX = Random.Range(3f, 5f);
            while (!(destinationX + 0.1f >= botPlayer.transform.position.x && botPlayer.transform.position.x >= destinationX - 0.1f))
            {
                MoveBotTo(destinationX, 0.0f);
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            destinationX = Random.Range(-5f, -3f);
            while (!(destinationX + 0.1f >= botPlayer.transform.position.x && botPlayer.transform.position.x >= destinationX - 0.1f))
            {
                MoveBotTo(destinationX, 0.0f);
                yield return new WaitForFixedUpdate();
            }
        }

        MoveBotTo(destinationX, 0.2f);

        yield return new WaitForSeconds(delay);

        botPlayer.swinDownInputFlag = true;
        hitDelayCounter = hitDelay * 2.5f;

        newPrepareServe = false;
    }
}
