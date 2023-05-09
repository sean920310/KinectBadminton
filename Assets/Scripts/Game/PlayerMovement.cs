using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;
    public Animator animator;
    [SerializeField] Transform LeftHand;
    [SerializeField] AudioSource SwooshSound;

    // Player Parameter
    // Move
    [SerializeField] float movementSpeed = 1.0f;

    // Jump
    [SerializeField] float jumpForce = 5.0f;
    [SerializeField] Transform GroundChk;
    [SerializeField] LayerMask WhatIsGround;
    public bool onGround = true;

    // Swin
    [SerializeField] float hitCoolDown;
    float hitCoolDownCounter = 0;

    [SerializeField] BallManager ball;
    [SerializeField] RacketManager racket;

    public bool PrepareServe = true;
    bool facingRight = false;

    // Input Flag
    public float moveInputFlag = 0.0f;
    public bool jumpInputFlag = false;
    public bool swinUpInputFlag = false;
    public bool swinDownInputFlag = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        if (transform.rotation.y == 0f)
        {
            facingRight = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        hitCoolDownCounter -= Time.deltaTime;

        // Jump
        onGround = Physics.Raycast(GroundChk.position, Vector3.down, 0.05f, WhatIsGround);
        animator.SetBool("OnGround", onGround);

        // Serving Can't Jump
        if (!PrepareServe && jumpInputFlag && onGround)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpInputFlag = false;
        }

        // Serve
        if (PrepareServe)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("ServePrepare"))
            {
                animator.SetTrigger("ServePrepare");
            }
            ball.transform.position = LeftHand.position;
            ball.transform.rotation = LeftHand.rotation;

            if (swinDownInputFlag && hitCoolDownCounter <= 0)
            {
                SwooshSound.Play();
                animator.SetTrigger("Serve");

                StartCoroutine(ball.Serve(0f, facingRight, racket.ServeForce));

                PrepareServe = false;
                swinDownInputFlag = false;

                hitCoolDownReset();
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

            hitCoolDownReset();
        }
        if (swinDownInputFlag && hitCoolDownCounter <= 0)
        {
            swinDownInputFlag = false;

            SwooshSound.Play();
            animator.SetTrigger("SwingDown");
            racket.swinDown();

            hitCoolDownReset();
        }
    }

    private void FixedUpdate()
    {
        // Movement
        float movementX = moveInputFlag;

        if (Mathf.Abs(movementX) > 0f)
            animator.SetBool("Move", true);
        else
            animator.SetBool("Move", false);

        rb.velocity = new Vector3(movementX * movementSpeed, rb.velocity.y, 0);
    }

    // This SetRacketColliderOff is for animation event
    public void SetRacketColliderOff()
    {
        racket.boxColliderDisable();
    }

    void hitCoolDownReset()
    {
        hitCoolDownCounter = hitCoolDown;
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
        if (context.performed)
            jumpInputFlag = true;

        if (context.canceled)
            jumpInputFlag = false;
    }
    public void OnSwinUp(InputAction.CallbackContext context)
    {
        if (context.performed)
            swinUpInputFlag = true;

        if (context.canceled)
            swinUpInputFlag = false;
    }
    public void OnSwinDown(InputAction.CallbackContext context)
    {
        if (context.performed)
            swinDownInputFlag = true;

        if (context.canceled)
            swinDownInputFlag = false;
    }
    #endregion
}
