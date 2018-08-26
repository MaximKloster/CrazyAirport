using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartapultRotation : MonoBehaviour
{
	[SerializeField]
	private Animator anim;
	[SerializeField]
	private GameObject cleanMesh;
	[SerializeField]
	private GameObject dirtyMesh;
	private Quaternion defaultRotation;
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
	

	private void Start()
	{
		defaultRotation = transform.rotation;
	}

	public void PlaneLeft()
	{
		anim.SetTrigger("Fire");
		PlaneOnField = false;
		StartCoroutine(RotateBackAfterTime());
	}

	public void CleanStart()
	{
		if(cleanMesh != null && dirtyMesh != null)
		{
			cleanMesh.SetActive(true);
			dirtyMesh.SetActive(false);
		}
	}

	private void OnMouseDown()
	{
		if (planeOnField) transform.Rotate(transform.up, 90);
	}

	private IEnumerator RotateBackAfterTime()
	{
		yield return new WaitForSeconds(0.3f);
		if (cleanMesh != null && dirtyMesh != null)
		{
			cleanMesh.SetActive(false);
			dirtyMesh.SetActive(true);
		}
		yield return new WaitForSeconds(0.7f);
		transform.rotation = defaultRotation;
	}
}
