using System.Collections.Generic;
using KoboldKare;
using UnityEngine;

public class SpoilableHandler : MonoBehaviour
{
	[SerializeField]
	private GameEventGeneric midnightEvent;

	[SerializeField]
	private LayerMask safeZoneMask;

	private List<ISpoilable> spoilables = new List<ISpoilable>();

	private static SpoilableHandler instance;

	public static LayerMask GetSafeZoneMask()
	{
		return instance.safeZoneMask;
	}

	private void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
		}
	}

	private void Start()
	{
		midnightEvent.AddListener(OnMidnight);
	}

	private void OnDestroy()
	{
		midnightEvent.RemoveListener(OnMidnight);
	}

	private void OnMidnight(object nothing)
	{
		foreach (ISpoilable spoilable in spoilables)
		{
			int hitCount = 0;
			RaycastHit[] array = Physics.RaycastAll(spoilable.transform.position + Vector3.up * 400f, Vector3.down, 400f, safeZoneMask, QueryTriggerInteraction.Collide);
			foreach (RaycastHit h in array)
			{
				hitCount++;
			}
			if (hitCount % 2 == 0)
			{
				spoilable.OnSpoil();
			}
		}
	}

	public static void AddSpoilable(ISpoilable spoilable)
	{
		instance.spoilables.Add(spoilable);
	}

	public static void RemoveSpoilable(ISpoilable spoilable)
	{
		if (instance.spoilables.Contains(spoilable))
		{
			instance.spoilables.Remove(spoilable);
		}
	}
}
