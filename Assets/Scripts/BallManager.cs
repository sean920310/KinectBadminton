using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    Rigidbody body;
    ParticleSystem particleSystems;
    TrailRenderer trailRenderer;


    [SerializeField] private float ServeForce = 10.0f;
    [SerializeField] private float hitForce = 10.0f;
    [SerializeField] private float powerHitForce = 10.0f;

    [SerializeField] AudioSource HitSound;
    [SerializeField] AudioSource SmashSound;
    [SerializeField] AudioSource HittingFloorSound;


    public bool isServing = true;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        particleSystems = GetComponent<ParticleSystem>();
        trailRenderer = GetComponent<TrailRenderer>();

        Physics.IgnoreLayerCollision(7, 8,true);
        Physics.IgnoreLayerCollision(7, 9, true);
    }
    private void Update()
    {
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
            body.AddForce( (new Vector3(1.0f, 2.5f,0.0f)).normalized * ServeForce, ForceMode.Impulse);
        else
            body.AddForce((new Vector3(-1.0f, 2.5f, 0.0f)).normalized * ServeForce, ForceMode.Impulse);
    }

    // Hit Racket
    private void OnTriggerEnter(Collider other)
    {
        RacketManager racketManager = other.transform.GetComponent<RacketManager>();
        if (racketManager)
        {
            particleSystems.Play();

            body.velocity = Vector3.zero;
            if (racketManager.isSwinDown)
            {
                body.AddForce(racketManager.transform.up.normalized * hitForce, ForceMode.Impulse);
                trailRenderer.startColor = Color.white;

                HitSound.Play();

            }
            else
            {
                Vector3 hittingAngle = Quaternion.FromToRotation(Vector3.right, -racketManager.transform.up).eulerAngles;
                if ((360 >= hittingAngle.z && hittingAngle.z >= 170 || 10 >= hittingAngle.z && hittingAngle.z >= 0) && !racketManager.transform.root.GetComponent<PlayerMovement>().onGround)
                {
                    body.AddForce((-racketManager.transform.up.normalized) * powerHitForce, ForceMode.Impulse);
                    trailRenderer.startColor = Color.red;
                    SmashSound.Play();
                }
                else
                {
                    body.AddForce((-racketManager.transform.up.normalized) * hitForce, ForceMode.Impulse);
                    trailRenderer.startColor = Color.white;
                    HitSound.Play();
                }
            }

            racketManager.boxColliderDisable();

            // Hit ball slow motion
            //StartCoroutine(AddTimeScale());
        }
    }

    // Hit Floor
    private void OnCollisionEnter(Collision collision)
    {
        if(!isServing && collision.transform.tag == "Ground")
        {

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
