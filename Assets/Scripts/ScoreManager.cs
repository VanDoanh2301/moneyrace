
using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
	public TMP_Text currentScoreLabel;

	public TMP_Text highScoreLabel;

	public TMP_Text currentScoreGameOverLabel;

	public TMP_Text highScoreGameOverLabel;

	public float currentScore;

	public float highScore;

	private bool counting;

	public static ScoreManager Instance
	{
		get;
		set;
	}

	private void Awake()
	{
		Object.DontDestroyOnLoad(this);
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		if (!PlayerPrefs.HasKey("HighScore"))
		{
			PlayerPrefs.SetFloat("HighScore", 0f);
		}
		highScore = PlayerPrefs.GetFloat("HighScore");
		UpdateHighScore();
		ResetCurrentScore();
	}

	private void UpdateHighScore()
	{
		if (currentScore > highScore)
		{
			highScore = currentScore;
		}
		highScoreLabel.text = highScore.ToString("F1");
		PlayerPrefs.SetFloat("HighScore", highScore);
	}

	public void UpdateScore(int value)
	{
		currentScore += value;
		Round(currentScore, 1);
		currentScoreLabel.text = currentScore.ToString("F1");
	}

	public void ResetCurrentScore()
	{
		currentScore = 0f;
		UpdateScore(0);
	}

	public void UpdateScoreGameover()
	{
		UpdateHighScore();
		currentScoreGameOverLabel.text = currentScore.ToString("F1");
		highScoreGameOverLabel.text = highScore.ToString("F1");
	}

	public void StartCounting()
	{
		counting = true;
		StartCoroutine(Counter());
	}

	public void StopCounting()
	{
		counting = false;
		StopCoroutine(Counter());
	}

	private IEnumerator Counter()
	{
		while (counting)
		{
			currentScore += 0.1f;
			Round(currentScore, 1);
			currentScoreLabel.text = currentScore.ToString("F1");
			yield return new WaitForSeconds(0.1f);
		}
	}

	public float Round(float value, int digits)
	{
		float num = Mathf.Pow(10f, digits);
		return Mathf.Round(value * num) / num;
	}
}
