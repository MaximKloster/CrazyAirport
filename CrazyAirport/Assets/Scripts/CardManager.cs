using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
	[Header("Cards", order = 2)]
	[SerializeField]
	private GameObject roadCard;
	[SerializeField]
	private GameObject buildCard;
	[SerializeField]
	private GameObject cardCard;
	[SerializeField]
	private GameObject[] parkCards;
	[SerializeField]
	private GameObject[] stopCards;
	[SerializeField]
	private GameObject[] controlCards;
	[SerializeField]
	private GameObject[] startCards;
	[SerializeField]
	private GameObject[] landCards;
	[SerializeField]
	private GameObject[] cleanCards;

	[Header("Setup", order = 2)]
	[SerializeField]
	private Transform[] slots;
	[SerializeField]
	private int roadCardAmount = 12;
	[SerializeField]
	private GameHandler gameMaster;

	[Header("CardInfo", order = 2)]
	[SerializeField]
	private GameObject[] infoObjects;
	[SerializeField]
	private Text[] infoNames;
	[SerializeField]
	private Text[] infoDescriptions;

	private List<GameObject> deck;
	private GameObject[] handCards;
	private int cardsGain = 1;
	private bool showInfo = false;

	// Use this for initialization
	void Start()
	{
		deck = GenerateDeck();
		SetUpStartCards();
	}

	// Instantiate the Hand Cards and give them the reference to the card manager for cummunication and its slot ID
	private void SetUpStartCards()
	{
		handCards = new GameObject[4];
		for (int i = 0; i < 4; i++)
		{
			TakeCardInHand(i);
		}
	}

	// Take all cards and generate a random deck of this cards
	private List<GameObject> GenerateDeck()
	{
		List<GameObject> allCards = new List<GameObject>();
		for (int i = 0; i < roadCardAmount; i++) // Add road Cards to the deck for the amount was set up
		{
			allCards.Add(roadCard);
		}
		allCards.Add(buildCard);
		allCards.Add(cardCard);
		allCards.AddRange(collection: parkCards);
		allCards.AddRange(collection: stopCards);
		allCards.AddRange(collection: controlCards);
		allCards.AddRange(collection: startCards);
		allCards.AddRange(collection: landCards);
		allCards.AddRange(collection: cleanCards);

		List<GameObject> newDeck = new List<GameObject>();

		while (allCards.Count > 0)
		{
			int rand = Random.Range(0, allCards.Count);
			newDeck.Add(allCards[rand]);
			allCards.RemoveAt(rand);
		}

		return newDeck;
	}

	// Take the next card from the deck and instantiate in the hand
	private void TakeCardInHand(int id)
	{
		handCards[id] = Instantiate(deck[0], slots[id]);
		handCards[id].GetComponent<BuildCard>().SetupCard(this, id);
		deck.RemoveAt(0);
		SetUpCardInfo(id);
		infoObjects[id].SetActive(showInfo);
	}

	public void NextRound()
	{
		// Only give next cards if the deck has atleast one card
		if (deck.Count > 0)
		{
			for (int a = 0; a < cardsGain; a++)
			{
				for(int i = 0; i < 4; i++)
				{
					if(handCards[i] == null)
					{
						TakeCardInHand(i);
						break;
					}
				}
			}
		}
	}

	/** Handle what to do with the released card
	 *  If the card is released at a spot were it can be build, than the card will be removed from the hand cards
	 *  else the card moves back to its hand position were it was before
	 **/
	public void CardReleased(GameHandler.BuildingType type, int buildID, int slotID, Vector2 pos)
	{
		if (gameMaster.TryToBuildAt(pos, type, buildID))
		{
			Destroy(handCards[slotID]);
			if (showInfo) infoObjects[slotID].SetActive(false);
		}
		else
		{
			handCards[slotID].GetComponent<BuildCard>().ResetPosition();
		}
	}

	// activate or deactivate the info about the hand cards
	public void ShowCardInfo()
	{
		showInfo = !showInfo;
		for(int i = 0; i < 4; i++)
		{
			if (handCards[i] != null) infoObjects[i].SetActive(showInfo);
		}
	}

	// the card setup is needed for the communication between the card and the card manager
	private void SetUpCardInfo(int id)
	{
		BuildCard card = handCards[id].GetComponent<BuildCard>();
		infoNames[id].text = card.BuildingName;
		infoDescriptions[id].text = card.Description;
	}
}
