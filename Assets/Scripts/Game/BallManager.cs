using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BallManager : MonoBehaviour
{
    public enum BallStates
    {
        Serving,
        NormalHit,
        Smash,
        Defense,
        TouchFloor
    }

    public Rigidbody rb;

    [SerializeField] StatesPanel p1StatesPanel;
    [SerializeField] StatesPanel p2StatesPanel;

    [SerializeField] Transform centerBorder;

    [SerializeField] bool badmintonSimulation = false;
    [SerializeField] float velocityDecayMultiplier = 0.99f;
    [SerializeField] float FlyDownVelocityDecayMultiplier = 0.99f;
    [SerializeField] float FlyDownVelocityXDecayMultiplier = 0.99f;

    // Visual Effect
    [SerializeField] ParticleSystem HitParticle;
    [SerializeField] ParticleSystem PowerHitParticle;

    [SerializeField] VisualEffect DefenseVFX;

    TrailRenderer trailRenderer;
    [SerializeField] Color SmashTrailColor;
    [SerializeField] Color DefenseTrailColor;
    [SerializeField] Color NormalTrailColor;

    // Audio
    [SerializeField] AudioSource HitSound;
    [SerializeField] AudioSource SmashSound;
    [SerializeField] AudioSource HittingFloorSound;

    bool isFlyingUp = false;

    // Serve State will set to true in GameManager when some one is about to serve.
    public BallStates ballStates = BallStates.Serving;

    public bool BallInLeftSide { get; private set; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();

        Physics.IgnoreLayerCollision(7, 8, true); // Border
        Physics.IgnoreLayerCollision(7, 9, true); // Player
    }

    private void FixedUpdate()
    {
        if (badmintonSimulation && isFlyingUp)
        {
            rb.velocity *= velocityDecayMultiplier;
        }
    }

    private void Update()
    {
        BallInLeftSide = (transform.position.x < centerBorder.transform.position.x);

        if (badmintonSimulation)
        {
            if (rb.velocity.y > 0)
            {
                isFlyingUp = true;
            }
            else
            {
                // To simulate badminton physics, reduce the velocity of the shuttle when it transitions from flying upwards to downwards.
                if (isFlyingUp)
                {
                    // Reduce the velocity of the shuttle especially velocity x.
                    rb.velocity = new Vector3(rb.velocity.x * FlyDownVelocityXDecayMultiplier, rb.velocity.y, rb.velocity.z) * FlyDownVelocityDecayMultiplier;
                }
                isFlyingUp = false;
            }
        }

        transform.rotation = Quaternion.Euler(0, 0, Quaternion.FromToRotation(Vector3.right, rb.velocity).eulerAngles.z);

    }
    public void Serve(bool faceRight, Vector2 ServeDirection, float ServeForce)
    {

        ballStates = BallStates.NormalHit;

        rb.velocity = Vector3.zero;

        if (faceRight)
        {
            ServeDirection = new Vector2(Mathf.Abs(ServeDirection.x), Mathf.Abs(ServeDirection.y));
            rb.AddForce( ServeDirection.normalized * ServeForce, ForceMode.Impulse);
        }
        else
        {
            ServeDirection = new Vector2(-Mathf.Abs(ServeDirection.x), Mathf.Abs(ServeDirection.y));
            rb.AddForce( ServeDirection.normalized * ServeForce, ForceMode.Impulse);

        }


        trailRenderer.enabled = true;
        HitParticle.Play();

        HitSound.Play();

    }

    // Hit Racket
    private void OnTriggerEnter(Collider other)
    {
        RacketManager racketManager = other.transform.GetComponent<RacketManager>();
        if (racketManager != null)
        {
            rb.velocity = Vector3.zero;

            if (racketManager.isSwinDown)
            {

                if (racketManager.transform.root.name == "Player1")
                    GameManager.instance.Player1Info.underhandCount++;
                if (racketManager.transform.root.name == "Player2")
                    GameManager.instance.Player2Info.underhandCount++;

                // If the previous state was a smash, then this hit should be a defensive shot.
                if (ballStates == BallStates.Smash)
                {
                    ballStates = BallStates.Defense;

                    rb.AddForce(racketManager.transform.up.normalized * racketManager.defenceHitForce, ForceMode.Impulse);

                    if (racketManager.transform.root.name == "Player1")
                    {
                        GameManager.instance.Player1Info.defenceCount++;
                        p1StatesPanel.ShowMessageLeft("Defence!!!");
                    }
                    if (racketManager.transform.root.name == "Player2")
                    {
                        GameManager.instance.Player2Info.defenceCount++;
                        p2StatesPanel.ShowMessageRight("Defence!!!");
                    }

                    trailRenderer.startColor = DefenseTrailColor;
                    DefenseVFX.Play();

                }
                else
                {
                    ballStates = BallStates.NormalHit;

                    rb.AddForce(racketManager.transform.up.normalized * racketManager.swinDownForce, ForceMode.Impulse);

                    trailRenderer.startColor = NormalTrailColor;
                    HitSound.Play();
                }
            }
            else
            {
                // Power Hit
                Vector3 hittingAngle = Quaternion.FromToRotation(Vector3.right, -racketManager.transform.up).eulerAngles;
                if ((360 >= hittingAngle.z && hittingAngle.z >= 170 || 10 >= hittingAngle.z && hittingAngle.z >= 0) && 
                    !racketManager.transform.root.GetComponent<PlayerMovement>().onGround)
                {
                    ballStates = BallStates.Smash;

                    rb.AddForce((-racketManager.transform.up.normalized) * racketManager.powerHitForce, ForceMode.Impulse);

                    if (racketManager.transform.root.name == "Player1")
                    {
                        p1StatesPanel.ShowMessageLeft("Smash!!!");
                        GameManager.instance.Player1Info.smashCount++;
                    }
                    if (racketManager.transform.root.name == "Player2")
                    {
                        p2StatesPanel.ShowMessageRight("Smash!!!");
                        GameManager.instance.Player2Info.smashCount++;
                    }

                    PowerHitParticle.Play();
                    trailRenderer.startColor = SmashTrailColor;
                    SmashSound.Play();
                }
                else
                {

                    if (racketManager.transform.root.name == "Player1")
                        GameManager.instance.Player1Info.overhandCount++;
                    if (racketManager.transform.root.name == "Player2")
                        GameManager.instance.Player2Info.overhandCount++;

                    // If the previous state was a smash, then this hit should be a defensive shot.
                    if (ballStates == BallStates.Smash)
                    {
                        ballStates = BallStates.Defense;

                        rb.AddForce(-racketManager.transform.up.normalized * racketManager.defenceHitForce, ForceMode.Impulse);

                        if (racketManager.transform.root.name == "Player1")
                        {
                            GameManager.instance.Player1Info.defenceCount++;
                            p1StatesPanel.ShowMessageLeft("Defence!!!");
                        }
                        if (racketManager.transform.root.name == "Player2")
                        {
                            GameManager.instance.Player2Info.defenceCount++;
                            p2StatesPanel.ShowMessageRight("Defence!!!");
                        }

                        trailRenderer.startColor = DefenseTrailColor;
                        DefenseVFX.Play();

                    }
                    else
                    {
                        ballStates = BallStates.NormalHit;

                        rb.AddForce(-racketManager.transform.up.normalized * racketManager.hitForce, ForceMode.Impulse);

                        trailRenderer.startColor = NormalTrailColor;
                        HitSound.Play();
                    }
                }
            }

            racketManager.boxColliderDisable();

            trailRenderer.enabled = true;
            HitParticle.Play();
        }
    }

    // Hit Floor
    private void OnCollisionEnter(Collision collision)
    {
        if(ballStates != BallStates.Serving && collision.transform.tag == "Ground")
        {
            ballStates = BallStates.Serving;

            if (collision.gameObject.name == "Player2Floor")
            {
                GameManager.instance.p1GetPoint();
            }
            else if (collision.gameObject.name == "Player1Floor")
            {
                GameManager.instance.p2GetPoint();
            }

            trailRenderer.startColor = NormalTrailColor;
            trailRenderer.enabled = false;
            HittingFloorSound.Play();
        }
    }
}
