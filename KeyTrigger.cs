using KoboldKare;
using UnityEngine;

public class KeyTrigger : MonoBehaviour
{
	public GameEventGeneric KeyDown;

	public GameEventGeneric KeyUp;

	public string key;

	private void Update()
	{
		if (Input.GetButtonDown(key))
		{
			KeyDown?.Raise(null);
		}
		if (Input.GetButtonUp(key))
		{
			KeyUp?.Raise(null);
		}
	}
}
