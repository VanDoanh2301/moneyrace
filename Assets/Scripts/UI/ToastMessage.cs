using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

/// <summary>
/// Thông báo ngắn nổi lên rồi tự ẩn (vd: "Không đủ coin!"). Dùng chung cho mọi màn.
/// </summary>
public class ToastMessage : MonoBehaviour
{
    [SerializeField]
    private GameObject m_Root;

    [SerializeField]
    private Text m_Text;

    private Coroutine m_Routine;

    private void Awake()
    {
        if (m_Root != null) m_Root.SetActive(false);
    }

    public void Show(string message, float duration = 1.5f)
    {
        if (m_Text != null) m_Text.text = message;
        if (m_Root != null) m_Root.SetActive(true);

        if (m_Routine != null) StopCoroutine(m_Routine);
        m_Routine = StartCoroutine(HideAfter(duration));
    }

    private IEnumerator HideAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (m_Root != null) m_Root.SetActive(false);
        m_Routine = null;
    }
}
