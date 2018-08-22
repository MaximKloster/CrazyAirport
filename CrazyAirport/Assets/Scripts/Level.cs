using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
	public enum LevelPosition { FIRST, CENTER, LAST }
	[SerializeField]
	private Sprite playImage;
	[SerializeField]
	private Sprite lockedImage;
	[SerializeField]
	private string levelName;
	[SerializeField]
	private string discription;
	[SerializeField]
	private Image image;
	[SerializeField]
	private Image playButton;
	[SerializeField]
	private Image whcButton;
	[SerializeField]
	private Text levelText;
	[SerializeField]
	private Text highScoreText;
	[SerializeField]
	private GameObject loadingAnim;

	private LevelSelection levelMaster;
	private PointerEventData pointerData;
	private Transform levelTransform;
	private float defaultXPos;
	private float defaultScale;
	private Color tempColor;
	private Color tempTextColor;
	private AudioSource audioSource;
	private bool soundAllowed = true;

	private bool isLocked = false;
	private bool selectionPossible = false;
	private bool selected = false;
	private float clickedDifference;
	private LevelPosition levelPos;
	private float borderOffset = 25f;
	private int screenWidth;

	public bool SoundAllowed
	{
		get
		{
			return soundAllowed;
		}

		set
		{
			soundAllowed = value;
		}
	}

	public string LevelName
	{
		get
		{
			return levelName;
		}
	}

	public string Discription
	{
		get
		{
			return discription;
		}
	}

	public bool IsLocked
	{
		get
		{
			return isLocked;
		}

		set
		{
			isLocked = value;
			if (isLocked) playButton.sprite = lockedImage;
			else playButton.sprite = playImage;
		}
	}

	private void Awake()
	{
		levelTransform = transform;
		defaultXPos = levelTransform.position.x;
	}

	private void Start()
	{
		loadingAnim.SetActive(false);
		audioSource = GetComponent<AudioSource>();
		screenWidth = Screen.width;
	}

	public void SetUp(LevelSelection master, float scale, float visible, LevelPosition pos = LevelPosition.CENTER, bool hideAll = false)
	{
		defaultScale = scale;
		levelPos = pos;
		tempColor = image.color;
		levelMaster = master;
		tempColor.a = visible;
		image.color = tempColor;
		levelTransform.localScale = new Vector3(scale, scale, scale);
		SetItemsVisibility(hideAll);
	}

	private void SetItemsVisibility(bool hide)
	{
		tempTextColor = levelText.color;
		if (hide)
		{
			tempTextColor.a = 0;
			tempColor.a = 0;
		}
		else
		{
			tempColor.a = 1;
			tempTextColor.a = 1;
		}
		whcButton.color = tempColor;
		playButton.color = tempColor;
		levelText.color = tempTextColor;
		highScoreText.color = tempTextColor;
	}

	void Update()
	{
		if (selected)
		{
			Vector3 newPos;
			if (levelPos == LevelPosition.FIRST) newPos = new Vector3(Mathf.Clamp(pointerData.position.x - clickedDifference, 0, defaultXPos + borderOffset), levelTransform.position.y);
			else if (levelPos == LevelPosition.LAST) newPos = new Vector3(Mathf.Clamp(pointerData.position.x - clickedDifference, defaultXPos - borderOffset, screenWidth), levelTransform.position.y);
			else newPos = new Vector3(Mathf.Clamp(pointerData.position.x - clickedDifference, 0, screenWidth), levelTransform.position.y);
			levelTransform.position = newPos;
			levelMaster.MovedLevel(newPos.x);
		}
	}

	public void Interaction(bool interactable)
	{
		selectionPossible = interactable;
	}

	public void SelectedMove(float toPos, float scale = 1)
	{
		levelTransform.position = new Vector3(toPos, levelTransform.position.y);
		levelTransform.localScale = new Vector3(scale, scale, scale);
	}

	public void MoveToNextView(float destination, float doneFactor, float scale, float visibility = 1)
	{
		// Movement
		float distance = destination - defaultXPos;
		float movement = distance * Mathf.Clamp(doneFactor, 0, 1);
		levelTransform.position = new Vector3(defaultXPos + movement, levelTransform.position.y);

		// Size
		float newScale = defaultScale + (scale - defaultScale) * doneFactor;
		levelTransform.localScale = new Vector3(newScale, newScale, newScale);

		// Visibility
		tempColor.a = Mathf.Clamp(visibility, 0, 1);
		tempTextColor.a = Mathf.Clamp(visibility, 0, 1);
		image.color = tempColor;
		whcButton.color = tempColor;
		playButton.color = tempColor;
		levelText.color = tempTextColor;
		highScoreText.color = tempTextColor;
	}

	public void Selected(BaseEventData data)
	{
		if (selectionPossible)
		{
			pointerData = data as PointerEventData;
			clickedDifference = pointerData.position.x - levelTransform.position.x;
			selected = true;
		}
	}

	public void Released()
	{
		if (selectionPossible)
		{
			levelMaster.ReleasedSelected(levelTransform.position.x);
			selected = false;
		}
	}

	public void SetParentDirection(Transform newParent)
	{
		levelTransform.SetParent(newParent);
	}

	public void SetDefaults(float defaultX, float defaultScaling)
	{
		defaultXPos = defaultX;
		defaultScale = defaultScaling;
		selectionPossible = false;
	}

	public void EnableSelection()
	{
		selectionPossible = true;
	}

	public void SelectedParent(Transform parent)
	{
		levelTransform.SetParent(parent);
	}

	public void PlayButtonClicked()
	{
		if (!IsLocked)
		{
			if (SoundAllowed) audioSource.Play();
			loadingAnim.SetActive(true);
			selectionPossible = false;
			levelMaster.PlayButtonClicked();
		}
	}

	public void WHCButtonClicked()
	{
		if (SoundAllowed) audioSource.Play();
		levelMaster.WHCButtonClicked();
	}
}
