using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuilder : MonoBehaviour
{
	[SerializeField]
	private int roadCardsAmount = 12;
	[SerializeField]
	private int parkCardsAmount = 6;
	[SerializeField]
	private int lakeCardsAmount = 6;
	[SerializeField]
	private int stopCardsAmount = 4;
	[SerializeField]
	private int controlCardsAmount = 2;
	[SerializeField]
	private int cardCardsAmount = 1;
	[SerializeField]
	private int buildCardsAmount = 1;
	[SerializeField]
	private int landingCardsAmount = 2;
	[SerializeField]
	private int cleanCardsAmount = 2;

	public int ParkCardsAmount
	{
		get
		{
			return parkCardsAmount;
		}

		set
		{
			parkCardsAmount = value;
		}
	}

	public int LakeCardsAmount
	{
		get
		{
			return lakeCardsAmount;
		}

		set
		{
			lakeCardsAmount = value;
		}
	}

	public int StopCardsAmount
	{
		get
		{
			return stopCardsAmount;
		}

		set
		{
			stopCardsAmount = value;
		}
	}

	public int ControlCardsAmount
	{
		get
		{
			return controlCardsAmount;
		}

		set
		{
			controlCardsAmount = value;
		}
	}

	public int CardCardsAmount
	{
		get
		{
			return cardCardsAmount;
		}

		set
		{
			cardCardsAmount = value;
		}
	}

	public int BuildCardsAmount
	{
		get
		{
			return buildCardsAmount;
		}

		set
		{
			buildCardsAmount = value;
		}
	}

	public int LandingCardsAmount
	{
		get
		{
			return landingCardsAmount;
		}

		set
		{
			landingCardsAmount = value;
		}
	}

	public int CleanCardsAmount
	{
		get
		{
			return cleanCardsAmount;
		}

		set
		{
			cleanCardsAmount = value;
		}
	}

	public int RoadCardsAmount
	{
		get
		{
			return roadCardsAmount;
		}

		set
		{
			roadCardsAmount = value;
		}
	}

	void Start()
	{

	}
}
