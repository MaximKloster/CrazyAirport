using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
	[SerializeField]
	private GameObject settingsMenu;
	private bool openMenu = false;
	
	void Start()
	{
		settingsMenu.SetActive(false);
	}
	
	public void Clicked()
	{
		openMenu = !openMenu;
		settingsMenu.SetActive(openMenu);
	}
}
