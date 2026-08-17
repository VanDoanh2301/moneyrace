using System.Collections;
using UnityEngine;

/// <summary>
/// Hiệu ứng nhấp nháy tạm thời cho Ring được Hint chọn. Tự gỡ bỏ sau khi chạy xong.
/// Nếu Ring bị nhặt (Destroy) giữa chừng, component và coroutine biến mất theo — an toàn.
/// </summary>
public class RingHighlight : MonoBehaviour
{
    private const float Duration = 2f;
    private const int FlashCount = 4;

    private void Start()
    {
        StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Destroy(this);
            yield break;
        }

        Color original = rend.material.color;
        Color highlight = Color.white;

        float interval = Duration / (FlashCount * 2f);

        for (int i = 0; i < FlashCount; i++)
        {
            rend.material.color = highlight;
            yield return new WaitForSeconds(interval);

            rend.material.color = original;
            yield return new WaitForSeconds(interval);
        }

        Destroy(this);
    }
}
