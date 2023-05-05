using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;
    Animator animator;
    [SerializeField] AudioSource SwooshSound;

    [SerializeField] float speed = 1.0f;

    [SerializeField] float jumpForce = 5.0f;
    [SerializeField] Transform GroundChk;
    [SerializeField] LayerMask WhatIsGround;

    [SerializeField] GameObject ball;
    [SerializeField] Transform LeftHand;

    [SerializeField] RacketManager racket;

    bool onGround = true;
    public bool PrepareServe = true;

    float move = 0.0f;
    bool jump = false;
    bool swinUp = false;
    bool swinDown = false;

    bool facingRight = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        if(transform.rotation.y == 0f)
        {
            facingRight = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

        // Jump
        if(Physics.Raycast(GroundChk.position, Vector3.down, 0.1f, WhatIsGround))
        {
            onGround = true;
        }
        else
        {
            onGround = false;
        }

        if (jump && onGround)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jump = false;
        }

        // Serve
        if (PrepareServe)
        {
            if(!animator.GetCurrentAnimatorStateInfo(0).IsName("ServePrepare"))
            {
                animator.SetTrigger("ServePrepare");
            }
            ball.transform.position = LeftHand.position;
            ball.transform.rotation = LeftHand.rotation;

            if (swinDown)
            {
                animator.SetTrigger("Serve");

                StartCoroutine(ball.GetComponent<BallManager>().Serve(0f, facingRight));
                
                PrepareServe = false;
                swinDown = false;
            }
            return;
        }

        // Swing
        if (swinUp)
        {
            SwooshSound.Play();
            animator.SetTrigger("SwingUp");
            racket.swinUp();
            swinUp = false;
        }
        if (swinDown)
        {
            SwooshSound.Play();
            animator.SetTrigger("SwingDown");
            racket.swinDown();
            swinDown = false;
        }
    }

    private void FixedUpdate()
    {
        float movementX = move;

        transform.position = transform.position + Vector3.right * movementX * speed;
    }

    public void SetRacketColliderOn()
    {
        racket.boxColliderEnable();
    }
    public void SetRacketColliderOff()
    {
        racket.boxColliderDisable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if(context.performed)
            move = context.ReadValue<float>();

        if(context.canceled)
            move = 0f;
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            jump = true;

        if (context.canceled)
            jump = false;
    }
    public void OnSwinUp(InputAction.CallbackContext context)
    {
        if (context.performed)
            swinUp = true;

        if (context.canceled)
            swinUp = false;
    }
    public void OnSwinDown(InputAction.CallbackContext context)
    {
        if (context.performed)
            swinDown = true;

        if (context.canceled)
            swinDown = false;
    }
}
