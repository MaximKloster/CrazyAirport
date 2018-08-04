using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
	#region card variables
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
	[SerializeField]
	private GameObject cleaningCard;
	#endregion
	#region setup variables
	[Header("Setup", order = 2)]
	[SerializeField]
	private Transform[] buildCardSlots;
	[SerializeField]
	private Transform[] cleanCardSlots;
	[SerializeField]
	private int roadCardAmount = 12;
	[SerializeField]
	private GameHandler gameMaster;
	[SerializeField]
	private float gainCardDelay = 0.3f;
	#endregion
	#region cardinfo variables
	[Header("CardInfo", order = 2)]
	[SerializeField]
	private GameObject[] infoObjects;
	[SerializeField]
	private Text[] infoNames;
	[SerializeField]
	private Text[] infoDescriptions;
	#endregion
	#region gameplay variables
	private List<GameObject> deck;
	private GameObject[] handCards;
	private GameObject[] handCleanCards;
	private int currentLevel;
	private int cardsGain = 1;
	private int cleansGain = 1;
	private bool showInfo = false;
	private bool hasBonusCard = false;
	public int CardsGain
	{
		get
		{
			return cardsGain;
		}

		set
		{
			cardsGain = value;
			buildCardSlots[buildCardSlots.Length - 1].gameObject.SetActive(true);
			hasBonusCard = true;
		}
	}
	public int CleansGain
	{
		get
		{
			return cleansGain;
		}

		set
		{
			cleansGain = value;
		}
	}
	#endregion
	#region sound variables
	private AudioSource audioSound;
	#endregion

	void Start()
	{
		foreach (GameObject info in infoObjects)
		{
			info.SetActive(false);
		}
		buildCardSlots[buildCardSlots.Length - 1].gameObject.SetActive(false);
		audioSound = GetComponent<AudioSource>();
		currentLevel = PlayerPrefs.GetInt("Level");
		deck = GenerateDeck();
		StartCoroutine(SetUpStartCards());
	}

	public void ReachedLevelOne()
	{
		List<GameObject> allCards = deck;
		List<GameObject> newDeck = new List<GameObject>();
		allCards.AddRange(collection: stopCards);
		while (allCards.Count > 0)
		{
			int rand = Random.Range(0, allCards.Count);
			newDeck.Add(allCards[rand]);
			allCards.RemoveAt(rand);
		}
		deck.Clear();
		deck = newDeck;
	}

	// Instantiate the Hand Cards and give them the reference to the card manager for cummunication and its slot ID
	private IEnumerator SetUpStartCards()
	{
		handCards = new GameObject[buildCardSlots.Length];
		for (int i = 0; i < buildCardSlots.Length; i++)
		{
			if (i == (buildCardSlots.Length -1) && !hasBonusCard) break;
			TakeCardInHand(i);
			yield return new WaitForSeconds(gainCardDelay);
		}
		infoObjects[infoObjects.Length - 1].SetActive(showInfo);

		handCleanCards = new GameObject[cleanCardSlots.Length];
		for (int i = 0; i < CleansGain; i++)
		{
			if (handCleanCards[i] == null)
			{
				GetCleanCard(i);
				yield return new WaitForSeconds(gainCardDelay);
			}
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
		//allCards.AddRange(collection: stopCards);
		allCards.AddRange(collection: controlCards);
		if (currentLevel > 0) allCards.AddRange(collection: startCards);
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

	// Take clean cards for the gain clean amount but not more than the clean amount (clean gain amount = max hand held amount)
	private void GetCleanCard(int i)
	{
		handCleanCards[i] = Instantiate(cleaningCard, cleanCardSlots[i]);
		handCleanCards[i].GetComponent<CleaningCard>().SetUp(this, i);
	}

	// Take the next card from the deck and instantiate in the hand
	private void TakeCardInHand(int id)
	{
		handCards[id] = Instantiate(deck[0], buildCardSlots[id]);
		handCards[id].GetComponent<BuildCard>().SetupCard(this, id);
		deck.RemoveAt(0);
		SetUpCardInfo(id);
		infoObjects[id].SetActive(showInfo);
	}

	public bool CheckIfHaveBuildPoints(GameHandler.BuildingType cardType)
	{
		return gameMaster.CheckIfHaveBuildPoints(cardType);
	}

	public void NextRound()
	{
		// Only give next cards if the deck has atleast one card
		StartCoroutine(GetPossibleAmountOfCards());
	}

	private IEnumerator GetPossibleAmountOfCards()
	{
		if (deck.Count > 0)
		{
			for (int a = 0; a < CardsGain; a++)
			{
				for (int i = 0; i < buildCardSlots.Length; i++)
				{
					if (i == (buildCardSlots.Length - 1) && !hasBonusCard) break;

					if (handCards[i] == null)
					{
						TakeCardInHand(i);
						yield return new WaitForSeconds(gainCardDelay);
						break;
					}
				}
			}
		}

		for (int i = 0; i < CleansGain; i++)
		{
			if (handCleanCards[i] == null)
			{
				GetCleanCard(i);
				yield return new WaitForSeconds(gainCardDelay);
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

	public void CleanCardReleased(Vector2 pos, int slotID)
	{
		if (gameMaster.TryCleanField(pos))
		{
			Destroy(handCleanCards[slotID]);
		}
		else
		{
			handCleanCards[slotID].GetComponent<CleaningCard>().ResetPosition();
		}
	}

	public void CleanCardSelected()
	{
		gameMaster.HighlightDirtyFields();
	}

	// activate or deactivate the info about the hand cards
	public void ShowCardInfo()
	{
		MenuButtonClicked();
		showInfo = !showInfo;
		for (int i = 0; i < infoObjects.Length - 1; i++)
		{
			if (handCards[i] != null) infoObjects[i].SetActive(showInfo);
		}
		infoObjects[infoObjects.Length - 1].SetActive(showInfo);
	}

	// the card setup is needed for the communication between the card and the card manager
	private void SetUpCardInfo(int id)
	{
		BuildCard card = handCards[id].GetComponent<BuildCard>();
		infoNames[id].text = card.BuildingName;
		infoDescriptions[id].text = card.Description;
	}

	// play sound if a menu button was clicked
	public void MenuButtonClicked()
	{
		audioSound.Play();
	}
}
