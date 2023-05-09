using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public Rigidbody body;

    [SerializeField] ParticleSystem HitParticle;
    [SerializeField] ParticleSystem PowerHitParticle;

    TrailRenderer trailRenderer;

    [SerializeField] AudioSource HitSound;
    [SerializeField] AudioSource SmashSound;
    [SerializeField] AudioSource HittingFloorSound;

    public bool isServing = false;

    [SerializeField] bool badmintonSimulation = false;
    [SerializeField] float velocityDecayMultiplier = 0.99f;
    [SerializeField] float FlyDownVelocityDecayMultiplier = 0.99f;
    [SerializeField] float FlyDownVelocityXDecayMultiplier = 0.99f;
    [SerializeField] bool hitForceEnhence = false;
    [SerializeField] float hitForceEnhenceMultiplier = 1.5f;
    bool hitForceEnhenceFlag = false;

    [SerializeField] StatesPanel p1StatesPanel;
    [SerializeField] StatesPanel p2StatesPanel;

    [SerializeField] Transform centerBorder;

    [SerializeField] Color SmashTrailColor;
    [SerializeField] Color DefenseTrailColor;
    [SerializeField] Color NormalTrailColor;


    bool isFlyingUp = false;
    public bool isSmashBall { get; private set; } = false;
    public bool isDefenseBall { get; private set; } = false;
    public bool BallInLeftSide { get; private set; }
    private void Start()
    {
        body = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();

        Physics.IgnoreLayerCollision(7, 8,true);
        Physics.IgnoreLayerCollision(7, 9, true);
    }

    private void FixedUpdate()
    {

        if (badmintonSimulation && isFlyingUp)
        {
            body.velocity *= velocityDecayMultiplier;
        }
    }

    private void Update()
    {
        BallInLeftSide = (transform.position.x < centerBorder.transform.position.x);

        if (hitForceEnhenceFlag && hitForceEnhence)
        {
            body.velocity *= hitForceEnhenceMultiplier;
            hitForceEnhenceFlag = false;
        }

        if (badmintonSimulation)
        {
            if (body.velocity.y > 0)
            {
                isFlyingUp = true;
            }
            else
            {
                if (isFlyingUp)
                {
                    body.velocity = new Vector3(body.velocity.x * FlyDownVelocityXDecayMultiplier, body.velocity.y, body.velocity.z) * FlyDownVelocityDecayMultiplier;
                }

                isFlyingUp = false;
            }
        }

        transform.rotation = Quaternion.Euler(0, 0, Quaternion.FromToRotation(Vector3.right, body.velocity).eulerAngles.z);

    }
    public IEnumerator Serve(float delay, bool faceRight, float ServeForce)
    {

        yield return new WaitForSeconds(delay);
        body.velocity = Vector3.zero;

        HitSound.Play();
        HitParticle.Play();

        trailRenderer.enabled = true;

        if (faceRight)
            body.AddForce( (new Vector3(1.0f, 1.5f,0.0f)).normalized * ServeForce, ForceMode.Impulse);
        else
            body.AddForce((new Vector3(-1.0f, 1.5f, 0.0f)).normalized * ServeForce, ForceMode.Impulse);

        //hitForceEnhenceFlag = true;
    }

    // Hit Racket
    private void OnTriggerEnter(Collider other)
    {
        RacketManager racketManager = other.transform.GetComponent<RacketManager>();
        if (racketManager)
        {
            trailRenderer.enabled = true;
            HitParticle.Play();

            body.velocity = Vector3.zero;
            bool isDefence = isSmashBall;

            if (isDefence)
            {
                isDefenseBall = true;
                trailRenderer.startColor = DefenseTrailColor;

                if (racketManager.transform.root.name == "Player1")
                {
                    GameManager.instance.player1Defence++;
                    p1StatesPanel.ShowMessageLeft("Defence!!!");
                }
                if (racketManager.transform.root.name == "Player2")
                {
                    GameManager.instance.player2Defence++;
                    p2StatesPanel.ShowMessageRight("Defence!!!");
                }
            }
            if (racketManager.isSwinDown)
            {
                isDefenseBall = false;
                isSmashBall = false;

                if (!isDefence)
                {
                    trailRenderer.startColor = NormalTrailColor;
                    body.AddForce(racketManager.transform.up.normalized * racketManager.swinDownForce, ForceMode.Impulse);
                }
                else
                {
                    body.AddForce(racketManager.transform.up.normalized * racketManager.defenceHitForce, ForceMode.Impulse);
                }

                HitSound.Play();

                if (racketManager.transform.root.name == "Player1")
                    GameManager.instance.player1Underhand++;
                if (racketManager.transform.root.name == "Player2")
                    GameManager.instance.player2Underhand++;
            }
            else
            {
                // Power Hit
                Vector3 hittingAngle = Quaternion.FromToRotation(Vector3.right, -racketManager.transform.up).eulerAngles;
                if ((360 >= hittingAngle.z && hittingAngle.z >= 170 || 10 >= hittingAngle.z && hittingAngle.z >= 0) && !racketManager.transform.root.GetComponent<PlayerMovement>().onGround)
                {
                    isSmashBall = true;
                    trailRenderer.startColor = SmashTrailColor;
                    SmashSound.Play();

                    body.AddForce((-racketManager.transform.up.normalized) * racketManager.powerHitForce, ForceMode.Impulse);

                    if (racketManager.transform.root.name == "Player1")
                    {
                        p1StatesPanel.ShowMessageLeft("Smash!!!");
                        GameManager.instance.player1Smash++;
                    }
                    if (racketManager.transform.root.name == "Player2")
                    {
                        p2StatesPanel.ShowMessageRight("Smash!!!");
                        GameManager.instance.player2Smash++;
                    }

                    PowerHitParticle.Play();
                }
                else
                {
                    isDefenseBall = false;
                    isSmashBall = false;

                    if (!isDefence)
                    {
                        trailRenderer.startColor = NormalTrailColor;
                        body.AddForce(-racketManager.transform.up.normalized * racketManager.hitForce, ForceMode.Impulse);
                    }
                    else
                    {
                        body.AddForce(-racketManager.transform.up.normalized * racketManager.defenceHitForce, ForceMode.Impulse);
                    }

                    HitSound.Play();

                    if (racketManager.transform.root.name == "Player1")
                        GameManager.instance.player1Overhand++;
                    if (racketManager.transform.root.name == "Player2")
                        GameManager.instance.player2Overhand++;
                }
            }

            racketManager.boxColliderDisable();

            hitForceEnhenceFlag = true;
            // Hit ball slow motion
            //StartCoroutine(AddTimeScale());
        }
    }

    // Hit Floor
    private void OnCollisionEnter(Collision collision)
    {
        if(!isServing && collision.transform.tag == "Ground")
        {
            isDefenseBall = false;
            isSmashBall = false;

            isServing = true;

            trailRenderer.enabled = false;
            trailRenderer.startColor = NormalTrailColor;

            if (collision.gameObject.name == "Player2Floor")
            {
                Debug.Log("Player1 Point +1");
                GameManager.instance.p1GetPoint();

                HittingFloorSound.Play();
            }
            else if (collision.gameObject.name == "Player1Floor")
            {
                Debug.Log("Player2 Point +1");
                GameManager.instance.p2GetPoint();

                HittingFloorSound.Play();
            }
        }
    }

    IEnumerator AddTimeScale()
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSeconds(0.3f);
        Time.timeScale = 1f;
    }
}
