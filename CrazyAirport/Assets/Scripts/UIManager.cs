using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[SerializeField]
	private float showEndscreenDelay = 1.5f;
	[SerializeField]
	private GameObject nuke;
	[SerializeField]
	private GameObject gameOverScreen;
	[SerializeField]
	private GameObject settingsScreen;
	[SerializeField]
	private Text playerPointsText;
	[SerializeField]
	private GameObject playerGainPointsGO;
	[SerializeField]
	private Text playerGainPointsText;
	[SerializeField]
	private Text buildPointsText;
	[SerializeField]
	private GameObject playerLoseBP;
	[SerializeField]
	private Text controlPointsText;
	[SerializeField]
	private Text playerChangeCPText;
	[SerializeField]
	private GameObject playerChangeCP;
	[SerializeField]
	private AudioSource gainPointsAudio;
	[SerializeField]
	private AudioClip successClip;
	[SerializeField]
	private AudioClip failClip;

	private bool allowSound;
	public bool AllowSound
	{
		get
		{
			return allowSound;
		}

		set
		{
			allowSound = value;
		}
	}
	private bool playResetAnimation;
	public bool PlayResetAnimation
	{
		get
		{
			return playResetAnimation;
		}

		set
		{
			playResetAnimation = value;
		}
	}

	void Start()
	{
		nuke.SetActive(false);
		gameOverScreen.SetActive(false);
		settingsScreen.SetActive(false);
		playerGainPointsGO.SetActive(false);
		playerLoseBP.SetActive(false);
		playerChangeCP.SetActive(false);
		playerPointsText.text = "0";
	}

	public void GameOver()
	{
		StartCoroutine(ShowEndScreenAfterTime());
	}

	public void ResetGame()
	{
		if (PlayResetAnimation) StartCoroutine(PlayReset());
		else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void SaveHighScore(int playerPoints)
	{
		string currentLevel = SceneManager.GetActiveScene().name;
		int highScore = PlayerPrefs.GetInt(currentLevel);
		if (playerPoints > highScore) PlayerPrefs.SetInt(currentLevel, playerPoints);
	}

	public void BackToMainMenu()
	{
		PlayerPrefs.SetInt("MainMenu", 1);
		SceneManager.LoadScene("Start");
	}

	public void BackToLevels()
	{
		PlayerPrefs.SetInt("MainMenu", 2);
		SceneManager.LoadScene("Start");
	}

	public void RefreshInteractionPoints(int currentBBP, int currentPCP)
	{
		buildPointsText.text = currentBBP.ToString();
		controlPointsText.text = currentPCP.ToString();
	}

	public void LoseBuildPoints(int leftBBP)
	{
		StartCoroutine(PlayerLoseBP(leftBBP));
	}

	public void ChangeControlPoints(bool gain, int leftPCP)
	{
		StartCoroutine(PlayerChagenCP(gain, leftPCP));
	}

	public void GainPoints(int pointsThisRound, int finalPoints)
	{
		StartCoroutine(PlayerGainPoints(pointsThisRound, finalPoints));
	}

	private IEnumerator ShowEndScreenAfterTime()
	{
		yield return new WaitForSeconds(showEndscreenDelay);
		gameOverScreen.SetActive(true);
	}

	private IEnumerator PlayReset()
	{
		settingsScreen.SetActive(false);
		gameOverScreen.SetActive(false);
		yield return new WaitForSeconds(1);
		nuke.SetActive(true);
		yield return new WaitForSeconds(5);
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	private IEnumerator PlayerLoseBP(int leftBBP)
	{
		playerLoseBP.SetActive(true);
		yield return new WaitForSeconds(0.25f);
		playerLoseBP.SetActive(false);
		buildPointsText.text = leftBBP.ToString();
	}

	private IEnumerator PlayerChagenCP(bool gain, int leftPCP)
	{
		if (gain)
		{
			playerChangeCPText.color = Color.blue;
			playerChangeCPText.text = "+1";
		}
		else
		{
			playerChangeCPText.color = Color.red;
			playerChangeCPText.text = "-1";
		}
		playerChangeCP.SetActive(true);
		yield return new WaitForSeconds(0.25f);
		playerChangeCP.SetActive(false);
		controlPointsText.text = leftPCP.ToString();
	}

	private IEnumerator PlayerGainPoints(int pointsThisRound, int finalPoints)
	{
		if (pointsThisRound != 0)
		{
			if (pointsThisRound > 0)
			{
				if (AllowSound)
				{
					gainPointsAudio.clip = successClip;
					gainPointsAudio.Play();
				}
				playerGainPointsText.color = Color.green;
				playerGainPointsText.text = "+" + pointsThisRound.ToString();
			}
			else
			{

				if (AllowSound)
				{
					gainPointsAudio.clip = failClip;
					gainPointsAudio.Play();
				}
				playerGainPointsText.color = Color.red;
				playerGainPointsText.text = "-" + pointsThisRound.ToString();
			}

			playerGainPointsGO.SetActive(true);
			yield return new WaitForSeconds(0.3f);
			playerGainPointsGO.SetActive(false);
			playerPointsText.text = finalPoints.ToString();
			pointsThisRound = 0;
		}
	}
}
