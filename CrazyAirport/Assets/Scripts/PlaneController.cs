using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaneController : MonoBehaviour
{
	enum PlaneRotation { NONE, LEFT, RIGHT }
	public enum Destination { NONE, NORTH, EAST, SOUTH, WEST }
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
	private BuildingRotator markerRotator;
	[SerializeField]
	private MeshRenderer arrowsMesh;
	[SerializeField]
	private Material[] arrowMats;
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
	private StartapultRotation startapult;
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
	private BoxCollider[] planeColliders;
	private float defaultRotFBSpeed;
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
		defaultRotFBSpeed = markerRotator.RotationSpeed;
		planeColliders = GetComponents<BoxCollider>();
		audioSource = GetComponent<AudioSource>();
		audioSource.clip = planeRotateClip;
		planeTransform = transform;
		groundMarker.transform.parent = null;
		SetRotationFeedback(true);
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

	public void PlaneSetup(PlaneManager manager, Vector4 mapBorders, bool show, bool sound, int id, Destination[] newPossibleDestinations = null, bool startingPlane = false)
	{
		startID = id;
		showFB = show;
		AllowSound = sound;
		planeMan = manager;
		borders = mapBorders;
		isStartingPlane = startingPlane;
		if (isStartingPlane)
		{
			SetUpStartingPlane(newPossibleDestinations);
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

	private void SetUpStartingPlane(Destination[] newPossibleDestination)
	{
		int dir = Random.Range(0, newPossibleDestination.Length);
		Material mat = mat = dirFBMats[0];
		switch (newPossibleDestination[dir])
		{
			case Destination.NORTH:
				destinationDir = Destination.NORTH;
				directionFeedback.transform.Rotate(directionFeedback.transform.up, 180);
				mat = dirFBMats[0];
				break;
			case Destination.SOUTH:
				destinationDir = Destination.SOUTH;
				mat = dirFBMats[1];
				break;
			case Destination.EAST:
				destinationDir = Destination.EAST;
				directionFeedback.transform.Rotate(directionFeedback.transform.up, -90);
				mat = dirFBMats[2];
				break;
			case Destination.WEST:
				destinationDir = Destination.WEST;
				directionFeedback.transform.Rotate(directionFeedback.transform.up, 90);
				mat = dirFBMats[3];
				break;
		}
		directionFeedback.GetComponentInChildren<MeshRenderer>().material = mat;
		directionFeedback.transform.parent = null;
		flying = false;
		landingAnim.SetTrigger("Start");
		movementFeedback.transform.localScale = new Vector3(0, 0, 0);
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
			foreach (BoxCollider collider in planeColliders)
			{
				if (!collider.isTrigger) collider.enabled = true;
			}
			grabbed = false;
			flying = true;
			planeMan.ReleasedPlane(startID, true);
			ShowMovementFeedback(showFB);
			startapult = planeOnField.GetComponentInChildren<StartapultRotation>();
			startapult.PlaneOnField = true;
			planeTransform.parent = startapult.transform;
			planeTransform.rotation = startapult.transform.rotation;
		}
		else ResetPlane();
	}

	private void ResetPlane()
	{
		foreach (BoxCollider collider in planeColliders)
		{
			collider.enabled = true;
		}
		grabbed = false;
		planeMan.ReleasedPlane(startID, false);
		landingAnim.SetTrigger("Start");
		onStartapult = false;
		planeOnField = null;
		SetItemsPosition(defaultPos);
		gameObject.transform.rotation = defaultRot;
	}

	public bool WasRotated()
	{
		if (planeRotation == PlaneRotation.NONE) return false;
		return true;
	}

	// plane was clicked for rotation
	private void OnMouseDown()
	{
		if (notInteractable)
		{
			if (flying) return;
			if (planeMan.TryToPlacePlane())
			{
				foreach (BoxCollider collider in planeColliders)
				{
					collider.enabled = false;
				}
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
						arrowsMesh.material = arrowMats[1];
						markerRotator.RotationSpeed = defaultRotFBSpeed;
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
					arrowsMesh.material = arrowMats[0];
					planeRotation = PlaneRotation.NONE;
					planeMan.PlaneIsRotatedToDefault();
					planeTransform.Rotate(planeTransform.up, 90);
					movementFeedback.transform.Rotate(planeTransform.up, 90);
					if (AllowSound) audioSource.Play();
					break;
			}
		}
	}

	public void SetRotationFeedback(bool disable)
	{
		if(disable)
		{
			arrowsMesh.material = arrowMats[2];
			markerRotator.RotationSpeed = 0;
		}
		else
		{
			if (notInteractable) return;
			arrowsMesh.material = arrowMats[0];
			markerRotator.RotationSpeed = defaultRotFBSpeed;
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

	public void PlayeExplosion()
	{
		explosionPS.Play();
		burnPS.Play();
		smokePS.Play();
	}
	// plane moves as long as reached his destination
	private IEnumerator MoveToNextField()
	{
		if (onStartapult)
		{
			foreach (BoxCollider collider in planeColliders)
			{
				if (collider.isTrigger) collider.enabled = true;
			}
			planeTransform.parent = null;
			movementFeedback.transform.parent = null;
			startapult.PlaneLeft();
			onStartapult = false;
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
			SetItemsPosition(newPos);
			return false;
		}
		else
		{
			bool checkFielAfterMove = false;
			if (currentFlyDistance > Vector3.Distance(planeTransform.position, checkPointPos)) checkFielAfterMove = true;

			if (landing) SetItemsPosition(planeTransform.position + planeTransform.forward * Time.deltaTime);
			else SetItemsPosition(planeTransform.position + planeTransform.forward * fieldsMovement * Time.deltaTime);

			if (checkFielAfterMove)
			{
				checkPointPos = checkPointPos + planeTransform.forward;
				movesDone++;
				if (movesDone == fieldsMovement) CheckMapTile(true);
				else CheckMapTile();
			}
		}
		return true;
	}

	private void SetItemsPosition(Vector3 newPos)
	{
		planeTransform.position = newPos;
		directionFeedback.transform.position = newPos;
		groundMarker.transform.position = newPos;
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
			planeMan.PlaneLanded(this, fieldsMovement);
			Destroy(movementFeedback.gameObject);
			Destroy(groundMarker.gameObject);
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
					//case MapTile.BuildStatus.Start:
					case MapTile.BuildStatus.Park:
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
					if (status == MapTile.BuildStatus.Road && !isStartingPlane)
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
		if (planeTransform.position.z >= borders.y)
		{
			if (isStartingPlane && destinationDir == Destination.NORTH) planeMan.PlaneReachedDestination(this, fieldsMovement);
			else planeMan.OutOfMap(this, fieldsMovement);
			StartCoroutine(FlyAwayAnimation());
		}
		else if (planeTransform.position.x >= borders.z)
		{
			if (isStartingPlane && destinationDir == Destination.EAST) planeMan.PlaneReachedDestination(this, fieldsMovement);
			else planeMan.OutOfMap(this, fieldsMovement);
			StartCoroutine(FlyAwayAnimation());
		}
		else if (planeTransform.position.z <= borders.w)
		{
			if (isStartingPlane && destinationDir == Destination.SOUTH) planeMan.PlaneReachedDestination(this, fieldsMovement);
			else planeMan.OutOfMap(this, fieldsMovement);
			StartCoroutine(FlyAwayAnimation());
		}
		else if (planeTransform.position.x <= borders.x)
		{
			if (isStartingPlane && destinationDir == Destination.WEST) planeMan.PlaneReachedDestination(this, fieldsMovement);
			else planeMan.OutOfMap(this, fieldsMovement);
			StartCoroutine(FlyAwayAnimation());
		}
		else notInteractable = false;
		SetRotationFeedback(false);
	}

	// if crash with an object start crash animation
	private IEnumerator CrashAnimation()
	{
		if(movementCoroutine != null) StopCoroutine(movementCoroutine);
		GetComponent<Collider>().enabled = false;
		Destroy(movementFeedback.gameObject);
		groundMarker.SetActive(false);
		if (directionFeedback.activeSelf) directionFeedback.SetActive(false);
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
		groundMarker.SetActive(false);
		if (directionFeedback.activeSelf) directionFeedback.SetActive(false);
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
		currentCam = planeMan.GiveCurrentCam();
		bool perspectivCam = planeMan.PerspectiveCam();
		grabbed = true;
		Vector3 worldPoint;
		RaycastHit hit;
		while (grabbed)
		{
			Vector2 mouse = Input.mousePosition;
			if (perspectivCam)
			{
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
							SetItemsPosition(mapTile.transform.position);
							gameObject.transform.rotation = mapTile.transform.rotation;
						}
						else
						{
							onStartapult = false;
							planeOnField = null;
							landingAnim.SetTrigger("Start");
							SetItemsPosition(hit.point);
						}
					}
					else
					{
						onStartapult = false;
						planeOnField = null;
						landingAnim.SetTrigger("Start");
						SetItemsPosition(hit.point);
					}
				}
				else ResetPlane();
			}
			else
			{
				Ray ray = currentCam.ScreenPointToRay(new Vector3(mouse.x, mouse.y, 250));
				if (Physics.Raycast(ray.origin, currentCam.transform.forward, out hit, 10, ignoreMask))
				{
					MapTile mapTile = hit.collider.gameObject.GetComponent<MapTile>();
					if (mapTile != null)
					{
						if (mapTile.TileStatus == MapTile.BuildStatus.Start && mapTile.IsBuildable)
						{
							landingAnim.SetTrigger("Prepare");
							onStartapult = true;
							planeOnField = mapTile;
							SetItemsPosition(mapTile.transform.position);
							gameObject.transform.rotation = mapTile.transform.rotation;
						}
						else
						{
							onStartapult = false;
							planeOnField = null;
							landingAnim.SetTrigger("Start");
							SetItemsPosition(hit.point);
						}
					}
					else
					{
						onStartapult = false;
						planeOnField = null;
						landingAnim.SetTrigger("Start");
						SetItemsPosition(hit.point);
					}
				}
				else ResetPlane();
			}
			yield return null;
		}
	}
}
