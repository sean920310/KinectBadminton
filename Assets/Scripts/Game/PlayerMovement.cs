using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class PlayerMovement : MonoBehaviour
{
    [ReadOnly] public Rigidbody rb;
    [ReadOnly] public Animator animator;
    [SerializeField] Transform LeftHand;
    [SerializeField] AudioSource SwooshSound;

    // Player Parameter
    // Move
    [Header("Movement")]
    [SerializeField] float movementSpeed = 4.9f;
    [SerializeField] float airMovementSpeed = 2.4f;

    // Jump
    [Header("Jump")]
    [SerializeField] float jumpForce = 5.0f;
    [SerializeField] Transform GroundChk;
    [SerializeField] LayerMask WhatIsGround;
    [ReadOnly] public bool onGround = true;
    [SerializeField] float normalGravityScale = 1.0f;
    public bool enableFallGravityScale;
    [DrawIf("enableFallGravityScale", true, ComparisonType.Equals)]
    [SerializeField] float fallGravityScale = 1.5f;


    // Swin
    [Header("Swin")]
    [SerializeField] float hitCoolDown;
    float hitCoolDownCounter = 0;

    [SerializeField] BallManager ball;
    [SerializeField] RacketManager racket;

    [SerializeField] public bool PrepareServe { get; private set; } = false;
    bool facingRight = false;

    // Input Flag
    [ReadOnly] public float moveInputFlag = 0.0f;
    [ReadOnly] public bool jumpInputFlag = false;
    [ReadOnly] public bool swinUpInputFlag = false;
    [ReadOnly] public bool swinDownInputFlag = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        if (enableFallGravityScale)
        {
            rb.useGravity = false;
        }
        facingRight = (transform.rotation.y == 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(GroundChk.position, GroundChk.position + Vector3.down * 0.02f);
    }

    // Update is called once per frame
    void Update()
    {
        hitCoolDownCounter -= Time.deltaTime;

        // Check On Ground
        onGround = Physics.Raycast(GroundChk.position, Vector3.down, 0.02f, WhatIsGround);
        animator.SetBool("OnGround", onGround);

        // Swoop
        //if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Swoop"))
        //{
        //    if (!PrepareServe && onGround)
        //    {
        //        if (facingRight && Input.GetKeyDown(KeyCode.Z))
        //        {
        //            racket.swoop();
        //            animator.SetTrigger("Swoop");
        //            racket.boxColliderEnable();
        //            rb.velocity = Vector3.zero;
        //            rb.AddForce(new Vector3(1f, 0.05f, 0f).normalized * 11f, ForceMode.Impulse);

        //        }
        //        else if(!facingRight && Input.GetKeyDown(KeyCode.M))
        //        {
        //            racket.swoop();
        //            animator.SetTrigger("Swoop");
        //            racket.boxColliderEnable();
        //            rb.velocity = Vector3.zero;
        //            rb.AddForce(new Vector3(-1f, 0.05f, 0f).normalized * 11f, ForceMode.Impulse);

        //        }
        //    }
        //}
        //else
        //{
        //    return;
        //}

        // Jump
        // Serving Can't Jump
        if (!PrepareServe && jumpInputFlag && onGround)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpInputFlag = false;
        }

        // Serve
        if (PrepareServe)
        {
            // Set Serve Animation
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("ServePrepare"))
            {
                animator.SetBool("ServePrepare", true);
            }

            ball.transform.position = LeftHand.position;
            ball.transform.rotation = LeftHand.rotation;
            ball.rb.velocity = Vector3.zero;

            if (swinUpInputFlag && hitCoolDownCounter <= 0)
            {
                racket.boxColliderDisable();

                SwooshSound.Play();
                animator.SetTrigger("LongServe");

                ball.Serve(facingRight, racket.LongServeDirection, racket.LongServeForce);

                PrepareServe = false;
                swinUpInputFlag = false;
                animator.SetBool("ServePrepare", false);
            }
            else if (swinDownInputFlag && hitCoolDownCounter <= 0)
            {
                racket.boxColliderDisable();

                SwooshSound.Play();
                animator.SetTrigger("ShortServe");

                ball.Serve(facingRight, racket.ShortServeDirection, racket.ShortServeForce);

                PrepareServe = false;
                swinUpInputFlag = false;
                animator.SetBool("ServePrepare", false);
            }
            return;
        }
        
        // Swing
        if (swinUpInputFlag && hitCoolDownCounter <= 0)
        {
            swinUpInputFlag = false;

            SwooshSound.Play();
            animator.SetTrigger("SwingUp");
            racket.swinUp();
        }
        if (swinDownInputFlag && hitCoolDownCounter <= 0)
        {
            swinDownInputFlag = false;
            SwooshSound.Play();

            // SwinDown Type Detection: Front Ball SwingDownFront, vice versa.
            if (facingRight)
            {
                if (ball.transform.position.x - transform.position.x <= 0.3f)
                {
                    animator.SetTrigger("SwingDownBack");
                }
                else
                {
                    animator.SetTrigger("SwingDownFront");
                }
            }
            else
            {
                if (ball.transform.position.x - transform.position.x >= 0.3f)
                {
                    animator.SetTrigger("SwingDownBack");
                }
                else
                {
                    animator.SetTrigger("SwingDownFront");
                }
            }
            racket.swinDown();
        }
    }

    private void FixedUpdate()
    {
        //if (animator.GetCurrentAnimatorStateInfo(0).IsName("Swoop"))
        //    return;


        // Fall Gravity
        if(enableFallGravityScale)
        {
            Vector3 gravity = gravity = -9.81f * normalGravityScale * Vector3.up;
            if(!onGround && rb.velocity.y <= 0)
            {
                gravity = -9.81f * fallGravityScale * Vector3.up;
            }
            rb.AddForce(gravity, ForceMode.Acceleration);
        }

        // Movement
        float movementX = moveInputFlag;

        if (Mathf.Abs(movementX) > 0f)
            animator.SetBool("Move", true);
        else
            animator.SetBool("Move", false);


        if(onGround)
            rb.velocity = new Vector3(movementX * movementSpeed, rb.velocity.y, 0);
        else
            rb.velocity = new Vector3(movementX * airMovementSpeed, rb.velocity.y, 0);
    }

    // This SetRacketColliderOff is for animation event
    public void SetRacketColliderOff()
    {
        racket.boxColliderDisable();
    }

    public void SetRacketTrailOn()
    {
        racket.setTrailOn();
    }

    public void SetRacketTrailOff()
    {
        racket.setTrailOff();
    }

    public void SetPlayerServe()
    {
        PrepareServe = true;
    }

    void hitCoolDownReset()
    {
        hitCoolDownCounter = hitCoolDown;
    }
    public void setHitCoolDown(float value)
    {
        hitCoolDownCounter = value;
    }
    public void ResetSwinInputFlag()
    {
        swinUpInputFlag = false;
        swinDownInputFlag = false;
    }
    public void ResetInputFlag()
    {
        moveInputFlag = 0.0f;
        jumpInputFlag = false;
        swinUpInputFlag = false;
        swinDownInputFlag = false;
    }

    #region Input Handler
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
            moveInputFlag = context.ReadValue<float>();

        if (context.canceled)
            moveInputFlag = 0f;
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
            jumpInputFlag = true;

        if (context.canceled)
            jumpInputFlag = false;
    }
    public void OnSwinUp(InputAction.CallbackContext context)
    {
        if (context.started)
            swinUpInputFlag = true;

        if (context.canceled)
            swinUpInputFlag = false;
    }
    public void OnSwinDown(InputAction.CallbackContext context)
    {
        if (context.started)
            swinDownInputFlag = true;

        if (context.canceled)
            swinDownInputFlag = false;
    }
    public void OnKinectSwinDown()
    {
        swinDownInputFlag = true;
    }
    public void OnKinectSwinUp()
    {
        swinUpInputFlag = true;
    }
    public void OnKinectPositionMapping(double pos)
    {
        transform.position = new Vector3 ((float)pos, transform.position.y, transform.position.z);
    }

    public void OnKinectJump()
    {
        jumpInputFlag = true;
    }
    #endregion
}
