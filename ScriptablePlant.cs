using System;
using UnityEngine;

public class ScriptablePlant : ScriptableObject
{
	[Serializable]
	public class Produce
	{
		public PhotonGameObjectReference prefab;

		public int minProduce;

		public int maxProduce;
	}

	public float fluidNeeded = 1f;

	public GameObject display;

	public ScriptablePlant[] possibleNextGenerations;

	public Produce[] produces;

	private void OnValidate()
	{
		Produce[] array = produces;
		foreach (Produce produce in array)
		{
			produce.prefab.OnValidate();
		}
	}
}
