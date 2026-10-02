
using UnityEngine;

public class Player : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.GetComponent<MeshRenderer>().material.name.Contains(ColorManager.Instance.backgroundMainMat.name))
		{
			UnityEngine.Object.Destroy(other.gameObject);
			ScoreManager.Instance.UpdateScore(1);
			GameManager.Instance.BallCollected();
			AudioManager.Instance.PlayEffects(AudioManager.Instance.sameColor);
		}
		else
		{
			AudioManager.Instance.PlayEffects(AudioManager.Instance.gameOver);
			AudioManager.Instance.PlayEffects(AudioManager.Instance.wrongColor);
			GameManager.Instance.GameOver();
		}
	}
}
