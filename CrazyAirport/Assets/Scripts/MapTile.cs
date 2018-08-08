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
	private MeshRenderer mesh;
	[SerializeField]
	private Material defaultMat;
	[SerializeField]
	private Material buildHeighlightMat;
	[SerializeField]
	private Material buildPossibleMat;
	[SerializeField]
	private Material notBuildableMat;
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
			if (isBuildable) mesh.material = buildPossibleMat;
			else mesh.material = defaultMat;
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

	public void PlanePathField()
	{
		if (!Dirty)
		{
			Dirty = true;
			dirtPS.Play();
			if (tileStatus == BuildStatus.Park) gameMaster.DeactivatedPark(true);
		}
	}

	public void CleanUpField()
	{
		if (Dirty)
		{
			Dirty = false;
			dirtPS.Stop();
			if (tileStatus == BuildStatus.Park) gameMaster.DeactivatedPark(false);
		}
	}

	public void CardOverItem(bool highlight)
	{
		if (highlight)
		{
			if (IsBuildable) mesh.material = buildHeighlightMat;
			else mesh.material = notBuildableMat;
		}
		else
		{
			if (IsBuildable) mesh.material = buildPossibleMat;
			else mesh.material = defaultMat;
		}
	}

	public void DeactivateMesh()
	{
		mesh.gameObject.SetActive(false);
	}

	public void SetNewRenderMesh(MeshRenderer newMesh)
	{
		mesh = newMesh;
	}
}
