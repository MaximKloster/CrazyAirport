using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
	public enum BuildStatus { Empty, Border, Building }

	[Header("Start Setup", order = 2)]
	[SerializeField]
	private BuildStatus tileStatus = BuildStatus.Empty;

	public BuildStatus TileStatus
	{
		get
		{
			return tileStatus;
		}
		set
		{
			tileStatus = value;
		}
	}
}
