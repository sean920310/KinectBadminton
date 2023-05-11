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

    [SerializeField] float trailDestoryTime;
    [SerializeField] float trailSpawnTime;

    [SerializeField] GameObject RacketModel;

    bool trailEnd = false;

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

    public void setTrailOn()
    {
        trailEnd = false;
        StartCoroutine(slideTrail());
    }
    public void setTrailOff()
    {
        trailEnd = true;
    }


    IEnumerator slideTrail()
    {
        while (!trailEnd)
        {
            GameObject tempObject = Instantiate(RacketModel);
            tempObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
            //tempObject.transform.localScale = transform.localScale;
            GameObject.Destroy(tempObject, trailDestoryTime);

            //StartCoroutine(trailAlphaChange(tempObject));

            yield return new WaitForSeconds(trailSpawnTime);
        }
    }
    IEnumerator trailAlphaChange(GameObject trailObj)
    {
        SpriteRenderer sr = trailObj.GetComponent<SpriteRenderer>();
        float aliveTime = 0, alpha = 1.0f;
        float maxAliveTime = trailDestoryTime;

        while (trailObj != null)
        {
            aliveTime += 0.01f;
            alpha = Mathf.Lerp(0.8f, 0f, aliveTime / maxAliveTime);
            sr.color = new Color(1.0f, 1.0f, 1.0f, alpha);

            yield return new WaitForSeconds(0.01f);
        }

    }
}
