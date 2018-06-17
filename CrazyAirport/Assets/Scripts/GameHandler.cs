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
	[SerializeField]
	private float camYRotSpeed = 2.5f;
	[SerializeField]
	private float camXRotSpeed = 2.5f;
	[SerializeField]
	private float xCampDegree = 35;

	[Header("Player Interaction", order = 2)]
	[SerializeField]
	private int maxPCP = 1; // PCP = Plane Control Point
	private int currentPCP;
	[SerializeField]
	private int maxBCP = 1; // BCP = Building Clean Point
	private int currentBCP;
	[SerializeField]
	private int maxBBP = 1; // BBP = Building Build Point
	private int currentBBP;
	private int playerPoints = 0;

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

	private Vector2 mousePos;
	float yRot = 0;
	float xRot = 0;
	private Transform camTransform;
	private void Start()
	{
		GameSetup();
	}

	// Check if the Type of Building can be build at the spot
	public bool TryToBuildAt(Vector2 pos, BuildingType type, int buildID)
	{
		Camera cam = camControl.GetCurrentCamera().GetComponent<Camera>();
		Vector3 worldPoint;
		RaycastHit hit;

		if (camControl.CamRotationAllowed)
		{
			worldPoint = cam.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 250));
			if (Physics.Raycast(cam.transform.position, worldPoint, out hit, 10))
			{
				return BuildBuilding(hit, type, buildID);
			}
		}
		else
		{
			Ray ray = cam.ScreenPointToRay(new Vector3(pos.x, pos.y, 250));
			//Debug.DrawRay(ray.origin, cam.transform.forward, Color.red, 10);
			if (Physics.Raycast(ray.origin, cam.transform.forward, out hit, 10))
			{
				return BuildBuilding(hit, type, buildID);
			}
		}
		return false;
	}

	private bool BuildBuilding(RaycastHit hit, BuildingType type, int buildID)
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
				return true;
			}
		}
		return false;
	}

	private void GameSetup()
	{
		camTransform = camControl.gameObject.transform;
		currentBBP = maxBBP;
		currentBCP = maxBCP;
		currentPCP = maxPCP;
	}

	public void ResetGame()
	{
		SceneManager.LoadScene("Game");
	}

	public void FinishedTurn()
	{
		cardMan.NextRound();
	}

	private void SupportRound()
	{

	}

	public void MapTileClicked()
	{
		if (camControl.CamRotationAllowed) StartCoroutine(RotateCamera());
	}

	private IEnumerator RotateCamera()
	{
		mousePos = Input.mousePosition;
		while (Input.GetMouseButton(0))
		{
			Vector2 newPos = Input.mousePosition;
			yRot += (mousePos.x - newPos.x) * Time.deltaTime * camYRotSpeed;
			//camTransform.Rotate(camTransform.up, -yRot);
			xRot = Mathf.Clamp(xRot + (mousePos.y - newPos.y) * Time.deltaTime * camXRotSpeed, -xCampDegree, xCampDegree);

			camTransform.rotation = Quaternion.Euler(xRot, -yRot, 0);
			//camTransform.Rotate(camTransform.right, xRot);
			mousePos = newPos;
			yield return null;
		}
	}

	public bool TryToCleanField()
	{
		if (currentBCP > 0)
		{
			currentBCP--;
			return true;
		}
		else return false;
	}
}