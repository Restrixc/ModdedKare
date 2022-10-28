using UnityEngine;

public class FarmSpawnEventHandler : MonoBehaviour
{
	public delegate void ProduceSpawnedAction(GameObject produce);

	private static FarmSpawnEventHandler instance;

	private event ProduceSpawnedAction producedSpawned;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public static void TriggerProduceSpawn(GameObject produce)
	{
		instance.producedSpawned?.Invoke(produce);
	}

	public static void AddListener(ProduceSpawnedAction action)
	{
		instance.producedSpawned += action;
	}

	public static void RemoveListener(ProduceSpawnedAction action)
	{
		instance.producedSpawned -= action;
	}
}
