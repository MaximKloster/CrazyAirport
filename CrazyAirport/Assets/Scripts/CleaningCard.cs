using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CleaningCard : MonoBehaviour
{
	[SerializeField]
	private string tokenName = "";
	[SerializeField]
	private string discription = "";
	[SerializeField]
	private CanvasGroup canGroup;
	private PointerEventData pointerData;
	private bool grabbed = false;
	private Transform cardTransform;
	private Transform parent;
	private int id;
	private CardManager cardMan;
	private Animator anim;
	private bool showInfo = false;
	private float infoShowBorder;
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
	private bool allowSound = true;
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

	public string TokenName
	{
		get
		{
			return tokenName;
		}
	}

	public string Discription
	{
		get
		{
			return discription;
		}
	}

	private void Start()
	{
		anim = GetComponent<Animator>();
		audioSound = GetComponent<AudioSource>();
		cardTransform = transform;
		parent = transform.parent;
		if (AllowSound)
		{
			audioSound.clip = getCardSound;
			audioSound.Play();
		}
	}

	public void SetUp(CardManager manager, int slotID, float border, bool sound)
	{
		AllowSound = sound;
		infoShowBorder = border;
		cardMan = manager;
		id = slotID;
	}

	public void SelectCard(BaseEventData data)
	{
		if (anim != null) Destroy(anim);
		if (AllowSound)
		{
			audioSound.clip = grabSound;
			audioSound.Play();
		}
		pointerData = data as PointerEventData;
		cardMan.CleanCardSelected(this);
		StartCoroutine(FollowFinger());
	}

	public void ReleaseCard()
	{
		if (grabbed)
		{
			grabbed = false;
			cardMan.CleanCardReleased(pointerData.position, id);
		}
	}

	public void ResetPosition()
	{
		cardTransform.position = parent.position;
	}

	public void SetVisibility(bool visible)
	{
		if (visible) canGroup.alpha = 1;
		else canGroup.alpha = 0;
	}

	private IEnumerator FollowFinger()
	{
		grabbed = true;
		while (grabbed)
		{
			if (showInfo && cardTransform.position.y > infoShowBorder)
			{
				cardMan.CardOutOfBorder(true);
				showInfo = false;
			}
			else if (!showInfo && cardTransform.position.y < infoShowBorder)
			{
				cardMan.CardOutOfBorder(false);
				showInfo = true;
			}
			cardTransform.position = pointerData.position;
			yield return null;
		}
	}
}
