using UnityEngine;

[CreateAssetMenu(menuName = "Database/UI Data")]
public class CreditDataSO : ScriptableObject
{
    [Header("Customize Credit Text :")]
    [SerializeField] [TextArea] string _devText;
    [SerializeField] [TextArea] string _sfxText;
    [SerializeField] [TextArea] string _contactText;

    public string DevText => _devText;
    public string SfxTxt => _sfxText;
    public string ContactText => _contactText;
}
