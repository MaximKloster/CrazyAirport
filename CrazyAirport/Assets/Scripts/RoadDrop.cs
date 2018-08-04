using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadDrop : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem dropPS;
	
	public void Drop()
	{
		dropPS.Play();
	}

}
