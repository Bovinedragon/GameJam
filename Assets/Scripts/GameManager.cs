using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
	public GameObject m_MainCamera;
    public GameObject m_MainDirectionalLight;
	public GameObject m_LoadingProgressTextGO;
	public GameObject m_StartGameButtonGO;
	public Text m_LoadingProgressText;
    public Cubemap m_MainMenuCubemap;
    public Cubemap m_GameLevelCubmap;

	private AsyncOperation m_asyncSceneLoad;
	
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

	public void StartGame()
	{
		m_asyncSceneLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        Scene gameLevelScene = SceneManager.GetSceneAt(1);
        SceneManager.SetActiveScene(gameLevelScene);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
	}

	public void MainMenu()
	{
		SceneManager.UnloadScene(1);
		m_MainCamera.SetActive(true);
        m_MainDirectionalLight.SetActive(true);
		m_StartGameButtonGO.SetActive(true);
        Scene mainScene = SceneManager.GetSceneAt(0);
        SceneManager.SetActiveScene(mainScene);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
	}
}
