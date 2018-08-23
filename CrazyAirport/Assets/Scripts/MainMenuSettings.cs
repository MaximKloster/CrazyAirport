using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSettings : MonoBehaviour
{
	[SerializeField]
	private AudioSource audioSource;
	[SerializeField]
	private Image musicButton;
	[SerializeField]
	private Image soundButton;
	[SerializeField]
	private Sprite musicOnIcon;
	[SerializeField]
	private Sprite musicOffIcon;
	[SerializeField]
	private Sprite soundOnIcon;
	[SerializeField]
	private Sprite soundOffIcon;
	[SerializeField]
	private GameObject settingsMenu;
	[SerializeField]
	private LevelSelection levelSelect;

	private bool menuOpen = false;
	private bool soundOn;
	private bool musicOn;

	void Start()
	{
		musicOn = PlayerPrefs.GetInt("Music") > 0;
		soundOn = PlayerPrefs.GetInt("Sound") > 0;
		SetSoundIcon();
		SetMusicIcon();
		settingsMenu.SetActive(false);
		if(levelSelect != null) levelSelect.ChangeSoundState(soundOn);
	}

	public void SettingsClicked()
	{
		if(soundOn) audioSource.Play();
		menuOpen = !menuOpen;
		settingsMenu.SetActive(menuOpen);
	}

	public void SoundButtonClicked()
	{
		if (soundOn) audioSource.Play();
		soundOn = !soundOn;
		PlayerPrefs.SetInt("Sound", soundOn ? 1 : 0);
		SetSoundIcon();
		levelSelect.ChangeSoundState(soundOn);
	}

	public void MusicButtonClicked()
	{
		if (soundOn) audioSource.Play();
		musicOn = !musicOn;
		PlayerPrefs.SetInt("Music", musicOn ? 1 : 0);
		SetMusicIcon();
	}

	private void SetMusicIcon()
	{
		if (musicOn) musicButton.sprite = musicOnIcon;
		else musicButton.sprite = musicOffIcon;
	}

	private void SetSoundIcon()
	{
		if (soundOn) soundButton.sprite = soundOnIcon;
		else soundButton.sprite = soundOffIcon;
	}

	public void PlayClickSound()
	{
		if (soundOn) audioSource.Play();
	}
}
