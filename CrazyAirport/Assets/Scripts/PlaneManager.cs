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
	private GameObject[] planePrefab;
	[SerializeField]
	private Vector4 mapBorders;
	#endregion
	#region gameplay variables
	private List<PlaneController> allPlanes;
	private int[] lastSpawnPoints = new int[4] { -1, -1, -1, -1 }; // check if a plane was last time here started, so to do not use again in next round
	private int turn = 0;
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
	private int currentLevel; // TODO need later for starting planes
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
	#region debug
	[Header("Debug", order = 2)]
	[SerializeField]
	private bool onlyPlanes3 = false;
	#endregion
	// Use this for initialization
	void Start()
	{
		currentLevel = PlayerPrefs.GetInt("Level");
		audioSource = GetComponent<AudioSource>();
		turn = 0;
		level = 0;
		allPlanes = new List<PlaneController>();
	}

	public void NextRound()
	{
		turn++;
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
				if (turn > 5) // check [5] turns 1 to 5 
				{
					maxSpawnPlaneSize = 2;
					level++;
					gameMaster.ReachedLevelOne();
				}
				break;
			case 1:
				if (turn > 10) // check [5] turns 6 to 10 
				{
					maxSpawnPlaneSize = 3;
					level++;
				}
				break;
			case 2:
				if (turn > 15) // check turns [5] 11 to 15
				{
					maxSpawnPlaneAmount = 2;
					maxSpawnPlaneSize = 2;
					level++;
				}
				break;
			case 3:
				if (turn > 20) // check turns [5] 16 to 20
				{
					maxSpawnPlaneAmount = 2;
					maxSpawnPlaneSize = 3;
					level++;
				}
				break;
			case 4:
				if (turn > 25) // check turns [5] 21 to 25
				{
					maxSpawnPlaneAmount = 3;
					maxSpawnPlaneSize = 2;
					level++;
				}
				break;
			case 5:
				if (turn > 30) // check turns [5] 26 to 30
				{
					maxSpawnPlaneAmount = 3;
					maxSpawnPlaneSize = 3;
					level++;
				}
				break;
			case 6:
				if (turn > 37) // check turns [7] 31 to 37
				{
					maxSpawnPlaneAmount = 4;
					maxSpawnPlaneSize = 2;
					level++;
				}
				break;
			case 7:
				if (turn > 45) // check turns [8] 38 to 45
				{
					maxSpawnPlaneAmount = 4;
					maxSpawnPlaneSize = 3;
					level++;
				}
				break;
		}
	}

	private IEnumerator HandleTurn()
	{
		if (turn > 1)
		{
			MoveAllPlanes();
			yield return new WaitForSeconds(1.25f);
		}
		SpawnPlane();
		gameMaster.ReadyToContinue();
	}

	public bool TryToRotatePlane()
	{
		return (gameMaster.CheckIfHaveControlPoints());
	}

	public void PlaneIsRotatedToDefault()
	{
		gameMaster.InteractionPlaneReset();
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
			do
			{
				sp = Random.Range(0, allSpawnpoints.Length);
			} while (sp == lastSpawnPoints[0] || sp == lastSpawnPoints[1] || sp == lastSpawnPoints[2] || sp == lastSpawnPoints[3] || sp == tempSP[0] || sp == tempSP[1] || sp == tempSP[2] || sp == tempSP[3]);
			tempSP[i] = sp;

			int planeType;
			if (onlyPlanes3) planeType = 2;
			else planeType = Random.Range(0, maxSpawnPlaneSize);
			GameObject planeObject = Instantiate(planePrefab[planeType], allSpawnpoints[sp].transform.position, allSpawnpoints[sp].transform.rotation, transform);
			PlaneController plane = planeObject.GetComponent<PlaneController>();
			plane.PlaneSetup(this, mapBorders, ShowFeedback, AllowSound);
			allPlanes.Add(plane);
		}

		lastSpawnPoints = tempSP;
	}

	public void PlaneLanded(PlaneController plane)
	{
		allPlanes.Remove(plane);
		gameMaster.MissionComplete();
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
}
