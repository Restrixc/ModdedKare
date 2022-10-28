using UnityEngine;

public class RotateByTime : MonoBehaviour
{
	public Vector3 axis = new Vector3(0f, 0f, 1f);

	public float offset = 0f;

	public float multiplier = 1f;

	private void FixedUpdate()
	{
		base.transform.rotation = Quaternion.AngleAxis(offset + Time.time * 360f * multiplier, axis);
	}
}
