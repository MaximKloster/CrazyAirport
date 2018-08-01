using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildCard : MonoBehaviour
{
	[Header("Card Setup", order = 2)]
	[SerializeField]
	private string buildingName = "";
	[SerializeField]
	private string description = "";
	[SerializeField]
	private GameHandler.BuildingType cardType;
	[SerializeField]
	private int buildID = 0;
	[Header("Sound", order = 2)]
	[SerializeField]
	private AudioClip getCardSound;
	[SerializeField]
	private AudioClip grabSound;
	[SerializeField]
	private AudioClip wrongPlacedSound;
	[SerializeField]
	private AudioClip buildSound;
	[SerializeField]
	private AudioClip backInHandSound;

	private AudioSource audioSound;
	private Animator anim;

	private PointerEventData pointerData;
	private bool grabbed = false;
	private Transform cardTransform;
	private Transform parent;
	private CardManager manager;
	private int slot;

	public string BuildingName
	{
		get
		{
			return buildingName;
		}
	}

	public string Description
	{
		get
		{
			return description;
		}
	}

	private void Start()
	{
		anim = GetComponent<Animator>();
		audioSound = GetComponent<AudioSource>();
		cardTransform = transform;
		parent = transform.parent;
		audioSound.clip = getCardSound;
		audioSound.Play();
	}

	public void SetupCard(CardManager man, int slotID)
	{
		manager = man;
		slot = slotID;
	}

	public void SelectCard(BaseEventData data)
	{
		if (manager.CheckIfHaveBuildPoints(cardType))
		{
			if (anim != null) Destroy(anim);
			audioSound.clip = grabSound;
			audioSound.Play();
			pointerData = data as PointerEventData;
			StartCoroutine(FollowFinger());
		}
	}

	public void ReleaseCard()
	{
		if (grabbed)
		{
			grabbed = false;
			manager.CardReleased(cardType, buildID, slot, pointerData.position);
		}
	}

	public void ResetPosition()
	{
		cardTransform.position = parent.position;
	}

	private IEnumerator FollowFinger()
	{
		grabbed = true;
		while (grabbed)
		{
			cardTransform.position = pointerData.position;
			yield return null;
		}
	}
}
