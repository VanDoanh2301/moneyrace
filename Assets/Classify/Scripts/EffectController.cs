using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class EffectController : MonoBehaviour
{
    public SpriteRenderer gradientSpriteRenderer;
    public ParticleSystem[] particles;
    public ParticleSystem particle2;

    public void ShowEffect(Color color)
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Simulate(0, false, true);
            var mainModule1 = particles[i].main;
            mainModule1.startColor = color;
        }
        color.a = 0;
        gradientSpriteRenderer.color = color;
        Sequence animSeq = DOTween.Sequence();
        animSeq.Append(gradientSpriteRenderer.DOColor(new Color(color.r, color.g, color.b, 1), 0.1f).OnComplete(()=> {
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Play();
            }
        }));
        animSeq.Append(gradientSpriteRenderer.DOColor(color, 0.35f));
        

    }
    // Start is called before the first frame update
    void Start()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ColorItem item = collision.gameObject.GetComponent<ColorItem>();
        if(item)
        {
            ShowEffect(ColorItemUitilites.IntIdToColor(item.ItemId));
        }
    }
}
