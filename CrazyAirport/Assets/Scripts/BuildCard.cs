using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildCard : MonoBehaviour
{
	[Header("Card Setup", order = 2)]
	[SerializeField]
	private string buildingName = "";
	[SerializeField]
	private string description = "";
	[SerializeField]
	private Building.BuildingType cardType;
	[SerializeField]
	private int buildID = 0;



	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
