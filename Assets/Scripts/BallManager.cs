using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    Rigidbody body;
    [SerializeField] private float ServeForce = 10.0f;
    [SerializeField] private float hitForce = 10.0f;

    [SerializeField] PlayerMovement p1;
    [SerializeField] PlayerMovement p2;

    [SerializeField] AudioSource HitSound;
    [SerializeField] AudioSource HittingFloorSound;
    public bool isServing = true;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        Physics.IgnoreLayerCollision(7, 8,true);
        Physics.IgnoreLayerCollision(7, 9, true);
    }
    private void Update()
    {
        if(body.velocity.x >= 0)
            transform.rotation = Quaternion.Euler(0, 0, -Quaternion.LookRotation(body.velocity.normalized, Vector3.up).eulerAngles.x);
        else
            transform.rotation = Quaternion.Euler(0, 0, 180 + Quaternion.LookRotation(body.velocity.normalized, Vector3.up).eulerAngles.x);


    }
    public IEnumerator Serve(float delay, bool faceRight)
    {
        yield return new WaitForSeconds(delay);
        body.velocity = Vector3.zero;

        HitSound.Play();

        if (faceRight)
            body.AddForce( (new Vector3(1.0f, 2.5f,0.0f)).normalized * ServeForce, ForceMode.Impulse);
        else
            body.AddForce((new Vector3(-1.0f, 2.5f, 0.0f)).normalized * ServeForce, ForceMode.Impulse);
    }

    // Hit Racket
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit");
        RacketManager racketManager = other.transform.GetComponent<RacketManager>();
        if (racketManager)
        {
            HitSound.Play();
            body.velocity = Vector3.zero;
            Debug.Log("Hit racketManager");
            if (racketManager.isSwinDown)
            {
                body.AddForce(racketManager.transform.up.normalized * hitForce, ForceMode.Impulse);
            }
            else
            {
                body.AddForce((-racketManager.transform.up.normalized) * hitForce, ForceMode.Impulse);
            }
            racketManager.boxColliderDisable();
        }
    }

    // Hit Floor
    private void OnCollisionEnter(Collision collision)
    {
        if(!isServing && collision.transform.tag == "Ground")
        {
            p1.transform.position = new Vector3(-3, p1.transform.position.y, p1.transform.position.z);
            p2.transform.position = new Vector3(3, p1.transform.position.y, p1.transform.position.z);

            if (collision.gameObject.name == "Player2Floor")
            {
                Debug.Log("Player1 Point +1");
                GameManager.player1Score++;
                HittingFloorSound.Play();
                p1.PrepareServe = true;
            }
            else if (collision.gameObject.name == "Player1Floor")
            {
                Debug.Log("Player2 Point +1");
                GameManager.player2Score++;
                HittingFloorSound.Play();
                p2.PrepareServe = true;
            }
        }
    }
}
