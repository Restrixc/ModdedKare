using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PreparePool : MonoBehaviour
{
	public List<GameObject> Prefabs;

	private void Start()
	{
		if (!(PhotonNetwork.PrefabPool is DefaultPool pool) || Prefabs == null)
		{
			return;
		}
		foreach (GameObject prefab in Prefabs)
		{
			pool.ResourceCache.Add(prefab.name, prefab);
		}
	}
}
