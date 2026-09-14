using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ColorItem : Item
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Rigidbody2D rigidbody2D;
    Color currentColor;
    int currentId = -1;
    /// <summary>
    /// for Color item the id simply represent the color
    /// </summary>
    public override int ItemId
    {
        get
        {
            Color color = spriteRenderer.color;
            if (currentColor == color && currentId >= 0)
                return currentId;
            currentColor = color;
            currentId = ColorItemUitilites.ColorToIntId(color);
            return currentId;
        }
    }

    public Color Color
    {
        get
        {
            return spriteRenderer.color; 
        }
    }

    public void Freeze()
    {
        rigidbody2D.linearVelocity = Vector2.zero;
        rigidbody2D.angularVelocity = 0;
        rigidbody2D.gravityScale = 0;
    }

    public void Release()
    {
        rigidbody2D.gravityScale = 1;
    }

    void Update()
    {
        if(transform.position.y<-9)
        {
            gameObject.SetActive(false);
            transform.localPosition = Vector3.zero;
        }

    }

    public void Decay()
    {
        if (gameObject.activeSelf)
        {
            spriteRenderer.DOFade(0, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                gameObject.SetActive(false);
                Color color = spriteRenderer.color;
                color.a = 1;
                spriteRenderer.color = color;
                rigidbody2D.gravityScale = 1;
            });
        }
    }
    public void OnEnable()
    {
        spriteRenderer.DOKill();
    }
}
public static class ColorItemUitilites
{
    public static int ColorToIntId(Color32 color)
    {
        int rgb = color.r;
        rgb = (rgb << 8) + color.g ;
        rgb = (rgb << 8) + color.b ;
        int id = rgb;
        return id;
    }

    public static Color32 IntIdToColor(int id)
    {
        int blue = id & 255;
        int green = (id >> 8) & 255;
        int red = (id >> 16) & 255;
        return new Color(red / 255.0f, green / 255.0f, blue / 255.0f);
    }
}