using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardObject : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer render;
	[SerializeField]
	private GameObject cardObject;
	[SerializeField]
	private GameObject tokenObject;
	[SerializeField]
	private Material[] allMats;

	public void SetUp(GameHandler.BuildingType cardType, int typeID = 0)
	{
		switch(cardType)
		{
			case GameHandler.BuildingType.Build:
				cardObject.SetActive(true);
				tokenObject.SetActive(false);
				render.material = allMats[6];
				break;
			case GameHandler.BuildingType.Card:
				cardObject.SetActive(true);
				tokenObject.SetActive(false);
				render.material = allMats[8];
				break;
			case GameHandler.BuildingType.Clean:
				cardObject.SetActive(true);
				tokenObject.SetActive(false);
				render.material = allMats[5];
				break;
			case GameHandler.BuildingType.Control:
				cardObject.SetActive(true);
				tokenObject.SetActive(false);
				render.material = allMats[7];
				break;
			case GameHandler.BuildingType.Land:
				cardObject.SetActive(true);
				tokenObject.SetActive(false);
				render.material = allMats[1];
				break;
			case GameHandler.BuildingType.Park:
				cardObject.SetActive(true);
				tokenObject.SetActive(false);
				if (typeID == 0) render.material = allMats[3];
				else render.material = allMats[2];
				break;
			case GameHandler.BuildingType.Stop:
				cardObject.SetActive(true);
				tokenObject.SetActive(false);
				render.material = allMats[4];
				break;
			case GameHandler.BuildingType.Road:
				cardObject.SetActive(true);
				tokenObject.SetActive(false);
				render.material = allMats[0];
				break;
			case GameHandler.BuildingType.Start:
				cardObject.SetActive(true);
				tokenObject.SetActive(false);
				render.material = allMats[9];
				break;
			case GameHandler.BuildingType.None:
				cardObject.SetActive(false);
				tokenObject.SetActive(true);
				break;

		}
	}
}
