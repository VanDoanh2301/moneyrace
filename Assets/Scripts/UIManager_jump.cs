using UnityEngine;
using UnityEngine.EventSystems;
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
			if (IsPointerOverUI())
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

	//check if pointer is over any UI element that blocks raycasts (buttons, panels, Shop screen...)
	//dùng IsPointerOverGameObject thay vì raycast + check riêng Button, vì check theo Button
	//sẽ bỏ lọt các UI không phải Button (vd: nền panel Shop, ScrollView) => input vẫn lọt xuống gameplay phía sau.
	public bool IsPointerOverUI()
	{
		if (EventSystem.current == null) return false;

		// Trên cảm ứng thật, bản IsPointerOverGameObject() không tham số chỉ check pointer id -1 (chuột ảo),
		// không khớp fingerId thật của ngón tay => luôn trả false dù đang chạm đúng UI (Shop Button...),
		// khiến tap lọt xuống thành bắt đầu game thay vì mở Shop. Phải truyền đúng fingerId khi có touch.
		if (Input.touchCount > 0)
		{
			return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
		}

		return EventSystem.current.IsPointerOverGameObject();
	}
}
