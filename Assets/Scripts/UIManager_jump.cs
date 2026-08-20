using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager_jump : MonoBehaviour {

	[Header("GUI Components")]
	public GameObject mainMenuGui;
	public GameObject pauseGui, gameplayGui, gameOverGui;

	public GameState_jump gameState;

	bool clicked;
	public GameObject panel_loading;

	// Use this for initialization
	void Start () {
		mainMenuGui.SetActive(true);
		pauseGui.SetActive(false);
		gameplayGui.SetActive(false);
		gameOverGui.SetActive(false);
		gameState = GameState_jump.MENU;
	}

    void Update()
    {
		if (Input.GetMouseButtonDown(0) && gameState == GameState_jump.MENU && !clicked)
		{
			if (IsButton())
				return;

			AudioManager_jump.Instance.PlayEffects(AudioManager_jump.Instance.buttonClick);
			ShowGameplay();
			AudioManager_jump.Instance.PlayMusic(AudioManager_jump.Instance.gameMusic);
		}
		else if (Input.GetMouseButtonUp(0) && clicked && gameState == GameState_jump.MENU)
			clicked = false;
	}

    //show main menu
    public void ShowMainMenu()
	{
		panel_loading.SetActive(true);
        SceneManager.LoadSceneAsync(0 , LoadSceneMode.Single);  
	}

    //show pause menu
    public void ShowPauseMenu()
	{
		if (gameState == GameState_jump.PAUSED)
			return;

		pauseGui.SetActive(true);
		Time.timeScale = 0;
		gameState = GameState_jump.PAUSED;
		AudioManager_jump.Instance.PlayEffects(AudioManager_jump.Instance.buttonClick);
	}

	//hide pause menu
	public void HidePauseMenu()
	{
		pauseGui.SetActive(false);
		Time.timeScale = 1;
		gameState = GameState_jump.PLAYING;
		AudioManager_jump.Instance.PlayEffects(AudioManager_jump.Instance.buttonClick);
	}

	//show gameplay gui
	public void ShowGameplay()
	{
		mainMenuGui.SetActive(false);
		pauseGui.SetActive(false);
		gameplayGui.SetActive(true);
		gameOverGui.SetActive(false);
		gameState = GameState_jump.PLAYING;
		AudioManager_jump.Instance.PlayEffects(AudioManager_jump.Instance.buttonClick);
	}

	//show game over gui
	public void ShowGameOver()
	{
		mainMenuGui.SetActive(false);
		pauseGui.SetActive(false);
		gameplayGui.SetActive(false);
		gameOverGui.SetActive(true);
		gameState = GameState_jump.GAMEOVER;
		AudioManager_jump.Instance.PlayMusic(AudioManager_jump.Instance.menuMusic);
	}

	//check if user click any menu button
	public bool IsButton()
	{
		bool temp = false;

		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = Input.mousePosition
		};

		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, results);

		foreach (RaycastResult item in results)
		{
			temp |= item.gameObject.GetComponent<Button>() != null;
		}

		return temp;
	}
}
