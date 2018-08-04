using UnityEngine;

public class InfoToggle : MonoBehaviour
{
	[Header("SetUp", order =2)]
	[SerializeField]
	private PlaneManager planeMan;
	[SerializeField]
	private CardManager cardMan;

	private int state = 0;
	// Use this for initialization
	void Start()
	{

	}
	
	public void Clicked()
	{
		state++;
		if (state > 2) state = 0;
		switch(state)
		{
			case 0:
				planeMan.ShowMovementFeedback();
				break;
			case 1:
				planeMan.ShowMovementFeedback();
				cardMan.ShowCardInfo();
				break;
			case 2:
				cardMan.ShowCardInfo();
				break;

		}
	}
}
