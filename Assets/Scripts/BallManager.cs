using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public Rigidbody body;
    ParticleSystem particleSystems;
    TrailRenderer trailRenderer;

    [SerializeField] private float ServeForce = 10.0f;
    [SerializeField] private float hitForce = 10.0f;
    [SerializeField] private float powerHitForce = 10.0f;

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

    bool isFlyingUp = false;
    public bool isSmashBall { get; private set; } = false;

    [SerializeField] StatesPanel p1StatesPanel;
    [SerializeField] StatesPanel p2StatesPanel;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        particleSystems = GetComponent<ParticleSystem>();
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

        if (trailRenderer.startColor == Color.red)
            isSmashBall = true;
        else
            isSmashBall = false;

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

        if (isServing)
        {
            trailRenderer.enabled = false;
        }
        else
        {
            trailRenderer.enabled = true;
        }
    }
    public IEnumerator Serve(float delay, bool faceRight)
    {

        yield return new WaitForSeconds(delay);
        body.velocity = Vector3.zero;

        HitSound.Play();
        particleSystems.Play();

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
            particleSystems.Play();

            body.velocity = Vector3.zero;
            bool isDefence = isSmashBall;

            if (isDefence)
            {
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

                body.AddForce(racketManager.transform.up.normalized * hitForce, ForceMode.Impulse);
                trailRenderer.startColor = Color.white;

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
                    body.AddForce((-racketManager.transform.up.normalized) * powerHitForce, ForceMode.Impulse);
                    trailRenderer.startColor = Color.red;
                    SmashSound.Play();

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

                }
                else
                {
                    body.AddForce((-racketManager.transform.up.normalized) * hitForce, ForceMode.Impulse);
                    trailRenderer.startColor = Color.white;
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
            isServing = true;
            trailRenderer.startColor = Color.white;

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
