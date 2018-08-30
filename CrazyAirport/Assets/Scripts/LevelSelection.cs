using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelection : MonoBehaviour
{
	public enum SwipeTransform { RIGHT, NONE, LEFT }
	#region Level SetUP
	[Header("Level SetUp", order = 2)]
	[SerializeField]
	private int[] unlockLevelPoints; // 0 = points from level 1 needed to unlock level 2 // 1 = points from level 2 needed to unlock level 3 and so on
	#endregion
	#region Transforms & Objects
	[Header("SetUp Parts", order = 2)]
	[SerializeField]
	private Transform selectedTransform;
	[SerializeField]
	private Transform frontTransform;
	[SerializeField]
	private Transform leftTransform01;
	[SerializeField]
	private Transform leftTransform02;
	[SerializeField]
	private Transform leftTransformBack;
	[SerializeField]
	private Transform rightTransform01;
	[SerializeField]
	private Transform rightTransform02;
	[SerializeField]
	private Transform rightTransformBack;
	[SerializeField]
	private GameObject[] allLevelPrefabs;

	private SwipeTransform swipeTrasform = SwipeTransform.NONE;
	private float frontX;
	private float rightX01;
	private float rightX02;
	private float rightBackX;
	private float leftX01;
	private float leftX02;
	private float leftBackX;
	#endregion
	[SerializeField]
	private Text levelNameText;
	[SerializeField]
	private Text discriptionText;

	private float backScale = 0.65f;
	private float scale02 = 0.7f;
	private float scale01 = 0.85f;
	private float frontScale = 1f;
	private List<Level> allLevels;
	private int selectedLevel;
	private int screenWidth;
	private float moveSpeed = 7.5f;
	AsyncOperation loadingScene;
	private bool isLoading = false;
	private bool soundAllowed = false;

	void Start()
	{
		isLoading = false;
		screenWidth = Screen.width;
		frontX = frontTransform.position.x;
		rightX01 = rightTransform01.position.x;
		rightX02 = rightTransform02.position.x;
		rightBackX = rightTransformBack.position.x;
		leftX01 = leftTransform01.position.x;
		leftX02 = leftTransform02.position.x;
		leftBackX = leftTransformBack.position.x;
		allLevels = new List<Level>();

		for (int i = 0; i < allLevelPrefabs.Length; i++)
		{
			Transform parent = rightTransformBack;
			float newScale = backScale;
			bool hideItems = true;
			switch (i)
			{
				case 0:
					parent = frontTransform;
					newScale = frontScale;
					hideItems = false;
					break;
				case 1:
					parent = rightTransform01;
					newScale = scale01;
					hideItems = false;
					break;
				case 2:
					parent = rightTransform02;
					newScale = scale02;
					hideItems = false;
					break;
			}
			GameObject levelObject = Instantiate(allLevelPrefabs[i], parent);
			Level level = levelObject.GetComponent<Level>();
			Level.LevelPosition levelPos = Level.LevelPosition.CENTER;
			if (i == 0) levelPos = Level.LevelPosition.FIRST;
			else if (i == allLevelPrefabs.Length - 1) levelPos = Level.LevelPosition.LAST;

			level.SetUp(this, newScale, i < 3 ? 1 : 0, levelPos, hideItems);
			level.SoundAllowed = soundAllowed;
			if (i == 0) level.IsLocked = true;
			else if (i == 1) level.IsLocked = false;
			else
			{
				int levelChecking = i - 1;
				int points = PlayerPrefs.GetInt("Level_" + levelChecking);
				if (points >= unlockLevelPoints[levelChecking - 1]) level.IsLocked = false;
				else level.IsLocked = true;
			}
			allLevels.Add(level);
		}

		selectedLevel = 0;
		allLevels[0].Interaction(true);
		ChangeDiscription();
	}

	public void ReleasedSelected(float xPos)
	{
		float factor = xPos / screenWidth;
		if (factor < 0.45f)
		{
			StartCoroutine(ChangePos(false, xPos));
		}
		else if (factor > 0.55f)
		{
			StartCoroutine(ChangePos(true, xPos));
		}
		else
		{
			StartCoroutine(BounceBack(xPos));
		}
	}

	private IEnumerator ChangePos(bool right, float xPos)
	{
		float destination = right ? screenWidth : 0;
		float distance = destination - xPos;
		float newPos = xPos;
		while (newPos != destination)
		{
			float movement = distance * Time.deltaTime * moveSpeed;

			if (Mathf.Abs(movement) > Mathf.Abs(destination - newPos)) newPos = destination;
			else newPos += movement;
			allLevels[selectedLevel].SelectedMove(newPos);
			MovedLevel(newPos);
			yield return null;
		}
		destination = right ? screenWidth + 500 : -500;
		distance = destination - xPos;
		while (right ? newPos < destination : newPos > destination)
		{
			newPos += distance * Time.deltaTime * moveSpeed;
			allLevels[selectedLevel].SelectedMove(newPos);
			yield return null;
		}

		int previousSelected = selectedLevel;
		if (right)
		{
			destination = rightX01;
			selectedLevel--;
		}
		else
		{
			destination = leftX01;
			selectedLevel++;
		}

		for (int i = 0; i < allLevels.Count; i++)
		{
			SetNewDefaults(i);
		}

		ChangeDiscription();
		allLevels[previousSelected].SetParentDirection(right ? rightTransform01 : leftTransform01);
		swipeTrasform = SwipeTransform.NONE;

		distance = destination - newPos;

		while (newPos != destination)
		{
			// Movement
			float movement = distance * Time.deltaTime * moveSpeed;
			if (Mathf.Abs(movement) > Mathf.Abs(destination - newPos)) newPos = destination;
			else newPos += movement;

			// Scale

			float factorDone = 1 - (destination - newPos) / distance;
			float newScale = 1 + (scale01 - 1) * factorDone;

			allLevels[previousSelected].SelectedMove(newPos, newScale);
			yield return null;
		}
		allLevels[selectedLevel].EnableSelection();
	}

	private void SetNewDefaults(int i)
	{
		int slotID = i - selectedLevel;
		switch (slotID)
		{
			case -3:
				allLevels[i].SetDefaults(leftBackX, backScale);
				break;
			case -2:
				allLevels[i].SetDefaults(leftX02, scale02);
				break;
			case -1:
				allLevels[i].SetDefaults(leftX01, scale01);
				break;
			case 0:
				allLevels[i].SetDefaults(frontX, frontScale);
				break;
			case 1:
				allLevels[i].SetDefaults(rightX01, scale01);
				break;
			case 2:
				allLevels[i].SetDefaults(rightX02, scale02);
				break;
			case 3:
				allLevels[i].SetDefaults(rightBackX, backScale);
				break;
		}
	}

	private IEnumerator BounceBack(float xPos)
	{
		float distance = frontX - xPos;
		float newPos = xPos;
		while (newPos != frontX)
		{
			float movement = distance * Time.deltaTime * moveSpeed;
			if (Mathf.Abs(movement) > Mathf.Abs(frontX - newPos)) newPos = frontX;
			else newPos += movement;
			allLevels[selectedLevel].SelectedMove(newPos);
			MovedLevel(newPos);
			yield return null;
		}
	}

	public void MovedLevel(float newXPos)
	{
		float screenFactor = newXPos / screenWidth;
		bool rightSwipe = screenFactor > 0.5f;
		float factorDone = rightSwipe ? (screenFactor - 0.5f) * 2 : 1 - (screenFactor * 2);
		Transform parent = frontTransform;
		allLevels[selectedLevel].SelectedParent(selectedTransform);
		if (newXPos >= 0 && newXPos <= screenWidth)
		{
			for (int i = 0; i < allLevels.Count; i++)
			{
				float visibility = 1;
				bool changeAllowed = true;
				if (i != selectedLevel)
				{
					int slotID = i - selectedLevel;
					float newPos = frontX;
					float newScale = frontScale;
					switch (slotID)
					{
						case -3:
							if (rightSwipe)
							{
								newPos = leftX02;
								newScale = scale02;
								visibility *= factorDone;
								parent = leftTransform02;
							}
							else
							{
								changeAllowed = false;
							}
							break;
						case -2:
							if (rightSwipe)
							{
								newPos = leftX01;
								newScale = scale01;
								parent = leftTransform01;
							}
							else
							{
								newPos = leftBackX;
								newScale = backScale;
								visibility *= (1 - factorDone);
								parent = leftTransformBack;
							}
							break;
						case -1:
							if (rightSwipe)
							{
								newPos = frontX;
								newScale = frontScale;
								parent = frontTransform;
							}
							else
							{
								newPos = leftX02;
								newScale = scale02;
								parent = leftTransform02;
							}
							break;
						case 1:
							if (rightSwipe)
							{
								newPos = rightX02;
								newScale = scale02;
								parent = rightTransform02;
							}
							else
							{
								newPos = frontX;
								newScale = frontScale;
								parent = frontTransform;
							}
							break;
						case 2:
							if (rightSwipe)
							{
								newPos = rightBackX;
								newScale = backScale;
								visibility *= (1 - factorDone);
								parent = rightTransformBack;
							}
							else
							{
								newPos = rightX01;
								newScale = scale01;
								parent = rightTransform01;
							}
							break;
						case 3:
							if (rightSwipe)
							{
								changeAllowed = false;
							}
							else
							{
								newPos = rightX02;
								newScale = scale02;
								visibility *= factorDone;
								parent = rightTransform02;
							}
							break;
						default:
							changeAllowed = false;
							break;
					}

					if (rightSwipe && swipeTrasform != SwipeTransform.RIGHT)
					{
						allLevels[i].SetParentDirection(parent);
					}
					else if (!rightSwipe && swipeTrasform != SwipeTransform.LEFT)
					{
						allLevels[i].SetParentDirection(parent);
					}

					if (changeAllowed) allLevels[i].MoveToNextView(newPos, factorDone, newScale, visibility);
				}
			}
			if (rightSwipe && swipeTrasform != SwipeTransform.RIGHT) swipeTrasform = SwipeTransform.RIGHT;
			else if (!rightSwipe && swipeTrasform != SwipeTransform.LEFT) swipeTrasform = SwipeTransform.LEFT;
		}
	}

	public void PlayButtonClicked()
	{
		isLoading = true;
		int level = selectedLevel;
		PlayerPrefs.SetString("LevelName", allLevels[selectedLevel].LevelName);
		PlayerPrefs.SetString("LevelDescription", allLevels[selectedLevel].Discription);
		loadingScene = SceneManager.LoadSceneAsync("Level_" + level, LoadSceneMode.Single);
		PlayerPrefs.SetInt("Level", level);
		PlayerPrefs.SetInt("LastLevel", level);
		loadingScene.allowSceneActivation = false;
		StartCoroutine(LoadLevel());
	}

	public void WHCButtonClicked()
	{
		if (!isLoading) ;
	}

	private IEnumerator LoadLevel()
	{
		yield return new WaitForSeconds(1.5f);
		loadingScene.allowSceneActivation = true;
	}

	public void ResetAllHighscores()
	{
		int i = 1;
		foreach (Level level in allLevels)
		{
			ShowHighscore highScore = level.gameObject.GetComponent<ShowHighscore>();
			PlayerPrefs.SetInt("Level_" + i, 0);
			highScore.ResetHighscore();
			i++;
		}
	}

	public void ChangeSoundState(bool allowed)
	{
		soundAllowed = allowed;
		if (allLevels != null)
		{
			foreach (Level level in allLevels)
			{
				level.SoundAllowed = allowed;
			}
		}
	}

	private void ChangeDiscription()
	{
		levelNameText.text = allLevels[selectedLevel].LevelName;
		if (allLevels[selectedLevel].IsLocked)
		{
			if (selectedLevel == 0) discriptionText.text = "The Story Mode is at the moment not available";
			else
			{
				int levelChecking = selectedLevel - 2;
				discriptionText.text = "To unlock this Level you need at least a Highscore of "+ unlockLevelPoints[levelChecking]+ " in Level "+ (levelChecking + 1);
			}
		}
		else discriptionText.text = allLevels[selectedLevel].Discription;

	}

	public string GetLevel1Name()
	{
		return allLevels[1].LevelName;
	}

	public string GetLevel1Description()
	{
		return allLevels[1].Discription;
	}
}
