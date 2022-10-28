using UnityEngine;

public class AimAtJoint : MonoBehaviour
{
	public Rigidbody body;

	public Transform connectedBody;

	public Vector3 localAnchor;

	public Vector3 localConnectedAnchor;

	public Vector3 localConnectedAimVector;

	public Transform localTransform;

	public Vector3 localAimVector;

	public Vector3 localUpVector;

	public Vector3 worldUpVector;

	public float springStrength = 10f;

	public float softness = 90f;

	public float dampingStrength = 1f;

	[Range(0f, 1f)]
	public float weight = 1f;

	private void Start()
	{
		body = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (!(body == null) && !(connectedBody == null) && weight != 0f && !(localTransform == null))
		{
			Vector3 selfPosition = body.transform.TransformPoint(localAnchor);
			Vector3 otherPosition = connectedBody.TransformPoint(localConnectedAnchor);
			float distanceAdjust = Mathf.Clamp01(1f - Vector3.Distance(selfPosition, otherPosition) * 2f);
			otherPosition += localTransform.TransformDirection(localConnectedAimVector) * distanceAdjust;
			float fangle = 0f;
			Vector3 faxis = Vector3.zero;
			Quaternion.FromToRotation(body.transform.TransformDirection(localAimVector), (otherPosition - selfPosition).normalized).ToAngleAxis(out fangle, out faxis);
			if (fangle >= 180f)
			{
				fangle = 360f - fangle;
				faxis = -faxis;
			}
			float uangle = 0f;
			Vector3 uaxis = Vector3.zero;
			Quaternion.FromToRotation(Vector3.ProjectOnPlane(planeNormal: Vector3.ProjectOnPlane(body.transform.TransformDirection(localAimVector), worldUpVector).normalized, vector: body.transform.TransformDirection(localUpVector)).normalized, worldUpVector).ToAngleAxis(out uangle, out uaxis);
			if (uangle >= 180f)
			{
				uangle = 360f - uangle;
				uaxis = -uaxis;
			}
			Vector3 newAngularVelocity = uaxis * uangle / softness;
			newAngularVelocity += faxis * fangle / softness;
			body.AddTorque(newAngularVelocity * weight * springStrength);
		}
	}
}
