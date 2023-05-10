using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSlotUI : MonoBehaviour
{
    [SerializeField] Image sprite;
    [SerializeField] Button LeftButton;
    [SerializeField] Button RightButton;

    private void Start()
    {

    }

    public void UIUpdate(int idx)
    {
        if(idx == 0)
        {
            LeftButton.gameObject.SetActive(false);
        }
        else
        {
            LeftButton.gameObject.SetActive(true);
        }

        if (idx >= CharacterSlot.HatList.Length - 1)
        {
            RightButton.gameObject.SetActive(false);
        }
        else
        {
            RightButton.gameObject.SetActive(true);
        }

        if(CharacterSlot.HatList[idx].hatData.HatSprite != null)
        {
            sprite.color = Color.white;
            sprite.sprite = CharacterSlot.HatList[idx].hatData.HatSprite;
        }
        else
            sprite.color = new Color(0,0,0,0);
    }

}
