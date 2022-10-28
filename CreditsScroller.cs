using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
	[SerializeField]
	private float speed;

	[SerializeField]
	private float height;

	private float startTime = 0f;

	private void OnEnable()
	{
		startTime = Time.unscaledTime;
	}

	private void Update()
	{
		float currentPosition = Mathf.Repeat((Time.unscaledTime - startTime) * speed, height + 100f) - 100f;
		Transform obj = base.transform;
		Vector3 position = base.transform.position;
		float? y = currentPosition;
		obj.position = position.With(null, y);
	}
}
