using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameHandler : MonoBehaviour
{
	[System.Serializable]
	public class Map
	{
		public MapTile[] tiles;
	}

	public enum BuildingType { Park, Stop, Start, Land, Build, Card, Clean, Control, Road, None }
	public enum Buildable { None, Road, Building }

	#region setup variables
	[Header("Setup", order = 2)]
	[SerializeField]
	private Text versionText;
	[SerializeField]
	private GameObject cardObject;
	[SerializeField]
	private CameraController camControl;
	[SerializeField]
	private LayerMask ignoreMask;
	[SerializeField]
	private CardManager cardMan;
	[SerializeField]
	private PlaneManager planeMan;
	[SerializeField]
	private float camYRotSpeed = 2.5f;
	[SerializeField]
	private float camXRotSpeed = 2.5f;
	[SerializeField]
	private float xCampDegree = 35;
	[SerializeField]
	private float zoomFactor = 5;
	[SerializeField]
	private Map[] map;
	#endregion
	#region ui variables
	[Header("UI", order = 2)]
	[SerializeField]
	private GameObject gameOverScreen;
	[SerializeField]
	private Text buildPointsText;
	[SerializeField]
	private Text controlPointsText;
	[SerializeField]
	private Text playerPointsText;
	[SerializeField]
	private GameObject playerGainPointsGO;
	[SerializeField]
	private Text playerGainPointsText;
	#endregion
	#region player interaction variables
	[Header("Player Interaction", order = 2)]
	[SerializeField]
	private int maxPCP = 1; // PCP = Plane Control Point
	private int currentPCP;
	[SerializeField]
	private int maxBBP = 1; // BBP = Building Build Point
	private int currentBBP;
	private int playerPoints = 0;
	[SerializeField]
	private int pointsLoseFactor = 2;
	#endregion
	#region building variables
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
	#endregion
	#region gameplay variables
	private int gamePoints;
	private int pointsPerMission = 1;
	private Vector2 mousePos;
	private float yRot = 0;
	private float xRot = 0;
	private float autoZoomFactor = 1.5f;
	private float defaultCamDistance = -5f;
	private float maxYDegree = 45;
	private Transform camTransform;
	private Camera currentCam;
	private bool gameEnd = false;
	private bool waitForContinue = true;
	private Coroutine checkTileMapCoroutine;
	private Coroutine cameraCoroutine;
	private MapTile highlightedTile;
	private int[][] buildableTiles;
	private float touchDistance = 0;
	private bool zoom = false;
	private int pointsThisRound = 0;
	private BuildCard cardInHand;
	private CleaningCard tokenInHand;
	#endregion
	#region particle
	[SerializeField]
	private ParticleSystem sparksPS;
	[SerializeField]
	private ParticleSystem cleanPS;
	float cleanYRot = 0;
	#endregion
	#region sound
	[Header("Sound", order = 2)]
	private AudioSource audioSource;
	[SerializeField]
	private AudioClip constructionClip;
	[SerializeField]
	private AudioClip cleaningClip;
	[SerializeField]
	private AudioClip wrongPlacedClip;
	#endregion

	private void Start()
	{
		cardObject.SetActive(false);
		versionText.text = "V" + Application.version;
		waitForContinue = true;
		playerGainPointsGO.SetActive(false);
		gameOverScreen.SetActive(false);
		audioSource = GetComponent<AudioSource>();
		GameSetup();
		SetUpMap();
	}

	public void ReachedLevelOne()
	{
		cardMan.ReachedLevelOne();
	}

	private void PlayBuildParticle(Vector3 pos)
	{
		sparksPS.transform.position = new Vector3(pos.x, sparksPS.transform.position.y, pos.z);
		sparksPS.Play();
	}

	private void PlayCleanParticle(Vector3 pos)
	{
		cleanPS.transform.position = new Vector3(pos.x, cleanPS.transform.position.y, pos.z);
		cleanPS.transform.Rotate(cleanPS.transform.up, cleanYRot);
		cleanPS.Play();
	}

	private void SetUpMap()
	{
		buildableTiles = new int[map.Length][];
		int rowID = 0;
		foreach (Map row in map)
		{
			buildableTiles[rowID] = new int[row.tiles.Length];
			int tileID = 0;
			foreach (MapTile tile in row.tiles)
			{
				MapTile.BuildStatus status = tile.TileStatus;
				if (status == MapTile.BuildStatus.Building || status == MapTile.BuildStatus.Start)
				{
					buildableTiles[rowID][tileID] = 12;
				}
				else if (status == MapTile.BuildStatus.Road)
				{
					buildableTiles[rowID][tileID] = 11;
				}
				else
				{
					buildableTiles[rowID][tileID] = 0;
				}
				tileID++;
			}
			rowID++;
		}

		for (int i = 0; i < buildableTiles.Length; i++)
		{
			for (int j = 0; j < buildableTiles[i].Length; j++)
			{
				int type = buildableTiles[i][j];
				if (type > 10) CheckAndSetAroundTiles(i, j, type % 10);
			}
		}
	}

	private void CheckAndSetAroundTiles(int row, int tile, int type)
	{
		if (row - 1 >= 0)
		{
			int value = buildableTiles[row - 1][tile];
			if (value < 3 && value != type) buildableTiles[row - 1][tile] += type;
		}
		if (row + 1 < buildableTiles.Length)
		{
			int value = buildableTiles[row + 1][tile];
			if (value < 3 && value != type) buildableTiles[row + 1][tile] += type;
		}
		if (tile - 1 >= 0)
		{
			int value = buildableTiles[row][tile - 1];
			if (value < 3 && value != type) buildableTiles[row][tile - 1] += type;
		}
		if (tile + 1 < buildableTiles[row].Length)
		{
			int value = buildableTiles[row][tile + 1];
			if (value < 3 && value != type) buildableTiles[row][tile + 1] += type;
		}
	}

	// Check if the Type of Building can be build at the spot
	public bool TryToBuildAt(Vector2 pos, BuildingType type, int buildID)
	{
		ResetHighLight(type);
		if(checkTileMapCoroutine != null) StopCoroutine(checkTileMapCoroutine);
		Camera cam = camControl.GetCurrentCamera().GetComponent<Camera>();
		Vector3 worldPoint;
		RaycastHit hit;

		if (camControl.CamRotationAllowed)
		{
			worldPoint = cam.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 250));
			if (Physics.Raycast(cam.transform.position, worldPoint, out hit, 10, ignoreMask))
			{
				return BuildBuilding(hit, type, buildID);
			}
		}
		else
		{
			Ray ray = cam.ScreenPointToRay(new Vector3(pos.x, pos.y, 250));
			//Debug.DrawRay(ray.origin, cam.transform.forward, Color.red, 10);
			if (Physics.Raycast(ray.origin, cam.transform.forward, out hit, 10, ignoreMask))
			{
				return BuildBuilding(hit, type, buildID);
			}
		}
		// if no hit
		HighlightBuildableTiles(false);
		audioSource.clip = wrongPlacedClip;
		audioSource.Play();
		return false;
	}

	private bool BuildBuilding(RaycastHit hit, BuildingType type, int buildID)
	{
		MapTile mapTile = hit.collider.gameObject.GetComponent<MapTile>();
		if (mapTile != null)
		{
			int z = (int)mapTile.transform.position.z;
			int x = (int)mapTile.transform.position.x;
			if (mapTile.TileStatus == MapTile.BuildStatus.Empty && mapTile.IsBuildable)
			{
				audioSource.clip = constructionClip;
				audioSource.Play();
				mapTile.DeactivateGroundMesh();
				Transform buildingParent = mapTile.gameObject.transform;
				PlayBuildParticle(buildingParent.position);
				switch (type)
				{
					case BuildingType.Road:
						Instantiate(road, buildingParent);
						mapTile.TileStatus = MapTile.BuildStatus.Road;
						buildableTiles[z][x] = 11;
						CheckAndSetAroundTiles(z, x, 1);
						break;
					case BuildingType.Start:
						Instantiate(startingBuildings[buildID], buildingParent);
						mapTile.TileStatus = MapTile.BuildStatus.Start;
						buildableTiles[z][x] = 12;
						CheckAndSetAroundTiles(z, x, 2);
						break;
					case BuildingType.Stop:
						Instantiate(stopBuildings[buildID], buildingParent);
						mapTile.TileStatus = MapTile.BuildStatus.Stop;
						buildableTiles[z][x] = 12;
						CheckAndSetAroundTiles(z, x, 2);
						break;
					case BuildingType.Land:
						Instantiate(landingBuildings[buildID], buildingParent);
						mapTile.TileStatus = MapTile.BuildStatus.Road;
						buildableTiles[z][x] = 12;
						CheckAndSetAroundTiles(z, x, 2);
						break;
					case BuildingType.Park:
						Instantiate(parkBuildings[buildID], buildingParent);
						mapTile.TileStatus = MapTile.BuildStatus.Park;
						pointsPerMission++;
						buildableTiles[z][x] = 12;
						CheckAndSetAroundTiles(z, x, 2);
						break;
					case BuildingType.Control:
						Instantiate(controlBuildings[buildID], buildingParent);
						mapTile.TileStatus = MapTile.BuildStatus.Building;
						maxPCP++;
						buildableTiles[z][x] = 12;
						CheckAndSetAroundTiles(z, x, 2);
						break;
					case BuildingType.Clean:
						Instantiate(cleaningBuildings[buildID], buildingParent);
						mapTile.TileStatus = MapTile.BuildStatus.Building;
						cardMan.CleansGain++;
						buildableTiles[z][x] = 12;
						CheckAndSetAroundTiles(z, x, 2);
						break;
					case BuildingType.Card:
						Instantiate(cardBuilding, buildingParent);
						mapTile.TileStatus = MapTile.BuildStatus.Building;
						cardMan.CardsGain++;
						buildableTiles[z][x] = 12;
						CheckAndSetAroundTiles(z, x, 2);
						break;
					case BuildingType.Build:
						Instantiate(buildingBuilding, buildingParent);
						mapTile.TileStatus = MapTile.BuildStatus.Building;
						maxBBP++;
						buildableTiles[z][x] = 12;
						CheckAndSetAroundTiles(z, x, 2);
						break;
				}
				HighlightBuildableTiles(false);
				currentBBP--;
				buildPointsText.text = currentBBP.ToString();
				return true;
			}
		}
		HighlightBuildableTiles(false);
		audioSource.clip = wrongPlacedClip;
		audioSource.Play();
		return false;
	}

	private void GameSetup()
	{
		camTransform = camControl.gameObject.transform;
		if (camControl.CamRotationAllowed)
		{
			currentCam = camControl.GetCurrentCamera();
			SetCurrentCamPos();
		}
		playerPointsText.text = "0";
		RefreshInteractionPoints();
	}

	private void RefreshInteractionPoints()
	{
		currentBBP = maxBBP;
		currentPCP = maxPCP;
		buildPointsText.text = currentBBP.ToString();
		controlPointsText.text = currentPCP.ToString();
	}

	public void ResetGame()
	{
		SceneManager.LoadScene("Game");
	}

	public void FinishedTurn()
	{
		if (gameEnd || waitForContinue) return;
		waitForContinue = true;
		planeMan.NextRound();
	}

	public void ReadyToContinue()
	{
		StartCoroutine(PlayPointGain());
	}

	public void EndGame()
	{
		gameEnd = true;
		StartCoroutine(ShowEndScreenAfterTime());
	}

	private IEnumerator ShowEndScreenAfterTime()
	{
		yield return new WaitForSeconds(2.5f);
		gameOverScreen.SetActive(true);
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	private void SupportRound()
	{
		cardMan.NextRound();
		RefreshInteractionPoints();
	}

	public void SupportFinished()
	{
		waitForContinue = false;
	}

	private void OnMouseDown()
	{
		if (!IsPointerOverUIObject())
		{
			if (cameraCoroutine == null && camControl.CamRotationAllowed)
			{
				currentCam = camControl.GetCurrentCamera();
				cameraCoroutine = StartCoroutine(RotateCamera());
			}
		}
	}

	private void ZoomCamera()
	{
		float currentDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
		float distanceChange = touchDistance - currentDistance;
		currentCam.fieldOfView = Mathf.Clamp(currentCam.fieldOfView + distanceChange * Time.deltaTime * zoomFactor, 30, 70);
		//defaultCamDistance = Mathf.Clamp(defaultCamDistance - distanceChange * Time.deltaTime, -8, -2);
		//SetCurrentCamPos();
		touchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
	}

	private bool IsPointerOverUIObject()
	{
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}

	private IEnumerator CheckTileMapHeighlight(BuildingType type)
	{
		Camera cam = camControl.GetCurrentCamera().GetComponent<Camera>();
		Vector3 worldPoint;
		RaycastHit hit;
		while (true)
		{
			Vector2 mouse = Input.mousePosition;
			if (camControl.CamRotationAllowed)
			{
				worldPoint = cam.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, 250));
				if (Physics.Raycast(cam.transform.position, worldPoint, out hit, 10, ignoreMask))
				{
					MapTile mapTile = hit.collider.gameObject.GetComponent<MapTile>();
					if (mapTile != null)
					{
						if (mapTile.TileStatus != MapTile.BuildStatus.Building && mapTile.TileStatus != MapTile.BuildStatus.Road)
						{
							if (mapTile.IsBuildable)
							{
								if (mapTile != highlightedTile)
								{
									HighlightTile(mapTile, type);
								}
							}
							else if (highlightedTile != null) ResetHighLight(type);
						}
						else if (highlightedTile != null) ResetHighLight(type);
					}
					else if (highlightedTile != null) ResetHighLight(type);
				}
				else if (highlightedTile != null) ResetHighLight(type);
			}
			else
			{
				Ray ray = cam.ScreenPointToRay(new Vector3(mouse.x, mouse.y, 250));
				if (Physics.Raycast(ray.origin, cam.transform.forward, out hit, 10, ignoreMask))
				{
					MapTile mapTile = hit.collider.gameObject.GetComponent<MapTile>();
					if (mapTile != null)
					{
						if (mapTile.TileStatus != MapTile.BuildStatus.Building && mapTile.TileStatus != MapTile.BuildStatus.Road)
						{
							if (mapTile.IsBuildable)
							{
								if (mapTile != highlightedTile)
								{
									HighlightTile(mapTile, type);
								}
							}
							else if (highlightedTile != null) ResetHighLight(type);
						}
						else if (highlightedTile != null) ResetHighLight(type);
					}
					else if (highlightedTile != null) ResetHighLight(type);
				}
				else if (highlightedTile != null) ResetHighLight(type);
			}
			yield return null;
		}
	}

	public void HighlightTile(MapTile mapTile, BuildingType type)
	{
		if (highlightedTile != null) highlightedTile.CardOverItem(false);
		highlightedTile = mapTile;
		highlightedTile.CardOverItem(true);
		if (type == BuildingType.None) tokenInHand.SetVisibility(false);
		else cardInHand.SetVisibility(false);
		cardObject.SetActive(true);
		cardObject.transform.position = new Vector3(mapTile.transform.position.x, mapTile.transform.position.y + 0.2f, mapTile.transform.position.z);
	}

	private void ResetHighLight(BuildingType type)
	{
		if (highlightedTile != null)
		{
			if (type == BuildingType.None) tokenInHand.SetVisibility(true);
			else cardInHand.SetVisibility(true);
			highlightedTile.CardOverItem(false);
			highlightedTile = null;
			cardObject.SetActive(false);
		}
	}

	private IEnumerator RotateCamera()
	{
		mousePos = Input.mousePosition;
		while (Input.GetMouseButton(0))
		{
			if (Input.touchCount > 1)
			{
				if (zoom) ZoomCamera();
				else
				{
					touchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
					zoom = true;
				}
			}
			else
			{
				if (zoom) zoom = false;
				Vector2 newPos = Input.mousePosition;
				yRot += (mousePos.x - newPos.x) * Time.deltaTime * camYRotSpeed;
				xRot = Mathf.Clamp(xRot + (mousePos.y - newPos.y) * Time.deltaTime * camXRotSpeed, -xCampDegree, xCampDegree);
				camTransform.rotation = Quaternion.Euler(xRot, -yRot, 0);

				SetCurrentCamPos();
				mousePos = newPos;
			}

			yield return null;
		}
		cameraCoroutine = null;
		zoom = false;
	}

	private void SetCurrentCamPos()
	{
		float zCam = defaultCamDistance + (autoZoomFactor * (1 - (xRot + xCampDegree) / (xCampDegree * 2)));
		float calculatedYRot = CalculateYRot();
		if (calculatedYRot <= maxYDegree)
		{
			zCam -= autoZoomFactor * calculatedYRot / maxYDegree;
		}
		else if (calculatedYRot >= (180 - maxYDegree))
		{
			zCam -= autoZoomFactor * Mathf.Abs(calculatedYRot - 180) / maxYDegree;
		}
		else zCam -= autoZoomFactor;

		Vector3 newCamPos = new Vector3(currentCam.transform.localPosition.x, currentCam.transform.localPosition.y, zCam);
		currentCam.transform.localPosition = newCamPos;
	}

	private float CalculateYRot()
	{
		float value = Mathf.Abs(yRot) % 360;
		if (value > 180) value -= 360;
		return Mathf.Abs(value);
	}

	private void HighlightBuildableTiles(bool highlight, bool road = false)
	{
		if (highlight)
		{
			for (int i = 0; i < buildableTiles.Length; i++)
			{
				for (int j = 0; j < buildableTiles[i].Length; j++)
				{
					if (road)
					{
						if (buildableTiles[i][j] == 1 || buildableTiles[i][j] == 3)
						{
							if (!map[i].tiles[j].PlaneOnField) map[i].tiles[j].IsBuildable = true;
						}
					}
					else
					{
						if (buildableTiles[i][j] < 4)
						{
							if (!map[i].tiles[j].PlaneOnField) map[i].tiles[j].IsBuildable = true;
						}
					}
				}
			}
		}
		else
		{
			for (int i = 0; i < buildableTiles.Length; i++)
			{
				for (int j = 0; j < buildableTiles[i].Length; j++)
				{
					if (map[i].tiles[j].IsBuildable) map[i].tiles[j].IsBuildable = false;
				}
			}
		}
	}

	public bool CheckIfHaveBuildPoints(BuildCard card)
	{
		if (gameEnd || waitForContinue) return false;

		if (currentBBP > 0)
		{
			cardObject.GetComponent<CardObject>().SetUp(card.CardType, card.BuildID);
			cardInHand = card;
			if (card.CardType == BuildingType.Road) HighlightBuildableTiles(true, true);
			else HighlightBuildableTiles(true);
			checkTileMapCoroutine = StartCoroutine(CheckTileMapHeighlight(card.CardType));
			return true;
		}
		else return false;
	}

	public bool CheckIfHaveControlPoints()
	{
		if (gameEnd || waitForContinue) return false;

		if (currentPCP > 0)
		{
			currentPCP--;
			controlPointsText.text = currentPCP.ToString();
			return true;
		}
		else return false;
	}

	public void InteractionPlaneReset()
	{
		currentPCP++;
		controlPointsText.text = currentPCP.ToString();
	}

	public void MissionComplete()
	{
		pointsThisRound += pointsPerMission;
	}

	private IEnumerator PlayPointGain()
	{
		if (pointsThisRound != 0)
		{
			if (pointsThisRound > 0)
			{
				playerGainPointsText.color = Color.green;
				playerGainPointsText.text = "+" + pointsThisRound.ToString();
			}
			else
			{
				playerGainPointsText.color = Color.red;
				playerGainPointsText.text = "-" + pointsThisRound.ToString();
			}

			playerGainPointsGO.SetActive(true);
			yield return new WaitForSeconds(0.3f);
			playerGainPointsGO.SetActive(false);
			playerPoints += pointsThisRound;
			playerPointsText.text = playerPoints.ToString();
			pointsThisRound = 0;
		}

		if (!gameEnd)
		{
			SupportRound();
		}
	}

	public void DeactivatedPark(bool deactivate)
	{
		if (deactivate) pointsPerMission--;
		else pointsPerMission++;
	}

	public void HighlightDirtyFields(CleaningCard token)
	{
		tokenInHand = token;
		for (int i = 0; i < map.Length; i++)
		{
			for (int j = 0; j < map[i].tiles.Length; j++)
			{
				if (!map[i].tiles[j].PlaneOnField && map[i].tiles[j].Dirty) map[i].tiles[j].IsBuildable = true;
			}
		}
		cardObject.GetComponent<CardObject>().SetUp(BuildingType.None);
		checkTileMapCoroutine = StartCoroutine(CheckTileMapHeighlight(BuildingType.None));
	}

	private void DeHighlightDirtyFields()
	{
		for (int i = 0; i < buildableTiles.Length; i++)
		{
			for (int j = 0; j < buildableTiles[i].Length; j++)
			{
				if (map[i].tiles[j].Dirty) map[i].tiles[j].IsBuildable = false;
			}
		}
	}

	public bool TryCleanField(Vector2 pos)
	{
		DeHighlightDirtyFields();
		ResetHighLight(BuildingType.None);
		if (checkTileMapCoroutine != null) StopCoroutine(checkTileMapCoroutine);

		Camera cam = camControl.GetCurrentCamera().GetComponent<Camera>();
		Vector3 worldPoint;
		RaycastHit hit;

		if (camControl.CamRotationAllowed)
		{
			worldPoint = cam.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 250));
			if (Physics.Raycast(cam.transform.position, worldPoint, out hit, 10, ignoreMask))
			{
				MapTile mapTile = hit.collider.gameObject.GetComponent<MapTile>();
				if (mapTile != null && mapTile.Dirty && !mapTile.PlaneOnField)
				{
					audioSource.clip = cleaningClip;
					audioSource.Play();
					cleanPS.transform.Rotate(cleanPS.transform.up, -cleanYRot);
					cleanYRot = camControl.transform.rotation.eulerAngles.y;
					PlayCleanParticle(mapTile.transform.position);
					mapTile.CleanUpField();
					return true;
				}
			}
			audioSource.clip = wrongPlacedClip;
			audioSource.Play();
			return false;
		}
		else
		{
			Ray ray = cam.ScreenPointToRay(new Vector3(pos.x, pos.y, 250));
			//Debug.DrawRay(ray.origin, cam.transform.forward, Color.red, 10);
			if (Physics.Raycast(ray.origin, cam.transform.forward, out hit, 10, ignoreMask))
			{
				MapTile mapTile = hit.collider.gameObject.GetComponent<MapTile>();
				if (mapTile != null && mapTile.Dirty)
				{
					audioSource.clip = cleaningClip;
					audioSource.Play();
					PlayCleanParticle(mapTile.transform.position);
					mapTile.CleanUpField();
					return true;
				}
			}
			audioSource.clip = wrongPlacedClip;
			audioSource.Play();
			return false;
		}
	}

	public void PlaneOutOfMap(int speed)
	{
		pointsThisRound -= pointsLoseFactor * speed;
	}
}