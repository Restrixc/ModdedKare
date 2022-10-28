using UnityEngine;
using UnityEngine.Events;

public class UnityEventOnStart : MonoBehaviour
{
	public UnityEvent OnStart;

	private void Start()
	{
		OnStart.Invoke();
	}
}
