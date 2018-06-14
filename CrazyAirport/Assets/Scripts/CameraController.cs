using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[Header("Camera Setup", order = 2)]
	[SerializeField]
	private GameObject[] cameras;

	private int currentCamera = 0;

	private void Start()
	{
		SetupCameras();
	}

	public void SwitchCamera()
	{
		cameras[currentCamera].SetActive(false);
		currentCamera++;
		if (currentCamera >= cameras.Length) currentCamera = 0;
		cameras[currentCamera].SetActive(true);
	}

	private void SetupCameras()
	{
		foreach(GameObject cam in cameras)
		{
			cam.SetActive(false);
		}
		currentCamera = 0;
		cameras[currentCamera].SetActive(true);
	}
}
