using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KinectImageOutput : MonoBehaviour
{
    public GameObject ColorSourceManager;
    private ColorSourceManager _ColorManager;
    private Image image;
    private Sprite imageSprite;
    void Start()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        if (ColorSourceManager == null)
        {
            return;
        }

        _ColorManager = ColorSourceManager.GetComponent<ColorSourceManager>();
        if (_ColorManager == null)
        {
            return;
        }

        imageSprite = Sprite.Create(_ColorManager.GetColorTexture(), 
            new Rect(0, 0, _ColorManager.GetColorTexture().width, _ColorManager.GetColorTexture().height), Vector2.zero);
        image.sprite = imageSprite;
    }
}
