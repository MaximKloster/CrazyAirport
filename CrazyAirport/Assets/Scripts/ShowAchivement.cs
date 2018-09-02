using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowAchivement : MonoBehaviour
{
	[SerializeField]
	private GameObject NameText;
	[SerializeField]
	private GameObject descriptionText;
	[SerializeField]
	private GameObject StatsText;

	private int viewState = 0;

	private void Start()
	{
		descriptionText.SetActive(false);

	}
	public void ChangeView()
	{

	}

}
