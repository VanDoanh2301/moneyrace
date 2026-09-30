using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tô gradient dọc cho một Graphic của uGUI bằng cách đổi màu từng đỉnh mesh.
/// Dùng chung với sprite bo góc trắng (9-slice) để có nút bo góc + gradient mà
/// không cần mỗi nút một ảnh riêng.
/// Nhân với màu sẵn có nên Button vẫn giữ được hiệu ứng ColorTint khi bấm.
/// </summary>
[AddComponentMenu("UI/Effects/UI Gradient")]
public class UIGradient : BaseMeshEffect
{
    [SerializeField]
    private Color topColor = Color.white;
    [SerializeField]
    private Color bottomColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    public Color TopColor
    {
        get { return topColor; }
        set { topColor = value; SetDirty(); }
    }

    public Color BottomColor
    {
        get { return bottomColor; }
        set { bottomColor = value; SetDirty(); }
    }

    public void SetColors(Color top, Color bottom)
    {
        topColor = top;
        bottomColor = bottom;
        SetDirty();
    }

    private void SetDirty()
    {
        if (graphic != null)
            graphic.SetVerticesDirty();
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0) return;

        Rect rect = graphic.rectTransform.rect;
        float bottom = rect.yMin;
        float height = rect.height;
        if (height <= 0f) return;

        UIVertex vertex = default(UIVertex);

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            float t = Mathf.Clamp01((vertex.position.y - bottom) / height);
            Color tint = Color.Lerp(bottomColor, topColor, t);

            // nhân với màu hiện tại để không phá ColorTint của Button
            Color current = vertex.color;
            vertex.color = new Color(current.r * tint.r, current.g * tint.g,
                                     current.b * tint.b, current.a * tint.a);

            vh.SetUIVertex(vertex, i);
        }
    }
}
