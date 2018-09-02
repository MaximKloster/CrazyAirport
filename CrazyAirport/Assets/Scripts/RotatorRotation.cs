using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorRotation : MonoBehaviour
{
	private Transform rotatorTransform;
	// Use this for initialization
	void Start()
	{
		rotatorTransform = transform;
	}

	// Update is called once per frame
	void Update()
	{

	}

	private void OnMouseDown()
	{
		rotatorTransform.Rotate(rotatorTransform.up, 90);
	}
}
