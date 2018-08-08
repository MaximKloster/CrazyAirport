using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CamChangerButton : MonoBehaviour
{
	[SerializeField]
	private Image image;
	[SerializeField]
	private Sprite cam3dSprite;
	[SerializeField]
	private Sprite cam2dSprite;
	private bool freeView = true;

	public void Clicked()
	{
		if(freeView)
		{
			image.sprite = cam3dSprite;
		}
		else
		{
			image.sprite = cam2dSprite;
		}
		freeView = !freeView;
	}
}
