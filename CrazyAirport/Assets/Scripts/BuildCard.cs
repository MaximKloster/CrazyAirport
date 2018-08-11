using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildCard : MonoBehaviour
{
	#region card setup variables
	[Header("Card Setup", order = 2)]
	[SerializeField]
	private CanvasGroup canGroup;
	[SerializeField]
	private string buildingName = "";
	[SerializeField]
	private string description = "";
	[SerializeField]
	private GameHandler.BuildingType cardType;
	[SerializeField]
	private int buildID = 0;
	#endregion
	#region sound variables
	[Header("Sound", order = 2)]
	[SerializeField]
	private AudioClip getCardSound;
	[SerializeField]
	private AudioClip grabSound;
	[SerializeField]
	private AudioClip wrongPlacedSound;
	[SerializeField]
	private AudioClip backInHandSound;
	private AudioSource audioSound;
	#endregion

	private Animator anim;
	private PointerEventData pointerData;
	private bool grabbed = false;
	private Transform cardTransform;
	private Transform parent;
	private CardManager manager;
	private int slot;
	private bool showInfo = false;
	private float infoShowBorder;

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

	public GameHandler.BuildingType CardType
	{
		get
		{
			return cardType;
		}

		private set
		{
			cardType = value;
		}
	}

	public int BuildID
	{
		get
		{
			return buildID;
		}

		private set
		{
			buildID = value;
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

	public void SetupCard(CardManager man, int slotID, float border)
	{
		infoShowBorder = border;
		manager = man;
		slot = slotID;
	}

	public void SelectCard(BaseEventData data)
	{
		if (manager.CheckIfHaveBuildPoints(this))
		{
			if (anim != null) Destroy(anim);
			showInfo = true;
			audioSound.clip = grabSound;
			audioSound.Play();
			pointerData = data as PointerEventData;
			StartCoroutine(FollowFinger());
		}
	}

	public void SetVisibility(bool visible)
	{
		if (visible) canGroup.alpha = 1;
		else canGroup.alpha = 0;
	}

	public void ReleaseCard()
	{
		if (grabbed)
		{
			grabbed = false;
			manager.CardReleased(CardType, BuildID, slot, pointerData.position);
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
			if(showInfo && cardTransform.position.y > infoShowBorder)
			{
				manager.CardOutOfBorder(true);
				showInfo = false;
			}
			else if(!showInfo && cardTransform.position.y < infoShowBorder)
			{
				manager.CardOutOfBorder(false);
				showInfo = true;
			}
			cardTransform.position = pointerData.position;
			yield return null;
		}
	}
}
