using UnityEngine;
using System.Collections;

public class OptionsMenu : MonoBehaviour
{
	public GameObject m_OptionsButtonCanvas;
	public GameObject m_OptionsMenuCanvas;

	void Awake()
	{
		m_OptionsButtonCanvas.SetActive(true);
		m_OptionsMenuCanvas.SetActive(false);
	}

	public void MainMenu()
	{
		m_OptionsButtonCanvas.SetActive(false);
		m_OptionsMenuCanvas.SetActive(false);
		GameManager.Get ().MainMenu ();
	}
}
