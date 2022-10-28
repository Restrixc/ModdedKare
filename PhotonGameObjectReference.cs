using System;
using UnityEngine;

[Serializable]
public class PhotonGameObjectReference
{
	public GameObject gameObject;

	public string photonName;

	public void OnValidate()
	{
		if (gameObject != null)
		{
			photonName = gameObject.name;
		}
	}
}
