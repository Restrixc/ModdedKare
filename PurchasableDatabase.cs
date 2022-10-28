using System.Collections.Generic;
using UnityEngine;

public class PurchasableDatabase : MonoBehaviour
{
	private static PurchasableDatabase instance;

	public List<ScriptablePurchasable> purchasables;

	private static PurchasableDatabase GetInstance()
	{
		if (instance == null)
		{
			instance = Object.FindObjectOfType<PurchasableDatabase>();
		}
		return instance;
	}

	public void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(this);
		}
		else
		{
			instance = this;
		}
	}

	public static ScriptablePurchasable GetPurchasable(string name)
	{
		foreach (ScriptablePurchasable purchasable in GetInstance().purchasables)
		{
			if (purchasable.name == name)
			{
				return purchasable;
			}
		}
		return null;
	}

	public static ScriptablePurchasable GetPurchasable(short id)
	{
		return GetInstance().purchasables[id];
	}

	public static short GetID(ScriptablePurchasable purchasable)
	{
		return (short)GetInstance().purchasables.IndexOf(purchasable);
	}
}
