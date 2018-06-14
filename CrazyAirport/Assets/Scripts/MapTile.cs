using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
	public enum BuildStatus { Empty, Road, Building }

	[Header("Start Setup", order = 2)]
	[SerializeField]
	private BuildStatus tileStatus = BuildStatus.Empty;
	[SerializeField]
	private GameObject buildingPrefab;

	//private GameObject currentBuilding = null;

	//private void OnDrawGizmos()
	//{
	//	switch (tileStatus)
	//	{
	//		case BuildStatus.Empty:
	//			if (currentBuilding != null) DestroyImmediate(currentBuilding);
	//			break;
	//		case BuildStatus.Building:
	//			if (currentBuilding == null) currentBuilding = Instantiate(buildingPrefab, transform);
	//			break;
	//		case BuildStatus.Road:
	//			if (currentBuilding != null) DestroyImmediate(currentBuilding);
	//			float height = 0.15f;
	//			Gizmos.color = Color.red;
	//			Gizmos.DrawWireCube(new Vector3(transform.position.x, height / 2, transform.position.z), new Vector3(0.7f, height, 0.7f));
	//			break;
	//	}
	//}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
