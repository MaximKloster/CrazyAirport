using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingRotator : MonoBehaviour
{
	public enum RotationType { Y, Z }
	[SerializeField]
	private Transform rotator;
	[SerializeField]
	private RotationType rotType = RotationType.Z;
	[SerializeField]
	[Range(5, 100)]
	private float rotationSpeed = 10;
	[SerializeField]
	private bool useRandomSpeed = true;

	public float RotationSpeed
	{
		get
		{
			return rotationSpeed;
		}

		set
		{
			rotationSpeed = value;
		}
	}

	void Start()
	{
		if (useRandomSpeed) rotationSpeed = Random.Range(5, 25);
	}

	// Update is called once per frame
	void Update()
	{
		if (rotator != null)
		{
			switch (rotType)
			{
				case RotationType.Y:
					rotator.Rotate(rotator.up, Time.deltaTime * -rotationSpeed);
					break;
				case RotationType.Z:
					rotator.Rotate(rotator.forward, Time.deltaTime * rotationSpeed);
					break;
			}
		}
	}
}
