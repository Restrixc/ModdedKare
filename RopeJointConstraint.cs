using UnityEngine;

public class RopeJointConstraint : MonoBehaviour
{
	public Rigidbody connectedBody;

	public Vector3 connectedAnchor;

	public Vector3 anchor;

	public float springStrength;

	public float dampStrength;

	public float maxDistance;

	private void FixedUpdate()
	{
		Vector3 connectedBodyPos;
		Vector3 cbodyVel;
		if (connectedBody != null)
		{
			connectedBodyPos = connectedBody.transform.TransformPoint(connectedAnchor);
			cbodyVel = connectedBody.velocity;
		}
		else
		{
			connectedBodyPos = connectedAnchor;
			cbodyVel = Vector3.zero;
		}
		Vector3 pos = anchor;
		Vector3 bodyVel = Vector3.zero;
		Vector3 normDir = Vector3.Normalize(connectedBodyPos - pos);
		float dist = Vector3.Distance(pos, connectedBodyPos);
		float power = Mathf.Max(dist - maxDistance, 0f);
		Vector3 dampingForce = (cbodyVel - bodyVel) * dampStrength;
		connectedBody.AddForceAtPosition(-normDir * springStrength * power, connectedBodyPos);
	}
}
