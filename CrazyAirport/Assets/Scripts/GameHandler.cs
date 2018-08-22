﻿using System.Collections;
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
	[Range(20, 120)]
	private float minFOW = 30;
	[SerializeField]
	[Range(70, 150)]
	private float maxFOW = 70;
	[SerializeField]
	private Map[] map;
	[SerializeField]
	private float showEndsreenDelay = 3;
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
	[SerializeField]
	private GameObject playerLoseBP;
	[SerializeField]
	private GameObject playerChangeCP;
	[SerializeField]
	private Text playerChangeCPText;
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
	#region camera veriables
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
	private bool closeUpScene = false;
	private float closeUpSpeed = 8;
	private float closeUpZoom = 0.07f;
	private float zoomDistance = 1f;
	private float closeUpMoveSpeed = 1.75f;
	#endregion
	#region particle variables
	[SerializeField]
	private ParticleSystem sparksPS;
	[SerializeField]
	private ParticleSystem cleanPS;
	float cleanYRot = 0;
	#endregion
	#region sound variables
	[Header("Sound", order = 2)]
	[SerializeField]
	private AudioClip constructionClip;
	[SerializeField]
	private AudioClip cleaningClip;
	[SerializeField]
	private AudioClip wrongPlacedClip;
	[SerializeField]
	private AudioClip successClip;
	[SerializeField]
	private AudioClip failClip;
	private AudioSource audioSource;
	private bool allowSound;
	public bool AllowSound
	{
		get
		{
			return allowSound;
		}

		set
		{
			allowSound = value;
		}
	}
	#endregion

	#region Setup Functions
	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		GameSetup();
		SetUpMap();
	}

	private void GameSetup()
	{
		cardObject.SetActive(false);
		waitForContinue = true;
		playerGainPointsGO.SetActive(false);
		playerLoseBP.SetActive(false);
		playerChangeCP.SetActive(false);
		gameOverScreen.SetActive(false);
		camTransform = camControl.gameObject.transform;
		LoadCameraSettings();
		playerPointsText.text = "0";
		RefreshInteractionPoints();
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
	#endregion
	#region Communication Functions
	public void ReachedLevelOne()
	{
		cardMan.ReachedLevelOne();
	}

	public void ResetGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void FinishedTurn()
	{
		if (gameEnd || waitForContinue) return;
		waitForContinue = true;
		planeMan.NextRound();
	}

	public void ReadyToContinue()
	{
		if(!gameEnd) StartCoroutine(PlayPointGain());
	}

	public void PlayCrashCloseUp(Vector3 pos, Vector3 dir)
	{
		gameEnd = true;
		string currentLevel = SceneManager.GetActiveScene().name;
		int highScore = PlayerPrefs.GetInt(currentLevel);
		if (playerPoints > highScore) PlayerPrefs.SetInt(currentLevel, playerPoints);
		SaveCameraSettings();
		closeUpScene = true;
		StartCoroutine(PlayCloseUpScene(pos, dir));
	}

	public void EndGame()
	{
		StartCoroutine(ShowEndScreenAfterTime());
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void BackToMainMenu()
	{
		SceneManager.LoadScene("Start");
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

	public void MissionComplete()
	{
		pointsThisRound += pointsPerMission;
	}

	public void PlaneOutOfMap(int speed)
	{
		pointsThisRound -= pointsLoseFactor * speed;
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
			StartCoroutine(PlayerChagenCP(false));
			return true;
		}
		else return false;
	}

	public void InteractionPlaneReset()
	{
		currentPCP++;
		StartCoroutine(PlayerChagenCP(true));
	}

	public void DeactivatedPark(bool deactivate)
	{
		if (deactivate) pointsPerMission--;
		else pointsPerMission++;
	}
	#endregion
	#region Camera Functions
	private void OnMouseDown()
	{
		if (gameEnd) return;
		if (!IsPointerOverUIObject())
		{
			if (cameraCoroutine == null && camControl.CamRotationAllowed)
			{
				currentCam = camControl.GetCurrentCamera();
				cameraCoroutine = StartCoroutine(RotateCamera());
			}
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

	private void ZoomCamera()
	{
		float currentDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
		float distanceChange = touchDistance - currentDistance;
		currentCam.fieldOfView = Mathf.Clamp(currentCam.fieldOfView + distanceChange * Time.deltaTime * zoomFactor, 30, 70);
		touchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
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

	public void SaveCameraSettings()
	{
		if (closeUpScene) return;
		PlayerPrefs.SetFloat("YRot", yRot);
		PlayerPrefs.SetFloat("XRot", xRot);
		PlayerPrefs.SetFloat("Zoom", currentCam.fieldOfView);
		PlayerPrefs.SetFloat("CamZPos", currentCam.transform.localPosition.z);
	}

	private void LoadCameraSettings()
	{
		xRot = PlayerPrefs.GetFloat("XRot");
		yRot = PlayerPrefs.GetFloat("YRot");
		currentCam = camControl.GetCurrentCamera();
		float tempFOW = PlayerPrefs.GetFloat("Zoom");
		currentCam.fieldOfView = tempFOW > minFOW ? maxFOW : tempFOW;
		Vector3 newCamPos = new Vector3(currentCam.transform.localPosition.x, currentCam.transform.localPosition.y, PlayerPrefs.GetFloat("CamZPos"));
		currentCam.transform.localPosition = newCamPos;
		camTransform.rotation = Quaternion.Euler(xRot, -yRot, 0);
		SetCurrentCamPos();
	}

	private IEnumerator PlayCloseUpScene(Vector3 pos, Vector3 dir)
	{
		currentCam = camControl.GetCurrentCamera();

		if (!camControl.CamRotationAllowed)
		{
			Vector3 endPos = new Vector3(pos.x, currentCam.transform.position.y, pos.z);
			Vector3 movement = (endPos - currentCam.transform.position).normalized;

			while (Vector3.Distance(currentCam.transform.position, endPos) > 0 || currentCam.orthographicSize > 2)
			{
				Vector3 tickMove = movement * Time.deltaTime * closeUpSpeed;

				if (tickMove.magnitude > Vector3.Distance(currentCam.transform.position, endPos))
				{
					currentCam.transform.position = endPos;
					currentCam.orthographicSize = Mathf.Clamp(currentCam.orthographicSize - closeUpZoom, 2, 4);
				}
				else
				{
					currentCam.transform.position += tickMove;
					currentCam.orthographicSize = Mathf.Clamp(currentCam.orthographicSize - closeUpZoom, 2, 4);
				}
				yield return null;
			}
		}
		else
		{
			float xPos = 0, yPos = 3, zPos = 0;
			bool positionX = true, positionZ = true, positionY = true;
			while (positionX || positionZ || positionY)
			{


				float yDis = pos.y - camTransform.position.y;
				if (yDis < -3)
				{
					if (positionY) positionY = false;
					yPos = 3;
				}
				else
				{
					yPos = camTransform.position.y + yDis * Time.deltaTime;
				}

				if (dir.x < -0.1f || dir.x > 0.1f)
				{
					float xDis = pos.x - camTransform.position.x;
					if (Mathf.Abs(xDis) > 0) xPos = camTransform.position.x + xDis * Time.deltaTime * closeUpMoveSpeed;
					else
					{
						if (positionX) positionX = false;
						xPos = pos.x;
					}

					float zDis = pos.z - camTransform.position.z;
					if (Mathf.Abs(zDis) > zoomDistance)
					{
						if (zDis > zoomDistance) zPos = Mathf.Clamp(camTransform.position.z + zDis * Time.deltaTime * closeUpMoveSpeed, camTransform.position.z, pos.z - zoomDistance);
						else zPos = Mathf.Clamp(camTransform.position.z + zDis * Time.deltaTime * closeUpMoveSpeed, pos.z + zoomDistance, camTransform.position.z);
					}
					else if (Mathf.Abs(zDis) < zoomDistance)
					{
						if (zDis > 0) zPos = Mathf.Clamp(camTransform.position.z - zDis * Time.deltaTime * closeUpMoveSpeed, pos.z - zoomDistance, camTransform.position.z);
						else zPos = Mathf.Clamp(camTransform.position.z - zDis * Time.deltaTime * closeUpMoveSpeed, camTransform.position.z, pos.z + zoomDistance);
					}
					else
					{
						if (positionZ) positionZ = false;
						zPos = camTransform.position.z;
					}
				}
				else
				{
					float xDis = pos.x - camTransform.position.x;
					if (Mathf.Abs(xDis) > zoomDistance)
					{
						if (xDis > zoomDistance) xPos = Mathf.Clamp(camTransform.position.x + xDis * Time.deltaTime * closeUpMoveSpeed, camTransform.position.x, pos.x - zoomDistance);
						else xPos = Mathf.Clamp(camTransform.position.x + xDis * Time.deltaTime * closeUpMoveSpeed, pos.x + zoomDistance, camTransform.position.x);
					}
					else if (Mathf.Abs(xDis) < zoomDistance)
					{
						if (xDis > 0) xPos = Mathf.Clamp(camTransform.position.x - xDis * Time.deltaTime * closeUpMoveSpeed, pos.x - zoomDistance, camTransform.position.x);
						else xPos = Mathf.Clamp(camTransform.position.x - xDis * Time.deltaTime * closeUpMoveSpeed, camTransform.position.x, pos.x + zoomDistance);
					}
					else
					{
						if (positionX) positionX = false;
						xPos = camTransform.position.x;
					}

					float zDis = pos.z - camTransform.position.z;
					if (Mathf.Abs(zDis) > 0) zPos = camTransform.position.z + zDis * Time.deltaTime * closeUpMoveSpeed;
					else
					{
						if (positionZ) positionZ = false;
						zPos = pos.z;
					}

				}
				camTransform.position = new Vector3(xPos, yPos, zPos);

				currentCam.fieldOfView = Mathf.Clamp(currentCam.fieldOfView - 0.3f, 30, 70);
				currentCam.transform.rotation = Quaternion.RotateTowards(currentCam.transform.rotation, Quaternion.LookRotation(pos - currentCam.transform.position), Time.deltaTime * 15);
				if (Vector3.Dot(camTransform.forward, dir) > 0.05f) camTransform.RotateAround(pos, Vector3.up, Time.deltaTime * 50);
				else if (Vector3.Dot(camTransform.forward, dir) < -0.05f) camTransform.RotateAround(pos, Vector3.up, -Time.deltaTime * 50);

				yield return null;
			}
		}

	}
	#endregion
	#region Build & Clean Functions
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
		if (checkTileMapCoroutine != null) StopCoroutine(checkTileMapCoroutine);
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
		if (AllowSound)
		{
			audioSource.clip = wrongPlacedClip;
			audioSource.Play();
		}
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
				if (AllowSound)
				{
					audioSource.clip = constructionClip;
					audioSource.Play();
				}
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
				StartCoroutine(PlayerLoseBP());
				return true;
			}
		}
		HighlightBuildableTiles(false);
		if (AllowSound)
		{
			audioSource.clip = wrongPlacedClip;
			audioSource.Play();
		}
		return false;
	}
	#endregion
	#region Highlight Functions
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
					if (AllowSound)
					{
						audioSource.clip = cleaningClip;
						audioSource.Play();
					}
					cleanPS.transform.Rotate(cleanPS.transform.up, -cleanYRot);
					cleanYRot = camControl.transform.rotation.eulerAngles.y;
					PlayCleanParticle(mapTile.transform.position);
					mapTile.CleanUpField();
					return true;
				}
			}
			if (AllowSound)
			{
				audioSource.clip = wrongPlacedClip;
				audioSource.Play();
			}
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
					if (AllowSound)
					{
						audioSource.clip = cleaningClip;
						audioSource.Play();
					}
					PlayCleanParticle(mapTile.transform.position);
					mapTile.CleanUpField();
					return true;
				}
			}
			if (AllowSound)
			{
				audioSource.clip = wrongPlacedClip;
				audioSource.Play();
			}
			return false;
		}
	}
	#endregion
	#region UI Functions
	private void RefreshInteractionPoints()
	{
		currentBBP = maxBBP;
		currentPCP = maxPCP;
		buildPointsText.text = currentBBP.ToString();
		controlPointsText.text = currentPCP.ToString();
	}

	private IEnumerator ShowEndScreenAfterTime()
	{
		yield return new WaitForSeconds(showEndsreenDelay);
		gameOverScreen.SetActive(true);
	}

	private IEnumerator PlayPointGain()
	{
		if (pointsThisRound != 0)
		{
			if (pointsThisRound > 0)
			{
				if (AllowSound)
				{
					audioSource.clip = successClip;
					audioSource.Play();
				}
				playerGainPointsText.color = Color.green;
				playerGainPointsText.text = "+" + pointsThisRound.ToString();
			}
			else
			{

				if (AllowSound)
				{
					audioSource.clip = failClip;
					audioSource.Play();
				}
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

	private IEnumerator PlayerLoseBP()
	{
		playerLoseBP.SetActive(true);
		yield return new WaitForSeconds(0.25f);
		playerLoseBP.SetActive(false);
		buildPointsText.text = currentBBP.ToString();
	}

	private IEnumerator PlayerChagenCP(bool gain)
	{
		if (gain)
		{
			playerChangeCPText.color = Color.blue;
			playerChangeCPText.text = "+1";
		}
		else
		{
			playerChangeCPText.color = Color.red;
			playerChangeCPText.text = "-1";
		}
		playerChangeCP.SetActive(true);
		yield return new WaitForSeconds(0.25f);
		playerChangeCP.SetActive(false);
		controlPointsText.text = currentPCP.ToString();
	}
	#endregion

	private bool IsPointerOverUIObject()
	{
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}
}