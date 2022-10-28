using UnityEngine;

public class PlantSpawnEventHandler : MonoBehaviour
{
	public delegate void PlantSpawnEventAction(GameObject obj, ScriptablePlant plant);

	private static PlantSpawnEventHandler instance;

	private event PlantSpawnEventAction planted;

	private void Awake()
	{
		instance = this;
	}

	public static void AddListener(PlantSpawnEventAction action)
	{
		instance.planted += action;
	}

	public static void RemoveListener(PlantSpawnEventAction action)
	{
		instance.planted -= action;
	}

	public static void TriggerPlantSpawnEvent(GameObject obj, ScriptablePlant plant)
	{
		instance.planted?.Invoke(obj, plant);
	}
}
