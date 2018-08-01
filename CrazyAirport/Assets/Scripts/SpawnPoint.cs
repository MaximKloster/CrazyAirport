using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position, new Vector3(0.1f, 0.1f, 0.1f));
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, transform.position +transform.forward);
	}
}
