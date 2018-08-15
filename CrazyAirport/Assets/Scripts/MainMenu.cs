using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	[SerializeField]
	private GameObject levelScreen;
	[SerializeField]
	private ShowHighscore[] allLevels;
	// Use this for initialization
	void Start()
	{
		levelScreen.SetActive(false);
	}

	public void OpenLevelScreen()
	{
		levelScreen.SetActive(true);
	}

	public void ResetAllHighscores()
	{
		int i = 1;
		foreach(ShowHighscore highscore in allLevels)
		{
			PlayerPrefs.SetInt("Level_" + i, 0);
			highscore.ResetHighscore();
			i++;
		}
	}

	public void LoadLevel(int id)
	{
		PlayerPrefs.SetInt("Level", id);
		SceneManager.LoadScene("Level_" + id);
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
