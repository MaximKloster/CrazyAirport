using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetRenderMesh : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer mesh;
	
	public MeshRenderer GetMesh()
	{
		return mesh;
	}
}
