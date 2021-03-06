﻿using System.Collections.Generic;
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
	private GameObject parkCard;
	[SerializeField]
	private GameObject lakeCard;
	[SerializeField]
	private GameObject stopCard;
	[SerializeField]
	private GameObject controlCard;
	[SerializeField]
	private GameObject startCard;
	[SerializeField]
	private GameObject landCard;
	[SerializeField]
	private GameObject cleanCard;
	[SerializeField]
	private GameObject cleaningCard;
	#endregion
	#region setup variables
	[Header("Setup", order = 2)]
	[SerializeField]
	private Transform cardBorder;
	private float yBorder;
	[SerializeField]
	private Transform[] buildCardSlots;
	[SerializeField]
	private Transform[] cleanCardSlots;
	[SerializeField]
	private int roadCardAmount = 12;
	[SerializeField]
	private float gainCardDelay = 0.3f;
	#endregion
	#region cardinfo variables
	[Header("CardInfo", order = 2)]
	[SerializeField]
	private GameObject infoCard;
	[SerializeField]
	private Image infoImage;
	[SerializeField]
	private GameObject tokenImage;
	[SerializeField]
	private Text infoName;
	[SerializeField]
	private Text infoText;
	[SerializeField]
	private Sprite[] allInfoSprites;
	#endregion
	#region gameplay variables
	private GameHandler gameMaster;
	private DeckBuilder deckBuilder;
	private List<GameObject> deck;
	private BuildCard[] handCards;
	private CleaningCard[] handCleanCards;
	private int currentLevel;
	private int cardsGain = 1;
	private int cleansGain = 1;
	private bool hasBonusCard = false;
	private int stopCardsPossible;
	private int stopCardsInDeck = 0;
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
			if (handCards != null)
			{
				foreach (BuildCard card in handCards)
				{
					if (card != null) card.AllowSound = allowSound;
				}
			}
			if (handCleanCards != null)
			{
				foreach (CleaningCard clean in handCleanCards)
				{
					if (clean != null) clean.AllowSound = allowSound;
				}
			}
		}
	}
	#endregion

	void Start()
	{
		yBorder = cardBorder.position.y;
		infoCard.SetActive(false);
		buildCardSlots[buildCardSlots.Length - 1].gameObject.SetActive(false);
		audioSound = GetComponent<AudioSource>();
		currentLevel = PlayerPrefs.GetInt("Level");
		deck = GenerateDeck();
		StartCoroutine(SetUpStartCards());
	}

	public void GetSetUpParts(GameHandler gameHandler, DeckBuilder newDeckBuilder)
	{
		gameMaster = gameHandler;
		deckBuilder = newDeckBuilder;
	}

	public void AddStopCard()
	{
		if (stopCardsInDeck < stopCardsPossible)
		{
			List<GameObject> allCards = deck;
			allCards.Add(stopCard);
			stopCardsInDeck++;
			List<GameObject> newDeck = new List<GameObject>();
			while (allCards.Count > 0)
			{
				int rand = Random.Range(0, allCards.Count);
				newDeck.Add(allCards[rand]);
				allCards.RemoveAt(rand);
			}
			deck.Clear();
			deck = newDeck;
		}
	}

	// Instantiate the Hand Cards and give them the reference to the card manager for cummunication and its slot ID
	private IEnumerator SetUpStartCards()
	{
		handCleanCards = new CleaningCard[cleanCardSlots.Length];
		for (int i = 0; i < CleansGain; i++)
		{
			if (handCleanCards[i] == null)
			{
				GetCleanCard(i);
				yield return new WaitForSeconds(gainCardDelay);
			}
		}

		handCards = new BuildCard[buildCardSlots.Length];
		for (int i = 0; i < buildCardSlots.Length; i++)
		{
			if (i == (buildCardSlots.Length - 1) && !hasBonusCard) break;
			TakeCardInHand(i);
			yield return new WaitForSeconds(gainCardDelay);
		}

		gameMaster.SupportFinished();
	}

	// Take all cards and generate a random deck of this cards
	private List<GameObject> GenerateDeck()
	{
		List<GameObject> allCards = new List<GameObject>();
		for (int i = 0; i < deckBuilder.RoadCardsAmount; i++)
		{
			allCards.Add(roadCard);
		}
		for (int i = 0; i < deckBuilder.ParkCardsAmount; i++)
		{
			allCards.Add(parkCard);
		}
		for (int i = 0; i < deckBuilder.LakeCardsAmount; i++)
		{
			allCards.Add(lakeCard);
		}
		for (int i = 0; i < deckBuilder.ControlCardsAmount; i++)
		{
			allCards.Add(controlCard);
		}
		for (int i = 0; i < deckBuilder.BuildCardsAmount; i++)
		{
			allCards.Add(buildCard);
		}
		for (int i = 0; i < deckBuilder.CardCardsAmount; i++)
		{
			allCards.Add(cardCard);
		}
		for (int i = 0; i < deckBuilder.LandingCardsAmount; i++)
		{
			allCards.Add(landCard);
		}
		for (int i = 0; i < deckBuilder.CleanCardsAmount; i++)
		{
			allCards.Add(cleanCard);
		}

		List<GameObject> newDeck = new List<GameObject>();

		while (allCards.Count > 0)
		{
			int rand = Random.Range(0, allCards.Count);
			newDeck.Add(allCards[rand]);
			allCards.RemoveAt(rand);
		}

		stopCardsPossible = deckBuilder.StopCardsAmount;

		return newDeck;
	}

	// Take clean cards for the gain clean amount but not more than the clean amount (clean gain amount = max hand held amount)
	private void GetCleanCard(int i)
	{
		GameObject card = Instantiate(cleaningCard, cleanCardSlots[i]);
		handCleanCards[i] = card.GetComponent<CleaningCard>();
		handCleanCards[i].SetUp(this, i, yBorder, AllowSound);
	}

	// Take the next card from the deck and instantiate in the hand
	private void TakeCardInHand(int id)
	{
		GameObject card = Instantiate(deck[0], buildCardSlots[id]);
		handCards[id] = card.GetComponent<BuildCard>();
		handCards[id].SetupCard(this, id, yBorder, AllowSound);
		deck.RemoveAt(0);
	}

	public bool CheckIfHaveBuildPoints(BuildCard card)
	{
		infoText.text = card.Description;
		infoName.text = card.BuildingName;
		tokenImage.SetActive(false);
		infoCard.SetActive(true);
		card.transform.parent.transform.SetAsLastSibling();

		bool build = gameMaster.CheckIfHaveBuildPoints(card);
		
		switch (card.CardType)
		{
			case GameHandler.BuildingType.Road:
				infoImage.sprite = allInfoSprites[0];
				break;
			case GameHandler.BuildingType.Land:
				infoImage.sprite = allInfoSprites[1];
				break;
			case GameHandler.BuildingType.Park:
				if (card.BuildID == 0) infoImage.sprite = allInfoSprites[3];
				else infoImage.sprite = allInfoSprites[2];
				break;
			case GameHandler.BuildingType.Stop:
				infoImage.sprite = allInfoSprites[4];
				break;
			case GameHandler.BuildingType.Clean:
				infoImage.sprite = allInfoSprites[5];
				break;
			case GameHandler.BuildingType.Build:
				infoImage.sprite = allInfoSprites[6];
				break;
			case GameHandler.BuildingType.Control:
				infoImage.sprite = allInfoSprites[7];
				break;
			case GameHandler.BuildingType.Card:
				infoImage.sprite = allInfoSprites[8];
				break;
			case GameHandler.BuildingType.Start:
				infoImage.sprite = allInfoSprites[9];
				break;
		}
		return build;
	}

	public void NextRound()
	{
		// Only give next cards if the deck has atleast one card
		StartCoroutine(GetPossibleAmountOfCards());
	}

	private IEnumerator GetPossibleAmountOfCards()
	{
		for (int i = 0; i < CleansGain; i++)
		{
			if (handCleanCards[i] == null)
			{
				GetCleanCard(i);
				yield return new WaitForSeconds(gainCardDelay);
			}
		}

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
		yield return null;
		gameMaster.SupportFinished();
	}

	/** Handle what to do with the released card
	 *  If the card is released at a spot were it can be build, than the card will be removed from the hand cards
	 *  else the card moves back to its hand position were it was before
	 **/
	public void CardReleased(GameHandler.BuildingType type, int buildID, int slotID, Vector2 pos)
	{
		if (infoCard.activeSelf) infoCard.SetActive(false);

		if (gameMaster.TryToBuildAt(pos, type, buildID))
		{
			Destroy(handCards[slotID].gameObject);
		}
		else
		{
			handCards[slotID].ResetPosition();
		}
	}

	public void DisableInfo()
	{
		if (infoCard.activeSelf) infoCard.SetActive(false);
	}

	public void CleanCardReleased(Vector2 pos, int slotID)
	{
		if (infoCard.activeSelf) infoCard.SetActive(false);

		if (gameMaster.TryCleanField(pos))
		{
			Destroy(handCleanCards[slotID].gameObject);
		}
		else
		{
			handCleanCards[slotID].ResetPosition();
		}
	}

	public void CleanCardSelected(CleaningCard token)
	{
		tokenImage.SetActive(true);
		infoImage.sprite = allInfoSprites[10];
		infoText.text = token.Discription;
		infoName.text = token.TokenName;
		infoCard.SetActive(true);
		gameMaster.HighlightDirtyFields(token);
	}

	public void CardOutOfBorder(bool outOfBorder)
	{
		infoCard.SetActive(!outOfBorder);
	}

	// play sound if a menu button was clicked
	public void MenuButtonClicked()
	{
		if (AllowSound) audioSound.Play();
	}
}
