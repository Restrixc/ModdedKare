using System.Collections.Generic;
using UnityEngine;

public class RandomizedPurchasable : GenericPurchasable
{
	[SerializeField]
	private List<ScriptablePurchasable> randomizePool;

	public override void OnRestock(object nothing)
	{
		SwapTo(randomizePool[Random.Range(0, randomizePool.Count)]);
		base.OnRestock(nothing);
	}
}
