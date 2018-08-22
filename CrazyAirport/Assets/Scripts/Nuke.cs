using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nuke : MonoBehaviour
{
	[SerializeField]
	private Light halo;
	[SerializeField]
	private ParticleSystem flameUp;
	[SerializeField]
	private ParticleSystem top;

	private float topSpeed = 0.55f;
	// Use this for initialization
	void Start()
	{
		StartCoroutine(BombActivity());
	}

	private IEnumerator BombActivity()
	{
		halo.gameObject.SetActive(true);
		while (halo.intensity < 30)
		{
			top.gameObject.transform.Translate(top.gameObject.transform.up * Time.deltaTime * topSpeed);
			halo.intensity += 2;
			yield return null;
		}
		flameUp.Play();
		while (halo.intensity < 100)
		{
			if(top.gameObject.transform.position.y < 1.1f)top.gameObject.transform.Translate(top.gameObject.transform.up * Time.deltaTime * topSpeed);
			halo.intensity += 2;
			yield return null;
		}

		while (halo.intensity > 1)
		{
			if (top.gameObject.transform.position.y < 1.1f) top.gameObject.transform.Translate(top.gameObject.transform.up * Time.deltaTime * topSpeed);
			halo.intensity -= 0.5f;
			yield return null;
		}
		halo.gameObject.SetActive(false);

		yield return new WaitForSeconds(1);
		gameObject.SetActive(false);
	}
}
