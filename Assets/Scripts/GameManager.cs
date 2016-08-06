using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public int m_FeedWinCount = 15;
    public int m_DeadLoseCount = 3;
    public Canvas m_WinCanvas;
    public Canvas m_LoseCanvas;
	public GameObject m_MainCamera;
    public GameObject m_MainDirectionalLight;
	public GameObject m_LoadingProgressTextGO;
	public GameObject m_StartGameButtonGO;
	public GameObject m_QuitGameButton;
	public Text m_LoadingProgressText;
    public Cubemap m_MainMenuCubemap;
    public Cubemap m_GameLevelCubmap;

	private AsyncOperation m_asyncSceneLoad;
    private float m_winLoseDelayTime = 0f;
	
    static private GameManager s_instance;

    static public GameManager Get()
	{
		return s_instance;
	}

	void Awake()
	{
		s_instance = this;
	}

	void Start()
	{
		Application.targetFrameRate = 30;
		m_LoadingProgressText.gameObject.SetActive(false);
	}

	void Update()
	{
		if (m_asyncSceneLoad != null)
		{
			// finished loading game level
			if (m_asyncSceneLoad.isDone)
			{
				m_LoadingProgressText.gameObject.SetActive(false);
				m_MainCamera.SetActive(false);
                m_MainDirectionalLight.SetActive(false);
				m_asyncSceneLoad = null;
				return;
			}

			// show loading progress
			float progress = m_asyncSceneLoad.progress;
			if (!m_LoadingProgressText.gameObject.activeSelf)
				m_LoadingProgressText.gameObject.SetActive(true);
			m_LoadingProgressText.text = string.Format("{0}%", Mathf.CeilToInt(progress) * 100);
		}
	}

    public void UpdateGameData(int happy, int dead)
    {
        if (happy >= m_FeedWinCount)
            Win();
        if (dead >= m_DeadLoseCount)
            Lose();
    }

	public void QuitGame()
	{
		Application.Quit();
	}

	public void StartGame()
	{
        Time.timeScale = 1f;
		m_asyncSceneLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        Scene gameLevelScene = SceneManager.GetSceneAt(1);
        SceneManager.SetActiveScene(gameLevelScene);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
	}

	public void MainMenu()
	{
        // add small delay to win lose screen before click though
        if (Time.realtimeSinceStartup < m_winLoseDelayTime)
            return;
        
        Time.timeScale = 1f;
		SceneManager.UnloadScene(1);
		m_MainCamera.SetActive(true);
        m_MainDirectionalLight.SetActive(true);
		m_StartGameButtonGO.SetActive(true);
		m_QuitGameButton.SetActive(true);
        Scene mainScene = SceneManager.GetSceneAt(0);
        SceneManager.SetActiveScene(mainScene);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        m_WinCanvas.gameObject.SetActive(false);
        m_LoseCanvas.gameObject.SetActive(false);
	}

    private void Win()
    {
        m_WinCanvas.gameObject.SetActive(true);
        Time.timeScale = 0.01f;
        m_winLoseDelayTime = Time.realtimeSinceStartup + 2f;
    }

    private void Lose()
    {
        m_LoseCanvas.gameObject.SetActive(true);
        Time.timeScale = 0.01f;
        m_winLoseDelayTime = Time.realtimeSinceStartup + 2f;
    }
}
