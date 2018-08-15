using System.Collections;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
	enum PlaneRotation { NONE, LEFT, RIGHT }
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
	#endregion
	#region sound variables
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

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, transform.position + transform.forward);
	}

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		planeTransform = transform;
	}

	public void ShowMovementFeedback(bool show)
	{
		movementFeedback.SetActive(show);
	}

	public void PlaneSetup(PlaneManager manager, Vector4 mapBorders, bool show, bool sound)
	{
		AllowSound = sound;
		planeMan = manager;
		borders = mapBorders;
		movementFeedback.SetActive(show);
	}

	private void OnCollisionEnter(Collision collision)
	{
		callCrashFinished = planeMan.CrashStarted(planeTransform.position, planeTransform.forward);
		StartCoroutine(CrashAnimation());
	}

	// plane was clicked for rotation
	private void OnMouseDown()
	{
		if (notInteractable) return;
		switch (planeRotation)
		{
			case PlaneRotation.NONE:
				if (planeMan.TryToRotatePlane())
				{
					planeRotation = PlaneRotation.RIGHT;
					planeTransform.Rotate(planeTransform.up, 90);
					if (AllowSound) audioSource.Play();
				}
				break;
			case PlaneRotation.RIGHT:
				planeRotation = PlaneRotation.LEFT;
				planeTransform.Rotate(planeTransform.up, -180);
				if (AllowSound) audioSource.Play();
				break;
			case PlaneRotation.LEFT:
				planeRotation = PlaneRotation.NONE;
				planeMan.PlaneIsRotatedToDefault();
				planeTransform.Rotate(planeTransform.up, 90);
				if (AllowSound) audioSource.Play();
				break;
		}
	}

	// preparing to move to the next field
	public void Move()
	{
		notInteractable = true;
		planeRotation = PlaneRotation.NONE;
		movementCoroutine = StartCoroutine(MoveToNextField());
	}

	// plane moves as long as reached his destination
	private IEnumerator MoveToNextField()
	{
		if (planeOnField != null)
		{
			planeOnField.PlaneOnField = false;
			planeOnField = null;
		}
		Vector3 newPos = planeTransform.position + planeTransform.forward * fieldsMovement;
		checkPointPos = planeTransform.position + planeTransform.forward * 0.5f;
		movesDone = 0;
		while (Fly(newPos))
		{
			yield return null;
		}
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
				if (movesDone == fieldsMovement) CheckMapTile(true);
				else CheckMapTile();
			}
		}
		return true;
	}

	// fly as long the plane reached the stop or landing field it stops the MoveToNextField coroutine
	private IEnumerator StopAtField(Vector3 newPos)
	{
		if (landing) movementFeedback.SetActive(false);
		while (Fly(newPos))
		{
			if (landing) planeMesh.transform.position = new Vector3(planeMesh.transform.position.x, planeMesh.transform.position.y - (landingSpeed * Time.deltaTime), planeMesh.transform.position.z);
			yield return null;
		}

		if (landing)
		{
			planeMesh.transform.position = new Vector3(planeMesh.transform.position.x, planeMesh.transform.position.y - (landingSpeed * Time.deltaTime), planeMesh.transform.position.z);
			planeMan.PlaneLanded(this);
			yield return new WaitForSeconds(0.5f);
			Destroy(gameObject);
		}
		else
		{
			notInteractable = false;
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
		movementFeedback.SetActive(false);
		groundMarker.SetActive(false);
		explosionPS.Play();
		while (planeMesh.transform.position.y > -0.12f)
		{
			planeMesh.transform.Rotate(planeMesh.transform.up, crashRotationSpeed);
			planeMesh.transform.position = new Vector3(planeMesh.transform.position.x, planeMesh.transform.position.y - (crashFallSpeed * Time.deltaTime), planeMesh.transform.position.z);
			yield return null;
		}
		burnPS.Play();
		smokePS.Play();
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
}
