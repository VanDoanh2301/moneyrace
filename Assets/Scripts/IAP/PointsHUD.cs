using UnityEngine;
using TMPro;

#pragma warning disable CS0649

/// <summary>
/// Hiển thị số điểm của ví bền vững (<see cref="PointsWallet"/>) trên HUD.
/// </summary>
public class PointsHUD : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_PointsText;

    private string m_PointsTextFormat;

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        PointsWallet.m_OnPointsChanged += OnPointsChanged;

        Refresh();
    }

    private void OnDisable()
    {
        PointsWallet.m_OnPointsChanged -= OnPointsChanged;
    }

    private void OnPointsChanged(int points)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (m_PointsText == null) return;

        if (string.IsNullOrEmpty(m_PointsTextFormat))
        {
            m_PointsTextFormat = m_PointsText.text;

            if (string.IsNullOrEmpty(m_PointsTextFormat) || !m_PointsTextFormat.Contains("{0}"))
            {
                m_PointsTextFormat = "{0}";
            }
        }

        m_PointsText.text = string.Format(m_PointsTextFormat, PointsWallet.Points);
    }
}
