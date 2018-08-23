using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
	[SerializeField]
	private GameObject nuke;
	[SerializeField]
	private CardManager cardMan;
	[SerializeField]
	private GameHandler gameMaster;
	[SerializeField]
	private PlaneManager planeMan;
	[SerializeField]
	private GameObject settingsMenu;
	[SerializeField]
	private Image musicButton;
	[SerializeField]
	private Image soundButton;
	[SerializeField]
	private Image planeInfoButton;
	[SerializeField]
	private Image nukeSceneButton;
	[SerializeField]
	private Sprite musicOnIcon;
	[SerializeField]
	private Sprite musicOffIcon;
	[SerializeField]
	private Sprite soundOnIcon;
	[SerializeField]
	private Sprite soundOffIcon;
	[SerializeField]
	private Sprite infoOnIcon;
	[SerializeField]
	private Sprite InfoOffIcon;
	[SerializeField]
	private Sprite nukeOnIcon;
	[SerializeField]
	private Sprite nukeOffIcon;
	private bool openMenu = false;
	private bool musicOn = true;
	private bool soundOn = true;
	private bool showPlaneFB = true;
	private bool showNukeReset = false;
	private bool allowMenuOpen = true;

	public bool AllowMenuOpen
	{
		get
		{
			return allowMenuOpen;
		}

		set
		{
			allowMenuOpen = value;
		}
	}

	void Start()
	{
		musicOn = PlayerPrefs.GetInt("Music") > 0;
		soundOn = PlayerPrefs.GetInt("Sound") > 0;
		showPlaneFB = PlayerPrefs.GetInt("PlaneFB") > 0;
		SetMusic();
		SetSound();
		SetPlaneFB();
		settingsMenu.SetActive(false);
	}

	private void SetSound()
	{
		if (soundOn) soundButton.sprite = soundOnIcon;
		else soundButton.sprite = soundOffIcon;
		cardMan.AllowSound = soundOn;
		planeMan.AllowSound = soundOn;
		gameMaster.AllowSound = soundOn;
	}

	private void SetMusic()
	{
		if (musicOn) musicButton.sprite = musicOnIcon;
		else musicButton.sprite = musicOffIcon;
	}

	private void SetPlaneFB()
	{
		if (showPlaneFB) planeInfoButton.sprite = infoOnIcon;
		else planeInfoButton.sprite = InfoOffIcon;
		planeMan.ShowFeedback = showPlaneFB;
	}

	public void MenuOpenClicked()
	{
		if (AllowMenuOpen)
		{
			cardMan.MenuButtonClicked();
			if (!openMenu) gameMaster.SaveCameraSettings();
			openMenu = !openMenu;
			settingsMenu.SetActive(openMenu);
		}
	}

	public void MusicButtonClicked()
	{
		cardMan.MenuButtonClicked();
		musicOn = !musicOn;
		PlayerPrefs.SetInt("Music", musicOn ? 1 : 0);
		SetMusic();
	}

	public void SoundButtonClicked()
	{
		cardMan.MenuButtonClicked();
		soundOn = !soundOn;
		PlayerPrefs.SetInt("Sound", soundOn ? 1 : 0);
		SetSound();
	}

	public void PlanesFBButtonClicked()
	{
		cardMan.MenuButtonClicked();
		showPlaneFB = !showPlaneFB;
		PlayerPrefs.SetInt("PlaneFB", showPlaneFB ? 1 : 0);
		SetPlaneFB();
	}

	public void NukeButtonClicked()
	{
		MenuOpenClicked();
		showNukeReset = !showNukeReset;

		nuke.SetActive(true);
	}
}
