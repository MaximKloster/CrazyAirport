﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	[SerializeField]
	private Text versionText;
	[SerializeField]
	private GameObject levelScreen;
	[SerializeField]
	private GameObject loadingAnim;
	AsyncOperation loadingScene;
	private bool isLoading = false;

	void Start()
	{
		versionText.text = "V "+Application.version;
		isLoading = false;
		levelScreen.SetActive(false);
		loadingAnim.SetActive(false);
	}

	public void OpenLevelScreen()
	{
		levelScreen.SetActive(true);
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

	public void QuitGame()
	{
		if(!isLoading) Application.Quit();
	}

	private IEnumerator LoadScene()
	{
		yield return new WaitForSeconds(1.5f);
		loadingScene.allowSceneActivation = true;
	}

	public void MainMenuButtonClicked()
	{
		if (!isLoading) levelScreen.SetActive(false);
	}
}