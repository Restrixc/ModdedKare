using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu]
public class PachinkoPrizeList : ScriptableObject
{
	[Serializable]
	public class PrizeEntry
	{
		public ScriptablePurchasable prize;

		public float chance;
	}

	[SerializeField]
	private List<PrizeEntry> prizes = new List<PrizeEntry>();

	public List<PrizeEntry> GetPrizes()
	{
		return prizes;
	}
}
