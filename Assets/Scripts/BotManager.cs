using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BotManager : MonoBehaviour
{
    [SerializeField] PlayerMovement enemyPlayer;
    [SerializeField] PlayerMovement botPlayer;
    [SerializeField] BallManager ball;
    [SerializeField] Transform centerBorder;

    [SerializeField] bool swinUpHeightRandom;
    [SerializeField] float swinUpHeight;
    [SerializeField] float swinUpHeightRange;

    [SerializeField] bool swinDownHeightRandom;
    [SerializeField] float swinDownHeight;
    [SerializeField] float swinDownHeightRange;

    [SerializeField] bool SmashHeightRandom;
    [SerializeField] float SmashHeight;
    [SerializeField] float SmashHeightRange;
    [SerializeField] float SmashProbability;
    [SerializeField] float hitDelay;
    bool newPrepareServe = false;
    bool canJump = false;

    float hitDelayCounter = 0;

    [SerializeField] bool isRightSidePlayer = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        hitDelayCounter -= Time.deltaTime;

        if (botPlayer.PrepareServe)
        {
            if(newPrepareServe == false)
            {
                StartCoroutine(ServeCoroutine(Random.Range(1f,2f)));
                newPrepareServe = true;
            }
        }
        else
        {
            newPrepareServe = false;

            // Ball is in enemy side
            if (isRightSidePlayer && ball.transform.position.x < centerBorder.position.x ||
                !isRightSidePlayer && ball.transform.position.x > centerBorder.position.x)
            {
                BallInEnemySideMovement();
            }
            else
            // Ball is in bot side: find ball
            {
                // Movement
                if(ball.isSmashBall && Mathf.Abs(ball.body.velocity.y) < 1f &&
                    (isRightSidePlayer && ball.body.velocity.x > 3f ||
                    !isRightSidePlayer && ball.body.velocity.x > -3f)
                    )
                {
                    if(isRightSidePlayer)
                    {
                        if (ball.transform.position.x < botPlayer.transform.position.x)
                        {
                            MoveBotTo(4.5f, 0.1f);
                        }
                        else
                        {
                            MoveBotTo(ball.transform.position.x - 0.2f, 0.1f);
                        }
                    }
                    else
                    {
                        if (ball.transform.position.x > botPlayer.transform.position.x)
                        {
                            MoveBotTo(-4.5f, 0.1f);
                        }
                        else
                        {
                            MoveBotTo(ball.transform.position.x + 0.2f, 0.1f);
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

                // Jump
                if (!canJump && ball.transform.position.y >= SmashHeight)
                {

                    if (Random.Range(0f, 1f) <= SmashProbability)
                    {
                        canJump = true;
                    }
                }
                else
                {
                    if (canJump && Mathf.Abs(ball.transform.position.x - botPlayer.transform.position.x) <= 0.6f)
                    {
                        setRandomValue();
                        botPlayer.jump = true;
                        canJump = false;
                    }
                }

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
        }
    }

    private void BallInEnemySideMovement()
    {
        if (isRightSidePlayer && (1.5 >= ball.body.velocity.x && ball.body.velocity.x > 0) ||
            !isRightSidePlayer && (-1.5 <= ball.body.velocity.x && ball.body.velocity.x < 0))
        {
            if(isRightSidePlayer)
                MoveBotTo(2.7f, 0.1f);
            else
                MoveBotTo(-2.7f, 0.1f);
        }
        else
        {

            if (isRightSidePlayer)
            {
                if (enemyPlayer.onGround)
                {
                    MoveBotTo(3f, 0.1f); // back to court center
                }
                else
                {
                    // Player Smash Predict
                    MoveBotTo(4.5f, 0.1f);
                }
            }
            else
            {
                if (enemyPlayer.onGround)
                {
                    MoveBotTo(-3f, 0.1f); // back to court center
                }
                else
                {
                    // Player Smash Predict
                    MoveBotTo(-4.5f, 0.1f);
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
        hitDelayCounter = hitDelay * 2;
    }
}
