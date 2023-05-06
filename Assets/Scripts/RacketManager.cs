using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class RacketManager : MonoBehaviour
{
    public bool isSwinUp { get; private set; }
    public bool isSwinDown { get; private set; }

    public float ServeForce = 10.0f;
    public float hitForce = 10.0f;
    public float powerHitForce = 10.0f;


    [SerializeField] BoxCollider boxCollider;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void swinUp()
    {
        isSwinUp = true;
        isSwinDown= false;
    }

    public void swinDown()
    {
        isSwinUp = false;
        isSwinDown = true;
    }

    public void boxColliderDisable()
    {
        boxCollider.enabled = false;
        isSwinUp = false;
        isSwinDown = false;
    }

    public void boxColliderEnable()
    {
        boxCollider.enabled = true;
    }

}
