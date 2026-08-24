using UnityEngine;

/// <summary>
/// Âm thanh bấm nút dùng chung toàn game (menu + gameplay). Tự sinh 1 tiếng "click" ngắn bằng code
/// (không cần asset, không cần kéo AudioSource/AudioClip vào Inspector) nên gọi được ở bất cứ đâu:
/// UISound.PlayClick().
/// </summary>
public static class UISound
{
    private static AudioSource s_source;
    private static AudioClip s_clickClip;
    private static AudioClip s_tapClip;

    /// <summary>Tiếng "click" cho UI (menu, shop, pause...).</summary>
    public static void PlayClick()
    {
        EnsureReady();

        if (s_source != null && s_clickClip != null)
            s_source.PlayOneShot(s_clickClip);
    }

    /// <summary>Tiếng "tap" đục hơn cho thao tác gameplay (bấm hạ bào xuống đập fruit).</summary>
    public static void PlayTap()
    {
        EnsureReady();

        if (s_source != null && s_tapClip != null)
            s_source.PlayOneShot(s_tapClip, 0.8f);
    }

    private static void EnsureReady()
    {
        if (s_source != null) return;

        var go = new GameObject("UISoundPlayer");
        Object.DontDestroyOnLoad(go);
        s_source = go.AddComponent<AudioSource>();
        s_source.playOnAwake = false;
        s_source.spatialBlend = 0f;

        s_clickClip = BuildTone(1400f, 0.06f, false);
        s_tapClip = BuildTone(220f, 0.09f, true);
    }

    /// <summary>Sinh 1 tiếng bíp ngắn giảm dần (sine + decay). pitchDrop=true cho cảm giác "đập/gõ" đục hơn.</summary>
    private static AudioClip BuildTone(float frequency, float duration, bool pitchDrop)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);

        var data = new float[sampleCount];
        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float progress = i / (float)sampleCount;
            float envelope = 1f - progress; // decay tuyến tính
            float freq = pitchDrop ? Mathf.Lerp(frequency * 1.6f, frequency * 0.6f, progress) : frequency;
            phase += freq / sampleRate;
            data[i] = Mathf.Sin(2f * Mathf.PI * phase) * envelope * 0.6f;
        }

        var clip = AudioClip.Create(pitchDrop ? "GameplayTap" : "UIClick", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
