using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSetUp : MonoBehaviour
{
	[Header("Prefabs for Setup", order = 2)]
	[SerializeField]
	private GameObject map;
	[SerializeField]
	private GameObject ui;
	[Header("Default Setup", order = 2)]
	[SerializeField]
	private CameraController camController;

	private GameHandler gameHander;
	private CardManager cardManager;
	private SettingsMenu settingsMenu;
	private UIManager uiMan;
	private PlaneManager planeMan;

	private void Awake()
	{
		gameHander = map.GetComponent<GameHandler>();
		planeMan = map.GetComponentInChildren<PlaneManager>();
		cardManager = ui.GetComponent<CardManager>();
		settingsMenu = ui.GetComponent<SettingsMenu>();
		uiMan = ui.GetComponent<UIManager>();
		gameHander.GetSetUpParts(camController, cardManager, settingsMenu, uiMan);
		settingsMenu.GetSetUpParts(gameHander, planeMan, camController);
		cardManager.GetSetUpParts(gameHander);
	}
}
