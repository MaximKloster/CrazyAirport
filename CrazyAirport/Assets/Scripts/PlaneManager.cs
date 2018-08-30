using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneManager : MonoBehaviour
{
	#region setup variables
	[Header("Setup", order = 2)]
	[SerializeField]
	private GameHandler gameMaster;
	[SerializeField]
	private Transform[] allSpawnpoints;
	[SerializeField]
	private Transform[] allStartSpawnpoints;
	[SerializeField]
	private MeshRenderer ampel01;
	[SerializeField]
	private MeshRenderer ampel02;
	[SerializeField]
	private Material[] ampelLights;
	[SerializeField]
	private GameObject[] planePrefab;
	[SerializeField]
	private Vector4 mapBorders;
	[SerializeField]
	[Range(0, 100)]
	private float startplaneSpawnChance = 30;
	[SerializeField]
	private PlaneController.Destination[] possibleDestinations;
	#endregion
	#region gameplay variables
	private List<PlaneController> allPlanes;
	private int[] lastSpawnPoints = new int[4] { -1, -1, -1, -1 }; // check if a plane was last time here started, so to do not use again in next round
	private bool[] startPlaneOnSpawnPoints = new bool[2] { false, false }; // check if a start plane is still on a startpoint
	private int turn = 0;
	public int Turn
	{
		get
		{
			return turn;
		}

		private set
		{
			turn = value;
		}
	}
	private int maxSpawnPlaneSize = 1;
	private int maxSpawnPlaneAmount = 1;
	private int level = 0;
	private bool showFeedback = true;
	public bool ShowFeedback
	{
		get
		{
			return showFeedback;
		}

		set
		{
			showFeedback = value;
			if (allPlanes != null)
			{
				foreach (PlaneController plane in allPlanes)
				{
					plane.ShowMovementFeedback(showFeedback);
				}
			}
		}
	}
	private bool endGame = false;
	private int spawnStartPlane = -1;
	#endregion
	#region sound variables
	[Header("Sound", order = 2)]
	[SerializeField]
	private AudioClip radarClip;
	[SerializeField]
	private AudioClip planeMoveClip;
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
			if (allPlanes != null)
			{
				foreach (PlaneController plane in allPlanes)
				{
					plane.AllowSound = allowSound;
				}
			}
		}
	}
	#endregion

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		Turn = 0;
		level = 0;
		allPlanes = new List<PlaneController>();
	}

	public void NextRound()
	{
		Turn++;
		StartCoroutine(HandleTurn());
	}

	public void ShowMovementFeedback()
	{
		if (endGame) return;
		ShowFeedback = !ShowFeedback;
		foreach (PlaneController plane in allPlanes)
		{
			plane.ShowMovementFeedback(ShowFeedback);
		}
	}

	private void CalculateLevel()
	{
		switch (level)
		{
			case 0:
				if (Turn > 5) // check [5] turns 1 to 5 
				{
					maxSpawnPlaneSize = 2;
					level++;
					gameMaster.NextStage();
				}
				break;
			case 1:
				if (Turn > 10) // check [5] turns 6 to 10 
				{
					maxSpawnPlaneSize = 3;
					level++;
					gameMaster.NextStage();
				}
				break;
			case 2:
				if (Turn > 15) // check turns [5] 11 to 15
				{
					maxSpawnPlaneAmount = 2;
					maxSpawnPlaneSize = 2;
					level++;
					gameMaster.NextStage();
				}
				break;
			case 3:
				if (Turn > 20) // check turns [5] 16 to 20
				{
					maxSpawnPlaneAmount = 2;
					maxSpawnPlaneSize = 3;
					level++;
					gameMaster.NextStage();
				}
				break;
			case 4:
				if (Turn > 25) // check turns [5] 21 to 25
				{
					maxSpawnPlaneAmount = 3;
					maxSpawnPlaneSize = 2;
					level++;
					gameMaster.NextStage();
				}
				break;
			case 5:
				if (Turn > 30) // check turns [5] 26 to 30
				{
					maxSpawnPlaneAmount = 3;
					maxSpawnPlaneSize = 3;
					level++;
					gameMaster.NextStage();
				}
				break;
			case 6:
				if (Turn > 37) // check turns [7] 31 to 37
				{
					maxSpawnPlaneAmount = 4;
					maxSpawnPlaneSize = 2;
					level++;
					gameMaster.NextStage();
				}
				break;
			case 7:
				if (Turn > 45) // check turns [8] 38 to 45
				{
					maxSpawnPlaneAmount = 4;
					maxSpawnPlaneSize = 3;
					level++;
					gameMaster.NextStage();
				}
				break;
		}
	}

	private IEnumerator HandleTurn()
	{
		if (Turn > 1)
		{
			MoveAllPlanes();
			yield return new WaitForSeconds(1.25f);
		}
		SpawnPlane();
		gameMaster.ReadyToContinue();
	}

	public bool TryToRotatePlane()
	{
		int pointsLeft = gameMaster.CheckIfHaveControlPoints();
		if (pointsLeft > 0) return true;
		else if (pointsLeft == 0)
		{
			foreach (PlaneController plane in allPlanes)
			{
				if (!plane.WasRotated()) plane.SetRotationFeedback(true);
			}
			return true;
		}
		return false;
	}

	public bool TryToPlacePlane()
	{
		return gameMaster.CheckIfCanPlacePlane();
	}

	public void ReleasedPlane(int id, bool onStartapult)
	{
		gameMaster.ReleasedPlane(onStartapult);
		if (onStartapult)
		{
			startPlaneOnSpawnPoints[id] = false;
		}
	}

	public Camera GiveCurrentCam()
	{
		return gameMaster.GiveCurrentCam();
	}

	public bool PerspectiveCam()
	{
		return gameMaster.PerspectiveCam();
	}

	public void PlaneIsRotatedToDefault()
	{
		gameMaster.InteractionPlaneReset();
		foreach (PlaneController plane in allPlanes)
		{
			if (!plane.WasRotated()) plane.SetRotationFeedback(false);
		}
	}

	private void MoveAllPlanes()
	{
		if (AllowSound)
		{
			audioSource.clip = planeMoveClip;
			audioSource.Play();
		}
		foreach (PlaneController plane in allPlanes)
		{
			plane.Move();
		}
	}

	// id: 0 = Speed 1 (small plane), 1 = Speed 2 (normal plane), 2 = Speed 3 (fast plane)
	private void SpawnPlane()
	{
		CalculateLevel();
		if (AllowSound)
		{
			audioSource.clip = radarClip;
			audioSource.Play();
		}
		int[] tempSP = new int[4] { -1, -1, -1, -1 };

		for (int i = 0; i < maxSpawnPlaneAmount; i++)
		{
			int sp;

			if (spawnStartPlane > -1)
			{
				switch (spawnStartPlane)
				{
					case 0:
						ampel01.material = ampelLights[0];
						break;
					case 1:
						ampel02.material = ampelLights[0];
						break;
				}

				int planeType;
				planeType = Random.Range(0, maxSpawnPlaneSize);
				GameObject planeObject = Instantiate(planePrefab[planeType], allStartSpawnpoints[spawnStartPlane].position, allStartSpawnpoints[spawnStartPlane].rotation, transform);
				PlaneController plane = planeObject.GetComponent<PlaneController>();
				plane.PlaneSetup(this, mapBorders, ShowFeedback, AllowSound, spawnStartPlane, possibleDestinations, true);
				allPlanes.Add(plane);

				if (startPlaneOnSpawnPoints[spawnStartPlane])
				{
					plane.PlayeExplosion();
					CrashStarted(plane.transform.position, plane.transform.forward);
					StartCoroutine(EndGameAfterTime());
				}
				else
				{
					startPlaneOnSpawnPoints[spawnStartPlane] = true;
					spawnStartPlane = -1;
				}
			}
			else
			{
				do
				{
					sp = Random.Range(0, allSpawnpoints.Length);
				} while (sp == lastSpawnPoints[0] || sp == lastSpawnPoints[1] || sp == lastSpawnPoints[2] || sp == lastSpawnPoints[3] || sp == tempSP[0] || sp == tempSP[1] || sp == tempSP[2] || sp == tempSP[3]);
				tempSP[i] = sp;
				int planeType;
				planeType = Random.Range(0, maxSpawnPlaneSize);
				GameObject planeObject = Instantiate(planePrefab[planeType], allSpawnpoints[sp].position, allSpawnpoints[sp].rotation, transform);
				PlaneController plane = planeObject.GetComponent<PlaneController>();
				plane.PlaneSetup(this, mapBorders, ShowFeedback, AllowSound, -1);
				allPlanes.Add(plane);
			}
		}

		lastSpawnPoints = tempSP;

		if (allStartSpawnpoints.Length > 0)
		{
			float number = Random.Range(0, 100);
			if (number < startplaneSpawnChance)
			{
				if (startPlaneOnSpawnPoints[0] && startPlaneOnSpawnPoints[1])
				{
					int id = Random.Range(0, allStartSpawnpoints.Length);
					spawnStartPlane = id;
					switch(id)
					{
						case 0:
							ampel01.material = ampelLights[1];
							break;
						case 1:
							ampel02.material = ampelLights[1];
							break;
					}
				}
				else if(startPlaneOnSpawnPoints[0])
				{
					spawnStartPlane = 1;
					ampel02.material = ampelLights[1];
				}
				else
				{
					spawnStartPlane = 0;
					ampel01.material = ampelLights[1];
				}
			}
		}
	}

	public void PlaneLanded(PlaneController plane, int speed)
	{
		allPlanes.Remove(plane);
		gameMaster.MissionComplete(speed, false);
	}

	public void PlaneReachedDestination(PlaneController plane, int speed)
	{
		allPlanes.Remove(plane);
		gameMaster.MissionComplete(speed, true);
	}

	public bool CrashStarted(Vector3 pos, Vector3 dir)
	{
		if (endGame) return false;

		endGame = true;
		gameMaster.PlayCrashCloseUp(pos, dir);
		return true;
	}

	public void CrashFinished()
	{
		gameMaster.EndGame();
	}

	public void OutOfMap(PlaneController plane, int speed)
	{
		gameMaster.PlaneOutOfMap(speed);
		allPlanes.Remove(plane);
	}

	private IEnumerator EndGameAfterTime()
	{
		yield return new WaitForSeconds(2);
		CrashFinished();
	}
}
