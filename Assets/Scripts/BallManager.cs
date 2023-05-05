using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    Rigidbody body;
    [SerializeField] private float ServeForce = 10.0f;
    [SerializeField] private float hitForce = 10.0f;

    [SerializeField] PlayerMovement p1;
    private void Start()
    {
        body = GetComponent<Rigidbody>();
        Physics.IgnoreLayerCollision(7, 8,true);
        Physics.IgnoreLayerCollision(7, 9, true);
    }

    public IEnumerator Serve(float delay, bool faceRight)
    {
        yield return new WaitForSeconds(delay);
        body.velocity = Vector3.zero;

        if(faceRight)
            body.AddForce( (new Vector3(1.0f, 3f,0.0f)).normalized * ServeForce, ForceMode.Impulse);
        else
            body.AddForce((new Vector3(-1.0f, 3f, 0.0f)).normalized * ServeForce, ForceMode.Impulse);
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit");
        RacketManager racketManager = other.transform.GetComponent<RacketManager>();
        if (racketManager)
        {
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
            //racketManager.boxColliderDisable();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Ground")
        {
            p1.PrepareServe = true;
        }
    }
}
