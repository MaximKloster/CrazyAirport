using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowHighscore : MonoBehaviour
{
	[SerializeField]
	private int levelID;
	[SerializeField]
	private Text highScoreText;
	// Use this for initialization
	void Start()
	{
		highScoreText.text = PlayerPrefs.GetInt("Level_"+levelID).ToString();
	}

	public void ResetHighscore()
	{
		highScoreText.text = "0";
	}
}
