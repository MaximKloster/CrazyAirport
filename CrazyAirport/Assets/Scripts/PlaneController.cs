using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaneController : MonoBehaviour
{
	enum PlaneRotation { NONE, LEFT, RIGHT }
	enum Destination { NONE, NORTH, EAST, SOUTH, WEST }
	#region plane parts
	[Header("Plane Parts", order = 2)]
	[SerializeField]
	private ParticleSystem explosionPS;
	[SerializeField]
	private ParticleSystem burnPS;
	[SerializeField]
	private ParticleSystem smokePS;
	[SerializeField]
	private GameObject groundMarker;
	[SerializeField]
	private GameObject planeMesh;
	[SerializeField]
	private GameObject movementFeedback;
	[SerializeField]
	private GameObject directionFeedback;
	[SerializeField]
	private Animator landingAnim;
	[SerializeField]
	private Animator moveFBAnim;
	[SerializeField]
	private Material[] dirFBMats; // 0 = North, 1 = East, 2 = South, 3 = West
	#endregion
	#region plane variables
	[Header("Variables", order = 2)]
	[SerializeField]
	private LayerMask ignoreMask;
	[SerializeField]
	[Range(1, 3)]
	private int fieldsMovement = 1;
	[SerializeField]
	[Range(0.1f, 10)]
	private float landingSpeed = 1.2f;
	[SerializeField]
	[Range(0.1f, 10)]
	private float crashRotationSpeed = 7.5f;
	[SerializeField]
	[Range(0.1f, 5)]
	private float crashFallSpeed = 0.4f;
	private Vector4 borders;
	bool landing = false;
	bool isStartingPlane = false;
	#endregion
	#region gameplay variables
	private PlaneRotation planeRotation = PlaneRotation.NONE;
	private Transform planeTransform;
	private bool notInteractable = true;
	private PlaneManager planeMan;
	private Coroutine movementCoroutine;
	private MapTile planeOnField;
	private Vector3 checkPointPos;
	private int movesDone = 0;
	private bool callCrashFinished = false;
	private Destination destinationDir = Destination.NONE;
	private bool flying = true;
	private Camera currentCam;
	private bool grabbed = false;
	private Vector3 defaultPos;
	private Quaternion defaultRot;
	private bool onStartapult = false;
	private bool showFB = true;
	private int startID;
	private BoxCollider interactionTrigger;
	#endregion
	#region sound variables
	[SerializeField]
	private AudioClip planeRotateClip;
	[SerializeField]
	private AudioClip planeCrashClip;
	[SerializeField]
	private AudioClip planeOnGroundClip;
	private AudioSource audioSource;
	private bool allowSound;
	public bool AllowSound
	{
		get
		{
			return allowSound;
		}

		set
		{
			allowSound = value;
		}
	}
	#endregion

	void Start()
	{
		BoxCollider[] colliders = GetComponents<BoxCollider>();
		foreach (BoxCollider collider in colliders)
		{
			if (collider.isTrigger) interactionTrigger = collider;
		}
		audioSource = GetComponent<AudioSource>();
		audioSource.clip = planeRotateClip;
		planeTransform = transform;
	}

	public void ShowMovementFeedback(bool show)
	{
		showFB = show;
		if (flying)
		{
			if (show)
			{
				movementFeedback.transform.localScale = new Vector3(1, 1, 1);
				moveFBAnim.SetTrigger("Reset");
			}
			else
			{
				movementFeedback.transform.localScale = new Vector3(0, 0, 0);
			}
		}
	}

	public void PlaneSetup(PlaneManager manager, Vector4 mapBorders, bool show, bool sound, int id, bool allDirections = false, bool startingPlane = false)
	{
		startID = id;
		showFB = show;
		AllowSound = sound;
		planeMan = manager;
		borders = mapBorders;
		isStartingPlane = startingPlane;
		if (isStartingPlane)
		{
			SetUpStartingPlane(allDirections);
		}
		else
		{
			directionFeedback.SetActive(false);
			movementFeedback.transform.parent = null;
			ShowMovementFeedback(show);
		}
		defaultPos = transform.position;
		defaultRot = transform.rotation;
	}

	private void SetUpStartingPlane(bool allDirections)
	{
		Debug.Log("setup");
		int dir = Random.Range(0, allDirections ? 4 : 3);
		switch (dir)
		{
			case 0:
				destinationDir = Destination.NORTH;
				break;
			case 1:
				destinationDir = Destination.SOUTH;
				break;
			case 2:
				destinationDir = Destination.EAST;
				break;
			case 3:
				destinationDir = Destination.WEST;
				break;
		}
		Debug.Log(destinationDir);
		directionFeedback.GetComponentInChildren<MeshRenderer>().material = dirFBMats[dir];
		flying = false;
		landingAnim.SetTrigger("Start");
		movementFeedback.transform.localScale = new Vector3(0, 0, 0);
		//directionFeedback.transform.parent = null;
		directionFeedback.SetActive(true);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (flying)
		{
			if (planeMan.CrashStarted(planeTransform.position, planeTransform.forward))
			{
				callCrashFinished = true;
				if (AllowSound)
				{
					audioSource.clip = planeCrashClip;
					audioSource.Play();
				}
			}
			StartCoroutine(CrashAnimation());
		}
	}

	private void OnMouseUp()
	{
		if (grabbed)
		{
			ReleasePlane();
		}
	}

	private void ReleasePlane()
	{
		if (onStartapult)
		{
			grabbed = false;
			flying = true;
			planeMan.ReleasedPlane(startID, true);
			ShowMovementFeedback(showFB);
			interactionTrigger.enabled = false;
			StartapultRotation startapult = planeOnField.GetComponentInChildren<StartapultRotation>();
			startapult.PlaneOnField = true;
			planeTransform.parent = startapult.transform;
		}
		else ResetPlane();
	}

	private void ResetPlane()
	{
		grabbed = false;
		planeMan.ReleasedPlane(startID, false);
		landingAnim.SetTrigger("Start");
		onStartapult = false;
		planeOnField = null;
		gameObject.transform.position = defaultPos;
		gameObject.transform.rotation = defaultRot;
	}

	// plane was clicked for rotation
	private void OnMouseDown()
	{
		if (notInteractable)
		{
			if (flying) return;
			if (planeMan.TryToPlacePlane())
			{
				currentCam = planeMan.GiveCurrentCam();
				StartCoroutine(FollowFinger());
			}
		}
		else
		{
			switch (planeRotation)
			{
				case PlaneRotation.NONE:
					if (planeMan.TryToRotatePlane())
					{
						planeRotation = PlaneRotation.RIGHT;
						planeTransform.Rotate(planeTransform.up, 90);
						movementFeedback.transform.Rotate(planeTransform.up, 90);
						if (AllowSound) audioSource.Play();
					}
					break;
				case PlaneRotation.RIGHT:
					planeRotation = PlaneRotation.LEFT;
					planeTransform.Rotate(planeTransform.up, -180);
					movementFeedback.transform.Rotate(planeTransform.up, -180);
					if (AllowSound) audioSource.Play();
					break;
				case PlaneRotation.LEFT:
					planeRotation = PlaneRotation.NONE;
					planeMan.PlaneIsRotatedToDefault();
					planeTransform.Rotate(planeTransform.up, 90);
					movementFeedback.transform.Rotate(planeTransform.up, 90);
					if (AllowSound) audioSource.Play();
					break;
			}
		}
	}

	// preparing to move to the next field
	public void Move()
	{
		notInteractable = true;
		planeRotation = PlaneRotation.NONE;
		if (moveFBAnim != null) moveFBAnim.SetTrigger("Move");
		if (flying) movementCoroutine = StartCoroutine(MoveToNextField());
	}

	// plane moves as long as reached his destination
	private IEnumerator MoveToNextField()
	{
		if (onStartapult)
		{
			planeTransform.parent = null;
			movementFeedback.transform.parent = null;
			planeOnField.GetComponentInChildren<StartapultRotation>().PlaneLeft();
			interactionTrigger.enabled = true;
			onStartapult = false;
			planeOnField.PlanePathField();
			landingAnim.SetTrigger("Fly");
		}

		if (planeOnField != null)
		{
			planeOnField.PlaneOnField = false;
			planeOnField = null;
		}

		Vector3 newPos = planeTransform.position + planeTransform.forward * fieldsMovement;
		checkPointPos = planeTransform.position + planeTransform.forward * 0.51f;
		movesDone = 0;
		while (Fly(newPos))
		{
			yield return null;
		}
		movementFeedback.transform.position = planeTransform.position;
		CheckBorders();
	}

	// fly every field and check if there is a stop tile or a landing part
	private bool Fly(Vector3 newPos)
	{
		float flyDistance = Vector3.Distance(planeTransform.position, newPos);
		float currentFlyDistance = Vector3.Distance(planeTransform.position, planeTransform.position + planeTransform.forward * fieldsMovement * Time.deltaTime);

		if (currentFlyDistance > flyDistance)
		{
			planeTransform.position = newPos;
			return false;
		}
		else
		{
			bool checkFielAfterMove = false;
			if (currentFlyDistance > Vector3.Distance(planeTransform.position, checkPointPos)) checkFielAfterMove = true;

			if (landing) planeTransform.position = planeTransform.position + planeTransform.forward * Time.deltaTime;
			else planeTransform.position = planeTransform.position + planeTransform.forward * fieldsMovement * Time.deltaTime;

			if (checkFielAfterMove)
			{
				checkPointPos = checkPointPos + planeTransform.forward;
				movesDone++;
				if (movesDone == fieldsMovement && !isStartingPlane) CheckMapTile(true);
				else CheckMapTile();
			}
		}
		return true;
	}

	// fly as long the plane reached the stop or landing field it stops the MoveToNextField coroutine
	private IEnumerator StopAtField(Vector3 newPos)
	{
		if (landing) landingAnim.SetTrigger("Land");
		while (Fly(newPos))
		{
			yield return null;
		}
		moveFBAnim.enabled = false;

		if (landing)
		{
			planeMesh.transform.position = new Vector3(planeMesh.transform.position.x, planeMesh.transform.position.y - (landingSpeed * Time.deltaTime), planeMesh.transform.position.z);
			planeMan.PlaneLanded(this);
			Destroy(movementFeedback.gameObject);
			yield return new WaitForSeconds(0.5f);
			Destroy(gameObject);
		}
		else
		{
			notInteractable = false;
			movementFeedback.transform.position = planeTransform.position;
			moveFBAnim.enabled = true;
			moveFBAnim.SetTrigger("Reset");
		}
	}

	// check the map tile for landing or stop field
	private void CheckMapTile(bool checkLanding = false)
	{
		RaycastHit hit;
		if (Physics.Raycast(planeMesh.transform.position, -planeTransform.up, out hit, 1, ignoreMask))
		{
			MapTile mapTile = hit.collider.gameObject.GetComponent<MapTile>();
			if (mapTile != null)
			{
				MapTile.BuildStatus status = mapTile.TileStatus;
				switch (status)
				{
					case MapTile.BuildStatus.Park:
					case MapTile.BuildStatus.Start:
						mapTile.PlanePathField();
						break;
					case MapTile.BuildStatus.Stop:
						if (!mapTile.Dirty)
						{
							StopCoroutine(movementCoroutine);
							planeOnField = mapTile;
							mapTile.PlaneOnField = true;
							movementCoroutine = StartCoroutine(StopAtField(mapTile.gameObject.transform.position));
							mapTile.PlanePathField();
						}
						break;
				}

				if (checkLanding)
				{
					if (status == MapTile.BuildStatus.Road)
					{
						StopCoroutine(movementCoroutine);
						landing = true;
						movementCoroutine = StartCoroutine(StopAtField(mapTile.gameObject.transform.position));
					}
					else
					{
						planeOnField = mapTile;
						mapTile.PlaneOnField = true;
					}
				}
			}
		}
	}

	// after reached destination check if the plane is outsite the map
	private void CheckBorders()
	{
		if (planeTransform.position.x <= borders.x || planeTransform.position.z >= borders.y || planeTransform.position.x >= borders.z || planeTransform.position.z <= borders.w)
		{
			StartCoroutine(FlyAwayAnimation());
		}
		else notInteractable = false;
	}

	// if crash with an object start crash animation
	private IEnumerator CrashAnimation()
	{
		StopCoroutine(movementCoroutine);
		GetComponent<Collider>().enabled = false;
		Destroy(movementFeedback.gameObject);
		groundMarker.SetActive(false);
		explosionPS.Play();
		switch (fieldsMovement)
		{
			case 1:
				landingAnim.SetTrigger("Crash_Small");
				break;
			case 2:
				landingAnim.SetTrigger("Crash_Medium");
				break;
			case 3:
				landingAnim.SetTrigger("Crash_Fast");
				break;
		}
		yield return new WaitForSeconds(2);

		burnPS.Play();
		smokePS.Play();
		if (AllowSound)
		{
			audioSource.clip = planeOnGroundClip;
			audioSource.Play();
		}

		yield return new WaitForSeconds(1.5f);
		if (callCrashFinished) planeMan.CrashFinished();
	}

	// if gets outsite the map start the fly away animation
	private IEnumerator FlyAwayAnimation()
	{
		GetComponent<Collider>().enabled = false;
		movementFeedback.SetActive(false);
		planeMan.OutOfMap(this, fieldsMovement);
		groundMarker.SetActive(false);
		float time = 0;
		while (time < 2)
		{
			planeTransform.position = planeTransform.position + planeTransform.forward * fieldsMovement * Time.deltaTime;
			planeMesh.transform.position = new Vector3(planeMesh.transform.position.x, planeMesh.transform.position.y + (landingSpeed * Time.deltaTime), planeMesh.transform.position.z);
			time += Time.deltaTime;
			yield return null;
		}
		Destroy(gameObject);
	}

	private IEnumerator FollowFinger()
	{
		grabbed = true;
		Vector3 worldPoint;
		RaycastHit hit;
		while (grabbed)
		{
			Vector2 mouse = Input.mousePosition;
			worldPoint = currentCam.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, 250));
			if (Physics.Raycast(currentCam.transform.position, worldPoint, out hit, 10, ignoreMask))
			{
				MapTile mapTile = hit.collider.gameObject.GetComponent<MapTile>();
				if (mapTile != null)
				{
					if (mapTile.TileStatus == MapTile.BuildStatus.Start && mapTile.IsBuildable)
					{
						landingAnim.SetTrigger("Prepare");
						onStartapult = true;
						planeOnField = mapTile;
						gameObject.transform.position = mapTile.transform.position;
						gameObject.transform.rotation = mapTile.transform.rotation;
					}
					else
					{
						onStartapult = false;
						planeOnField = null;
						landingAnim.SetTrigger("Start");
						gameObject.transform.position = hit.point;
					}
				}
				else
				{
					onStartapult = false;
					planeOnField = null;
					landingAnim.SetTrigger("Start");
					gameObject.transform.position = hit.point;
				}
			}
			else ResetPlane();

			yield return null;
		}
	}
}
