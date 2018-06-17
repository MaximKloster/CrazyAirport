using System.Collections;
using System.Collections.Generic;
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
		cardTransform = transform;
		parent = transform.parent;
	}

	public void SetupCard(CardManager man, int slotID)
	{
		manager = man;
		slot = slotID;
	}

	public void SelectCard(BaseEventData data)
	{
		pointerData = data as PointerEventData;
		StartCoroutine(FollowFinger());
	}

	public void ReleaseCard()
	{
		grabbed = false;
		manager.CardReleased(cardType, buildID, slot, pointerData.position);
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
