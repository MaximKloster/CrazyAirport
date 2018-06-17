using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
	public enum BuildStatus { Empty, Border, Building }

	[Header("Start Setup", order = 2)]
	[SerializeField]
	private GameHandler gameMaster;
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
	private bool dirty = false;

	private void OnMouseDown()
	{
		if (!dirty) gameMaster.MapTileClicked();
		else dirty = !gameMaster.TryToCleanField(); // If clean is possible the dirty will be removed
	}
}
