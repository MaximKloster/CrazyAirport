using UnityEngine;

public class InfoToggle : MonoBehaviour
{
	[Header("SetUp", order =2)]
	[SerializeField]
	private PlaneManager planeMan;
	[SerializeField]
	private CardManager cardMan;

	private int state = 0;

	public void Clicked()
	{
		state++;
		if (state > 2) state = 0;
		switch(state)
		{
			case 0:
				cardMan.ShowCardInfo();
				break;
			case 1:
				planeMan.ShowMovementFeedback();
				cardMan.ShowCardInfo();
				break;
			case 2:
				planeMan.ShowMovementFeedback();
				break;

		}
	}
}
