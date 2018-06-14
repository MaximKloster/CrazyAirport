using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
	public enum BuildingType { Park, Control, Build, Card, Cleaning, Land, Start, Stop, Road }

	[Header("Start Setup", order = 2)]
	[SerializeField]
	private BuildingType buildingType = BuildingType.Park;

	//private void OnDrawGizmos()
	//{
	//	float height = 0.1f;
	//	float width = 0.7f;
	//	switch (buildingType)
	//	{
	//		case BuildingType.Park:
	//			height = 0.2f;
	//			Gizmos.color = Color.green;
	//			Gizmos.DrawCube(new Vector3(transform.position.x, height / 2, transform.position.z), new Vector3(width, height, width));
	//			break;
	//		case BuildingType.Control:
	//			height = 1f;
	//			Gizmos.color = Color.red;
	//			Gizmos.DrawCube(new Vector3(transform.position.x, height / 2, transform.position.z), new Vector3(width, height, width));
	//			break;
	//		case BuildingType.Build:
	//			height = 1f;
	//			Gizmos.color = Color.cyan;
	//			Gizmos.DrawCube(new Vector3(transform.position.x, height / 2, transform.position.z), new Vector3(width, height, width));
	//			break;
	//		case BuildingType.Card:
	//			height = 1f;
	//			Gizmos.color = Color.yellow;
	//			Gizmos.DrawCube(new Vector3(transform.position.x, height / 2, transform.position.z), new Vector3(width, height, width));
	//			break;
	//		case BuildingType.Cleaning:
	//			height = 1f;
	//			Gizmos.color = Color.blue;
	//			Gizmos.DrawCube(new Vector3(transform.position.x, height / 2, transform.position.z), new Vector3(width, height, width));
	//			break;
	//		case BuildingType.Start:
	//			height = 0.2f;
	//			Gizmos.color = Color.gray;
	//			Gizmos.DrawCube(new Vector3(transform.position.x, height / 2, transform.position.z), new Vector3(width, height, width));
	//			break;
	//		case BuildingType.Land:
	//			height = 0.2f;
	//			Gizmos.color = Color.yellow;
	//			Gizmos.DrawCube(new Vector3(transform.position.x, height / 2, transform.position.z), new Vector3(width, height, width));
	//			break;
	//		case BuildingType.Stop:
	//			height = 0.2f;
	//			Gizmos.color = Color.red;
	//			Gizmos.DrawCube(new Vector3(transform.position.x, height / 2, transform.position.z), new Vector3(width, height, width));
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
