using UnityEngine;

public class AngleLimitJoint : MonoBehaviour
{
	public Rigidbody body;

	public Rigidbody connectedBody;

	public Transform connectedTransform;

	public Vector3 localAnchor;

	public Vector3 localConnectedAnchor;

	public Vector3 localConnectedUpVector;

	public Vector3 localAimVector;

	public Vector3 localUpVector;

	public Vector3 localConnectedAimVector;

	public float springStrength = 1f;

	public float dampStrength = 1f;

	public float softness = 360f;

	private void Start()
	{
		body = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (!(body == null) && !(connectedBody == null))
		{
			Vector3 dir = body.transform.TransformDirection(localAimVector);
			Vector3 otherDir = connectedTransform.TransformDirection(localConnectedAimVector);
			float fangle = 0f;
			Vector3 faxis = Vector3.zero;
			Quaternion.FromToRotation(dir, otherDir).ToAngleAxis(out fangle, out faxis);
			if (fangle >= 180f)
			{
				fangle = 360f - fangle;
				faxis = -faxis;
			}
			float uangle = 0f;
			Vector3 uaxis = Vector3.zero;
			Quaternion.FromToRotation(Vector3.ProjectOnPlane(planeNormal: Vector3.ProjectOnPlane(body.transform.TransformDirection(localAimVector), connectedTransform.TransformDirection(localConnectedUpVector)).normalized, vector: body.transform.TransformDirection(localUpVector)).normalized, connectedTransform.TransformDirection(localConnectedUpVector)).ToAngleAxis(out uangle, out uaxis);
			if (uangle >= 180f)
			{
				uangle = 360f - uangle;
				uaxis = -uaxis;
			}
			Vector3 wantedAngVel = faxis * fangle / softness;
			wantedAngVel += uaxis * uangle / softness;
			Vector3 torque = wantedAngVel * springStrength;
			body.AddTorque(torque * body.mass);
		}
	}
}
