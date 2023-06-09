using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct HatData
{
    public string HatName;
    public Sprite HatSprite;
    public GameObject HatPrefab;
}

[CreateAssetMenu]
public class HatSO : ScriptableObject
{
    public HatData hatData;
}
