

using System.Collections;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
	public Material backgroundMainMat;

	public Material backgroundSubMat;

	public Material ground1Mat;

	public Material ground2Mat;

	public Material obstacleMat;

	public ColorList[] colorSets;

	private int colorIndex;

	public static ColorManager Instance
	{
		get;
		set;
	}

	/// <summary>Bộ màu đang được áp dụng (bộ vừa truyền cho ChangeColors).</summary>
	public ColorList CurrentSet
	{
		get;
		private set;
	}

	/// <summary>Phát khi đổi sang bộ màu mới — UI bám theo qua BallThemeDriver.</summary>
	public event System.Action<ColorList> OnPaletteChanged;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void Start()
	{
		// Random.Range(int,int) exclusive max: dùng Length-1 sẽ không bao giờ ra bộ cuối.
		colorIndex = UnityEngine.Random.Range(0, colorSets.Length);
		ChangeColors();
	}

	public void ChangeColors()
	{
		CurrentSet = colorSets[colorIndex];
		if (OnPaletteChanged != null)
		{
			OnPaletteChanged(CurrentSet);
		}
		StartCoroutine(ChangeColor(ground1Mat, ground1Mat.color, colorSets[colorIndex].Ground1, Time.time, 1));
		StartCoroutine(ChangeColor(ground2Mat, ground2Mat.color, colorSets[colorIndex].Ground2, Time.time, 1));
		StartCoroutine(ChangeColor(obstacleMat, obstacleMat.color, colorSets[colorIndex].Obstacle, Time.time, 1));
		StartCoroutine(ChangeColor(backgroundMainMat, backgroundMainMat.color, colorSets[colorIndex].backgroundMain, Time.time, 1));
		StartCoroutine(ChangeColor(backgroundSubMat, backgroundSubMat.color, colorSets[colorIndex].backgroundSub, Time.time, 1));
		colorIndex = UnityEngine.Random.Range(0, colorSets.Length);
	}

	private IEnumerator ChangeColor(Material mat, Color startColor, Color endColor, float time, int alpha)
	{
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime;
			mat.color = Color.Lerp(startColor, endColor, t);
			Color color = mat.color;
			float r = color.r;
			Color color2 = mat.color;
			float g = color2.g;
			Color color3 = mat.color;
			mat.color = new Color(r, g, color3.b, Mathf.Lerp(1f, alpha, t));
			yield return 0;
		}
	}
}
