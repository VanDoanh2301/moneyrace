using UnityEngine;

[System.Serializable]
public struct NeonColorPair
{
    public Color top;
    public Color bottom;
}

public static class NeonColor
{
    static readonly int ColorTopID = Shader.PropertyToID("_ColorTop");
    static readonly int ColorBottomID = Shader.PropertyToID("_ColorBottom");
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    static MaterialPropertyBlock block;

    public static void Apply(SpriteRenderer renderer, NeonColorPair pair)
    {
        if (block == null)
            block = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(block);
        block.SetColor(ColorTopID, pair.top);
        block.SetColor(ColorBottomID, pair.bottom);
        block.SetColor(EmissionColorID, pair.top);
        renderer.SetPropertyBlock(block);
    }
}
