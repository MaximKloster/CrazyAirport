using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartapultRotation : MonoBehaviour
{
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
		PlaneOnField = false;
		StartCoroutine(RotateBackAfterTime());
	}

	private void OnMouseDown()
	{
		if (planeOnField) transform.Rotate(transform.up, 90);
	}

	private IEnumerator RotateBackAfterTime()
	{
		yield return new WaitForSeconds(1);
		transform.rotation = defaultRotation;
	}
}
