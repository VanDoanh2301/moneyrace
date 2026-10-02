

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[Header("GUI Components")]
	public GameObject mainMenuGui;

	public GameObject pauseGui;

	public GameObject gameplayGui;

	public GameObject gameOverGui;

	public GameState gameState;

	private bool clicked;

	public static UIManager Instance
	{
		get;
		set;
	}

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
		mainMenuGui.SetActive(value: true);
		pauseGui.SetActive(value: false);
		gameplayGui.SetActive(value: false);
		gameOverGui.SetActive(value: false);
		gameState = GameState.MENU;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && gameState == GameState.MENU && !clicked && !ShopPanel.IsOpen)
		{
			if (!IsButton())
			{
				AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
				AudioManager.Instance.PlayMusic(AudioManager.Instance.gameMusic);
				ShowGameplay();
			}
		}
		else if (Input.GetMouseButtonUp(0) && clicked && gameState == GameState.MENU)
		{
			clicked = false;
		}
	}

	public void ShowMainMenu()
	{
		if (gameState == GameState.PAUSED)
		{
			Time.timeScale = 1f;
		}
		clicked = true;
		mainMenuGui.SetActive(value: true);
		pauseGui.SetActive(value: false);
		gameplayGui.SetActive(value: false);
		gameOverGui.SetActive(value: false);
		gameState = GameState.MENU;
		AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
		GameManager.Instance.ClearScene();
	}

	public void ShowPauseMenu()
	{
		if (gameState != GameState.PAUSED)
		{
			pauseGui.SetActive(value: true);
			Time.timeScale = 0f;
			gameState = GameState.PAUSED;
			AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
		}
	}

	public void HidePauseMenu()
	{
		pauseGui.SetActive(value: false);
		Time.timeScale = 1f;
		gameState = GameState.PLAYING;
		AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
	}

	public void ShowGameplay()
	{
		mainMenuGui.SetActive(value: false);
		pauseGui.SetActive(value: false);
		gameplayGui.SetActive(value: true);
		gameOverGui.SetActive(value: false);
		gameState = GameState.PLAYING;
		AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
	}

	public void ShowGameOver()
	{
		mainMenuGui.SetActive(value: false);
		pauseGui.SetActive(value: false);
		gameplayGui.SetActive(value: false);
		gameOverGui.SetActive(value: true);
		gameState = GameState.GAMEOVER;
	}

	public bool IsButton()
	{
		bool flag = false;
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = UnityEngine.Input.mousePosition;
		PointerEventData eventData = pointerEventData;
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, list);
		foreach (RaycastResult item in list)
		{
			flag |= (item.gameObject.GetComponent<Button>() != null);
		}
		return flag;
	}
}
