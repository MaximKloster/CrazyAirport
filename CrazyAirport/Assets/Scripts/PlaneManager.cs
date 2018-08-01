using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneManager : MonoBehaviour
{
	[Header("Setup", order = 2)]
	[SerializeField]
	private GameHandler gameMaster;
	[SerializeField]
	private Transform[] allSpawnpoints;
	[SerializeField]
	private GameObject[] planePrefab;
	[SerializeField]
	private Vector4 mapBorders;
	private List<PlaneController> allPlanes;
	private int[] lastSpawnPoints = new int[3] { -1, -1, -1 }; // check if a plane was last time here started, so to do not use again in next round
	private int turn = 0;
	private int maxSpawnPlaneSize = 1;
	private int maxSpawnPlaneAmount = 1;
	private int level = 0;
	private bool showFeedback = false;
	private bool endGame = false;
	// Use this for initialization
	void Start()
	{
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
		showFeedback = !showFeedback;
		foreach (PlaneController plane in allPlanes)
		{
			plane.ShowMovementFeedback(showFeedback);
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
				if (turn > 15) // turns [5] 11 to 15
				{
					maxSpawnPlaneAmount = 2;
					maxSpawnPlaneSize = 2;
					level++;
				}
				break;
			case 3:
				if(turn > 20) // turns [5] 16 to 20
				{
					maxSpawnPlaneAmount = 2;
					maxSpawnPlaneSize = 3;
					level++;
				}
				break;
			case 4:
				if(turn > 25) // turns [5] 21 to 25
				{
					maxSpawnPlaneAmount = 3;
					maxSpawnPlaneSize = 2;
					level++;
				}
				break;
			case 5:
				if(turn > 30) // turns [5] 26 to 30
				{
					maxSpawnPlaneAmount = 3;
					maxSpawnPlaneSize = 3;
					level++;
				}
				break;
		}
	}

	private IEnumerator HandleTurn()
	{
		MoveAllPlanes();
		yield return new WaitForSeconds(1.25f);
		SpawnPlane();
		gameMaster.ReadyToContinue();
	}

	public bool TryToRotatePlane()
	{
		return gameMaster.CheckIfHaveControlPoints();
	}

	public void PlaneIsRotatedToDefault()
	{
		gameMaster.InteractionPlaneReset();
	}

	private void MoveAllPlanes()
	{
		foreach (PlaneController plane in allPlanes)
		{
			plane.Move();
		}
	}

	// id: 0 = Speed 1 (small plane), 1 = Speed 2 (normal plane), 2 = Speed 3 (fast plane)
	private void SpawnPlane()
	{
		CalculateLevel();

		for (int i = 0; i < maxSpawnPlaneAmount; i++)
		{
			int sp;
			do
			{
				sp = Random.Range(0, allSpawnpoints.Length);
			} while (sp == lastSpawnPoints[0] || sp == lastSpawnPoints[1] || sp == lastSpawnPoints[2]);
			lastSpawnPoints[i] = sp;

			int planeType = Random.Range(0, maxSpawnPlaneSize);
			GameObject planeObject = Instantiate(planePrefab[planeType], allSpawnpoints[sp].transform.position, allSpawnpoints[sp].transform.rotation, transform);
			PlaneController plane = planeObject.GetComponent<PlaneController>();
			plane.PlaneSetup(this, mapBorders, showFeedback);
			allPlanes.Add(plane);
		}
	}

	public void PlaneLanded(PlaneController plane)
	{
		allPlanes.Remove(plane);
		gameMaster.MissionComplete();
	}

	public void Crash()
	{
		endGame = true;
		gameMaster.EndGame();
		// TODO make game ending here
	}

	public void OutOfMap(PlaneController plane, int speed)
	{
		gameMaster.PlaneOutOfMap(speed);
		allPlanes.Remove(plane);
	}
}
