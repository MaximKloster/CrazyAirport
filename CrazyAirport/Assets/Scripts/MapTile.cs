using System.Collections;
using UnityEngine;

public class MapTile : MonoBehaviour
{
	public enum BuildStatus { Empty, Border, Building, Road, Stop, Start, Park }
	#region setup variables
	[Header("Start Setup", order = 2)]
	[SerializeField]
	private GameHandler gameMaster;
	[SerializeField]
	private ParticleSystem dirtPS;
	[SerializeField]
	private BuildStatus tileStatus = BuildStatus.Empty;
	[SerializeField]
	private GameObject ground;
	[SerializeField]
	private MeshRenderer highlightMesh;
	[SerializeField]
	private Material buildHeighlightMat;
	[SerializeField]
	private Material buildPossibleMat;
	#endregion
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
	public bool Dirty
	{
		get
		{
			return dirty;
		}

		private set
		{
			dirty = value;
		}
	}
	private bool isBuildable = false;
	public bool IsBuildable
	{
		get
		{
			return isBuildable;
		}

		set
		{
			isBuildable = value;
			if (isBuildable)
			{
				highlightMesh.gameObject.SetActive(true);
				highlightMesh.material = buildPossibleMat;
			}
			else highlightMesh.gameObject.SetActive(false);
		}
	}
	private bool planeOnField = false;
	public bool PlaneOnField
	{
		get
		{
			return planeOnField;
		}

		set
		{
			planeOnField = value;
		}
	}
	private GameObject cleanMesh;
	private GameObject dirtyMesh;

	private void Start()
	{
		if (dirtyMesh != null) dirtyMesh.SetActive(false);
		highlightMesh.gameObject.SetActive(false);
	}

	public void AddDirtyCleanMeshes(GameObject clean, GameObject dirty)
	{
		cleanMesh = clean;
		dirtyMesh = dirty;
		dirtyMesh.SetActive(false);
	}

	public void PlanePathField()
	{
		if (!Dirty)
		{
			Dirty = true;
			dirtPS.Play();
			if (tileStatus == BuildStatus.Park) gameMaster.DeactivatedPark(true);
			if (dirtyMesh != null && cleanMesh != null) ChangeMesh(true);
		}
	}

	public void CleanUpField()
	{
		if (Dirty)
		{
			Dirty = false;
			dirtPS.Stop();
			if (tileStatus == BuildStatus.Park) gameMaster.DeactivatedPark(false);
			if (dirtyMesh != null && cleanMesh != null) ChangeMesh(false);
			if (tileStatus == BuildStatus.Start) GetComponentInChildren<StartapultRotation>().CleanStart();
		}
	}

	public void CardOverItem(bool highlight)
	{
		if (highlight && IsBuildable) highlightMesh.material = buildHeighlightMat;
		else if(IsBuildable) highlightMesh.material = buildPossibleMat;
	}

	public void DeactivateGroundMesh()
	{
		ground.SetActive(false);
	}

	private void ChangeMesh(bool dirty)
	{
		if(dirty)
		{
			cleanMesh.SetActive(false);
			dirtyMesh.SetActive(true);
		}
		else
		{
			cleanMesh.SetActive(true);
			dirtyMesh.SetActive(false);
		}
	}
}
