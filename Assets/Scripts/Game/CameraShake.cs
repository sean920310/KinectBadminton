using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 orignalPosition = transform.position;
        float elapsed = 0f;

        float maxRange = 0.0f;

        while (elapsed < duration)
        {
            maxRange = (Mathf.Sin(elapsed) > maxRange) ? Mathf.Sin(elapsed) : maxRange;

            float x = Random.Range(-maxRange, maxRange) * magnitude;
            float y = Random.Range(-maxRange, maxRange) * magnitude;

            transform.position = new Vector3(x, y, 0.0f) + orignalPosition;
            elapsed += Time.deltaTime;
            yield return 0;
        }
        transform.position = orignalPosition;
    }


}