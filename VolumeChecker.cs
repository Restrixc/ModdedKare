using KoboldKare;
using UnityEngine;

public class VolumeChecker : MonoBehaviour
{
	public GameEventGeneric trigger;

	private bool hasPlayer = false;

	private void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.CompareTag("Player"))
		{
			hasPlayer = true;
		}
	}

	private void OnTriggerExit(Collider c)
	{
		if (c.gameObject.CompareTag("Player"))
		{
			hasPlayer = false;
		}
	}

	public void TriggerIfNotInside()
	{
		if (!hasPlayer)
		{
			trigger.Raise(null);
		}
	}
}
