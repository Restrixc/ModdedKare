using UnityEngine;

public class DelayedDelete : MonoBehaviour
{
	public float delay;

	private void Start()
	{
		Object.Destroy(base.gameObject, delay);
	}
}
