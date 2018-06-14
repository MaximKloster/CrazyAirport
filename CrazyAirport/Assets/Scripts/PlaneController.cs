using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{

	private Transform planeTransform;
	// Use this for initialization
	void Start()
	{
		planeTransform = transform;
	}

	// Update is called once per frame
	void Update()
	{

	}

	private void OnMouseDown()
	{
		planeTransform.Rotate(planeTransform.up, 90);
	}
}
