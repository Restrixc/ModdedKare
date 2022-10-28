using UnityEngine;

public class DecalLifetime : MonoBehaviour
{
	public float lifetime = 60f;

	private float dieTime;

	private void Start()
	{
		dieTime = Time.timeSinceLevelLoad + lifetime;
	}

	private void Update()
	{
		if (Time.timeSinceLevelLoad > dieTime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
