using UnityEngine;
using UnityEngine.Events;

public class DelayTrigger : MonoBehaviour
{
	public UnityEvent onTrigger;

	public float waitTime = 1f;

	public float waitVariance = 1f;

	private float timer = 0f;

	private void Start()
	{
		waitTime += Random.Range(0f - waitVariance, waitVariance);
	}

	private void FixedUpdate()
	{
		timer += Time.fixedDeltaTime;
		if (timer > waitTime)
		{
			onTrigger.Invoke();
			Object.Destroy(this);
		}
	}
}
