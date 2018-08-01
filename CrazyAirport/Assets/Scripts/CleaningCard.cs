using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CleaningCard : MonoBehaviour
{
	private PointerEventData pointerData;
	private bool grabbed = false;
	private Transform cardTransform;
	private Transform parent;
	private int id;
	private CardManager cardMan;
	[Header("Sound", order = 2)]
	[SerializeField]
	private AudioClip getCardSound;
	[SerializeField]
	private AudioClip grabSound;
	[SerializeField]
	private AudioClip wrongPlacedSound;
	[SerializeField]
	private AudioClip cleaningSound;
	[SerializeField]
	private AudioClip backInHandSound;
	private AudioSource audioSound;
	private Animator anim;

	private void Start()
	{
		anim = GetComponent<Animator>();
		audioSound = GetComponent<AudioSource>();
		cardTransform = transform;
		parent = transform.parent;
		audioSound.clip = getCardSound;
		audioSound.Play();
	}

	public void SetUp(CardManager manager, int slotID)
	{
		cardMan = manager;
		id = slotID;
	}

	public void SelectCard(BaseEventData data)
	{
		if (anim != null) Destroy(anim);
		audioSound.clip = grabSound;
		audioSound.Play();
		pointerData = data as PointerEventData;
		cardMan.CleanCardSelected();
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
