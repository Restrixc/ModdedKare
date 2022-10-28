using UnityEngine;

public class FaceTowardCamera : MonoBehaviour
{
	public Vector3 forward = Vector3.forward;

	public Vector3 worldUp = Vector3.up;

	public Camera main;

	private void Start()
	{
		main = Camera.main;
	}

	private void Update()
	{
		if (main == null)
		{
			main = Camera.main;
			return;
		}
		Vector3 dir = Vector3.Normalize(main.transform.position - base.transform.position);
		base.transform.rotation = Quaternion.FromToRotation(base.transform.TransformDirection(forward), dir) * base.transform.rotation;
		base.transform.rotation = Quaternion.FromToRotation(base.transform.up, worldUp) * base.transform.rotation;
	}
}
