using UnityEngine;

#pragma warning disable CS0649

/// <summary>
/// Nút Hint trong Game scene: tốn coin, làm sáng Ring còn lại gần nhất để gợi ý người chơi.
/// Ring không có thứ tự/identity riêng nên chọn Ring gần Main Camera nhất.
/// </summary>
public class HintController : MonoBehaviour
{
    public const int HintCost = 10;

    [SerializeField]
    private ToastMessage m_Toast;

    public void UseHint()
    {
        if (!CoinWallet.TrySpend(HintCost))
        {
            Debug.Log("[Hint] Không đủ coin.");
            if (m_Toast != null) m_Toast.Show("Không đủ coin!");
            return;
        }

        GameObject target = FindNearestRing();
        if (target != null)
        {
            target.AddComponent<RingHighlight>();
        }
    }

    private static GameObject FindNearestRing()
    {
        GameObject[] rings = GameObject.FindGameObjectsWithTag("Ring");
        if (rings.Length == 0) return null;

        Vector3 origin = Camera.main != null ? Camera.main.transform.position : Vector3.zero;

        GameObject nearest = null;
        float nearestDist = float.MaxValue;

        for (int i = 0; i < rings.Length; i++)
        {
            float dist = Vector3.Distance(origin, rings[i].transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = rings[i];
            }
        }

        return nearest;
    }
}
