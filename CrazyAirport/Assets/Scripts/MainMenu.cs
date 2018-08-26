using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	[SerializeField]
	private Text versionText;
	[SerializeField]
	private GameObject loadingAnim;
	[SerializeField]
	private GameObject mainScreen;
	[SerializeField]
	private GameObject levelScreen;
	[SerializeField]
	private GameObject endGamePopUp;
	AsyncOperation loadingScene;
	private bool isLoading = false;

	void Start()
	{
		int mainMenuScreen = PlayerPrefs.GetInt("MainMenu");
		OpenStartScreen(mainMenuScreen);
		versionText.text = "V "+Application.version;
		isLoading = false;
		if(loadingAnim != null) loadingAnim.SetActive(false);
	}

	private void OpenStartScreen(int id)
	{
		endGamePopUp.SetActive(false);
		switch (id)
		{
			case 0:
				mainScreen.SetActive(true);
				levelScreen.SetActive(false);
				break;
			case 1:
				mainScreen.SetActive(true);
				levelScreen.SetActive(false);
				break;
			case 2:
				mainScreen.SetActive(false);
				levelScreen.SetActive(true);
				break;
		}
		PlayerPrefs.SetInt("MainMenu", 0);
	}

	public void OpenLevelScreen()
	{
		if (!isLoading)
		{
			mainScreen.SetActive(false);
			levelScreen.SetActive(true);
		}
	}

	public void OpenMainMenu()
	{
		if (!isLoading)
		{
			mainScreen.SetActive(true);
			levelScreen.SetActive(false);
		}
	}

	public void LoadLastLevel()
	{
		isLoading = true;
		loadingAnim.SetActive(true);
		int lastLevel = PlayerPrefs.GetInt("LastLevel");
		if (lastLevel < 1)
		{
			PlayerPrefs.SetInt("Level", 1);
			PlayerPrefs.SetInt("LastLevel", 1);
			loadingScene = SceneManager.LoadSceneAsync("Level_" + 1, LoadSceneMode.Single);
		}
		else
		{
			PlayerPrefs.SetInt("Level", lastLevel);
			loadingScene = SceneManager.LoadSceneAsync("Level_" + lastLevel, LoadSceneMode.Single);
		}
		loadingScene.allowSceneActivation = false;
		StartCoroutine(LoadScene());
	}

	public void OpenWeeklyChalange()
	{
		if (!isLoading) ;
	}

	public void QuitGameRequest()
	{
		if (!isLoading) OpenEngamePopUp();
	}

	private IEnumerator LoadScene()
	{
		yield return new WaitForSeconds(1.5f);
		loadingScene.allowSceneActivation = true;
	}

	private void OpenEngamePopUp()
	{
		endGamePopUp.SetActive(true);
	}

	public void YesButtonClicked()
	{
		Application.Quit();
	}

	public void NoButtonClicked()
	{
		endGamePopUp.SetActive(false);
	}
}
