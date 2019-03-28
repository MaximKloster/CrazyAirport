using UnityEngine;
//using UnityEngine.PostProcessing;

public class CameraController : MonoBehaviour
{
	[Header("Camera Setup", order = 2)]
	[SerializeField]
	private GameObject[] cameras;
	[SerializeField]
	//private PostProcessingBehaviour ppb;


	private int currentCamera = 0;
	private bool camRotationAllowed = true;

	public bool CamRotationAllowed
	{
		get
		{
			return camRotationAllowed;
		}

		private set
		{
			camRotationAllowed = value;
		}
	}

	private void Start()
	{
		SetupCameras();
	}

	public void SwitchCamera()
	{
		cameras[currentCamera].SetActive(false);
		currentCamera++;
		if (currentCamera >= cameras.Length)
		{
			currentCamera = 0;
			CamRotationAllowed = true;
		}
		else
		{
			CamRotationAllowed = false;
		}
		cameras[currentCamera].SetActive(true);
	}

	//public void ActivateDOF(bool activate)
	//{
	//	ppb.profile.depthOfField.enabled = activate;
	//}

	private void SetupCameras()
	{
		foreach (GameObject cam in cameras)
		{
			cam.SetActive(false);
		}
		currentCamera = 0;
		cameras[currentCamera].SetActive(true);
		//ppb.profile.depthOfField.enabled = false;
	}

	public Camera GetCurrentCamera()
	{
		return cameras[currentCamera].GetComponent<Camera>();
	}
}
