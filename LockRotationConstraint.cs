using UnityEngine;

public class LockRotationConstraint : MonoBehaviour
{
	private Vector3 upDir = Vector3.forward;

	private void Update()
	{
		base.transform.rotation = Quaternion.FromToRotation(base.transform.TransformDirection(upDir), Vector3.up) * base.transform.rotation;
	}
}
