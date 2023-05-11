using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class RacketManager : MonoBehaviour
{
    public bool isSwinUp { get; private set; }
    public bool isSwinDown { get; private set; }

    public float LongServeForce = 11.0f;
    public Vector2 LongServeDirection;
    public float ShortServeForce = 8.0f;
    public Vector2 ShortServeDirection;
    public float hitForce = 9.5f;
    public float swinDownForce = 10f;
    public float powerHitForce = 11.0f;
    public float defenceHitForce = 11.0f;

    [SerializeField] BoxCollider boxCollider;

    public void swinUp()
    {
        isSwinUp = true;
        isSwinDown= false;

        boxColliderEnable();
    }

    public void swinDown()
    {
        isSwinUp = false;
        isSwinDown = true;

        boxColliderEnable();
    }
    //public void swoop()
    //{
    //    isSwinUp = false;
    //    isSwinDown = true;

    //    boxColliderEnable();
    //}

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
