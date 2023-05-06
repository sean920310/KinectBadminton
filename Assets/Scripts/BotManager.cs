using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class BotManager : MonoBehaviour
{
    [SerializeField] PlayerMovement player;
    [SerializeField] PlayerMovement botPlayer;
    [SerializeField] BallManager ball;
    [SerializeField] Transform centerBorder;

    [SerializeField] float swinupHeight;
    [SerializeField] float SmashHeight;
    [SerializeField] float hitDelay;
    bool newPrepareServe = false;
    bool canJump = false;

    float hitDelayCounter = 0;

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

            // Ball is in player side: back to court center
            if (ball.transform.position.x < centerBorder.position.x)
            {
                MoveBotTo(3f, 0.1f); // back to court center
            }
            else
            // Ball is in bot side: find ball
            {
                // Movement
                MoveBotTo(ball.transform.position.x, 0.3f);

                if(ball.transform.position.y > SmashHeight)
                {
                    canJump = true;
                }
                else
                {
                    if (canJump)
                    {
                        botPlayer.jump = true;
                        canJump = false;
                    }
                }

                // SwinUp
                if (ball.transform.position.y - botPlayer.transform.position.y <= swinupHeight && 
                    Mathf.Abs(ball.transform.position.x - botPlayer.transform.position.x) <= 0.6f && 
                    hitDelayCounter <= 0.0f)
                {
                    hitDelayReset();
                    botPlayer.swinUp = true;
                }

                // SwinUp
                if (ball.transform.position.y - botPlayer.transform.position.y <= 0 &&
                    Mathf.Abs(ball.transform.position.x - botPlayer.transform.position.x) <= 0.6f &&
                    hitDelayCounter <= 0.0f)
                {
                    hitDelayReset();
                    botPlayer.swinDown = true;
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
    IEnumerator ServeCoroutine(float delay)
    {

        float destinationX = Random.Range(3f,5f);

        while (!(destinationX + 0.2f >= botPlayer.transform.position.x && botPlayer.transform.position.x >= destinationX - 0.2f))
        {
            MoveBotTo(destinationX, 0.0f);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(delay);

        botPlayer.swinDown = true;
        hitDelayCounter = hitDelay * 2;
    }
}
