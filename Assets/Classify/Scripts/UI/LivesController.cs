using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class LivesController : MonoBehaviour
{
    public Image[] lives;
    public Color lifeColor;
    public Color usedLifeColor;
    public IntField livesField;
    public Sprite lifeSprite;
    public Sprite usedLifeSprite;


    int currentLifeIndex = 0;
    int usedLife = -1;

    public void OnEnable()
    {
        livesField.onChange.AddListener(OnLivesValueChanged);
    }

    public void OnDisable()
    {
        livesField.onChange.RemoveListener(OnLivesValueChanged);
    }

    private void OnLivesValueChanged()
    {
        if (livesField.Value > (currentLifeIndex - usedLife))
            AddLife();
        else
            RemoveLife();
    }

    public void Show()
    {
        currentLifeIndex = 0;
        usedLife = -1;
        for (int i = 0; i < lives.Length; i++)
        {
            lives[i].gameObject.SetActive(false);
            lives[i].color = lifeColor;
            lives[i].sprite = lifeSprite;
        }
        transform.localScale = new Vector3(0, 1, 1);
        transform.DOScaleX(1, 0.5f).SetEase(Ease.OutBounce).OnComplete(() => { lives[0].gameObject.SetActive(true); });
    }

    public void AddLife()
    {
        if (currentLifeIndex >= lives.Length - 1) return;
        currentLifeIndex ++;
        lives[currentLifeIndex].color = lifeColor;
        lives[currentLifeIndex].sprite = lifeSprite;
        lives[currentLifeIndex].gameObject.SetActive(true);
    }

    public void RemoveLife()
    {
        if (usedLife >= lives.Length - 1) return;
        usedLife++;
        lives[usedLife].color = usedLifeColor;
        lives[usedLife].sprite = usedLifeSprite;
        lives[usedLife].gameObject.transform.DOPunchScale(new Vector3(1.25f,1.25f,1), 0.25f).SetUpdate(true);
    }

    public void Hide()
    {
        for (int i = 0; i < lives.Length; i++)
        {
            lives[i].gameObject.SetActive(false);
        }
        transform.localScale = new Vector3(0, 1, 1);
    }
}
