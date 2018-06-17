using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
	public enum BuildingType { Park, Stop, Start, Land, Build, Card, Clean, Control, Road }

	[Header("Setup", order = 2)]
	[SerializeField]
	private CameraController camControl;
	[SerializeField]
	private CardManager cardMan;

	[Header("Buildings", order = 2)]
	[SerializeField]
	private GameObject[] stopBuildings;
	[SerializeField]
	private GameObject[] controlBuildings;
	[SerializeField]
	private GameObject[] landingBuildings;
	[SerializeField]
	private GameObject[] startingBuildings;
	[SerializeField]
	private GameObject[] parkBuildings;
	[SerializeField]
	private GameObject[] cleaningBuildings;
	[SerializeField]
	private GameObject cardBuilding;
	[SerializeField]
	private GameObject buildingBuilding;
	[SerializeField]
	private GameObject road;



	// Check if the Type of Building can be build at the spot
	public bool TryToBuildAt(Vector2 pos, BuildingType type, int buildID)
	{
		Camera cam = camControl.GetCurrentCamera().GetComponent<Camera>();
		Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 250));
		RaycastHit hit;

		if (Physics.Raycast(cam.transform.position, worldPoint, out hit, 10))
		{
			MapTile mapTile = hit.collider.gameObject.GetComponent<MapTile>();
			if (mapTile != null)
			{
				if (mapTile.TileStatus == MapTile.BuildStatus.Empty)
				{

					// TODO Check here if it is close to a building / street depending on what should be build

					mapTile.TileStatus = MapTile.BuildStatus.Building;
					Transform buildingParent = mapTile.gameObject.transform;

					switch (type)
					{
						case BuildingType.Road:
							Instantiate(road, buildingParent);
							break;
						case BuildingType.Start:
							Instantiate(startingBuildings[buildID], buildingParent);
							break;
						case BuildingType.Stop:
							Instantiate(stopBuildings[buildID], buildingParent);
							break;
						case BuildingType.Land:
							Instantiate(landingBuildings[buildID], buildingParent);
							break;
						case BuildingType.Park:
							Instantiate(parkBuildings[buildID], buildingParent);
							break;
						case BuildingType.Control:
							Instantiate(controlBuildings[buildID], buildingParent);
							break;
						case BuildingType.Clean:
							Instantiate(cleaningBuildings[buildID], buildingParent);
							break;
						case BuildingType.Card:
							Instantiate(cardBuilding, buildingParent);
							break;
						case BuildingType.Build:
							Instantiate(buildingBuilding, buildingParent);
							break;
					}
					//Debug.Log("hit " + hit.collider.gameObject.name.ToString());
					//Debug.DrawRay(cam.transform.position, worldPoint, Color.red, 10);
					return true;
				}
			}
		}
		return false;
	}

	public void ResetGame()
	{
		SceneManager.LoadScene("Game");
	}

	public void FinishedTurn()
	{
		cardMan.NextRound();
	}
}