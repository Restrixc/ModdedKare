using UnityEngine;

public class CharacterJointData : IJointData
{
	public GameObject gameObject;

	public ConfigurableJointMotion angularXMotion;

	public ConfigurableJointMotion angularYMotion;

	public ConfigurableJointMotion angularZMotion;

	public JointDrive angularXDrive;

	public JointDrive angularYZDrive;

	public ConfigurableJointMotion xMotion;

	public ConfigurableJointMotion yMotion;

	public ConfigurableJointMotion zMotion;

	public JointDrive xDrive;

	public JointDrive yDrive;

	public JointDrive zDrive;

	public Rigidbody connectedBody;

	public bool autoConfigureConnectedAnchor;

	public Vector3 anchor;

	public Vector3 connectedAnchor;

	public bool configuredInWorldSpace;

	public Vector3 axis;

	public Vector3 swingAxis;

	public SoftJointLimitSpring swingLimitSpring;

	public SoftJointLimitSpring twistLimitSpring;

	public float massScale;

	public float connectedMassScale;

	public float breakForce;

	public float breakTorque;

	public SoftJointLimit swing1Limit;

	public SoftJointLimit swing2Limit;

	public SoftJointLimit highTwistLimit;

	public SoftJointLimit lowTwistLimit;

	public bool swapBodies;

	public bool enableCollision;

	public bool enablePreprocessing;

	public JointDrive slerpDrive;

	public Vector3 targetPosition;

	public Vector3 targetVelocity;

	public Vector3 targetAngularVelocity;

	public Quaternion targetRotation;

	public JointProjectionMode projectionMode;

	public float projectionAngle;

	public float projectionDistance;

	public RotationDriveMode rotationDriveMode;

	public bool enableProjection;

	public Joint Apply(Rigidbody connectedBodyOverride = null)
	{
		return Apply(gameObject, connectedBodyOverride);
	}

	public CharacterJoint Apply(GameObject g, Rigidbody connectedBodyOverride = null)
	{
		CharacterJoint i = g.AddComponent<CharacterJoint>();
		i.connectedBody = ((connectedBodyOverride == null) ? connectedBody : connectedBodyOverride);
		i.projectionAngle = projectionAngle;
		i.projectionDistance = projectionDistance;
		i.autoConfigureConnectedAnchor = autoConfigureConnectedAnchor;
		i.anchor = anchor;
		i.connectedAnchor = connectedAnchor;
		i.axis = axis;
		i.swingAxis = swingAxis;
		i.twistLimitSpring = twistLimitSpring;
		i.massScale = massScale;
		i.connectedMassScale = connectedMassScale;
		i.breakForce = breakForce;
		i.breakTorque = breakTorque;
		i.enableCollision = enableCollision;
		i.enablePreprocessing = enablePreprocessing;
		i.swing1Limit = swing1Limit;
		i.swing2Limit = swing2Limit;
		i.swingLimitSpring = swingLimitSpring;
		i.highTwistLimit = highTwistLimit;
		i.lowTwistLimit = lowTwistLimit;
		i.enableProjection = enableProjection;
		return i;
	}

	public CharacterJointData(GameObject target)
	{
		gameObject = target;
		autoConfigureConnectedAnchor = true;
		anchor = Vector3.zero;
		axis = Vector3.right;
		swingAxis = Vector3.back;
		swing1Limit = new SoftJointLimit
		{
			limit = 33f,
			bounciness = 0.1f,
			contactDistance = 0f
		};
		swing2Limit = new SoftJointLimit
		{
			limit = 9f,
			bounciness = 0.1f,
			contactDistance = 0f
		};
		SoftJointLimitSpring limitSpring = new SoftJointLimitSpring
		{
			spring = 1f,
			damper = 0.1f
		};
		swingLimitSpring = limitSpring;
		twistLimitSpring = limitSpring;
		highTwistLimit = new SoftJointLimit
		{
			limit = 30f,
			bounciness = 0.1f
		};
		lowTwistLimit = new SoftJointLimit
		{
			limit = -40f,
			bounciness = 0.1f
		};
		massScale = 10f;
		connectedMassScale = 1f;
		breakForce = float.PositiveInfinity;
		breakTorque = float.PositiveInfinity;
		enableCollision = false;
		enablePreprocessing = true;
		projectionAngle = 5f;
		projectionDistance = 0.1f;
		connectedBody = null;
		enableProjection = true;
	}

	public CharacterJointData(CharacterJoint j)
	{
		gameObject = j.gameObject;
		autoConfigureConnectedAnchor = j.autoConfigureConnectedAnchor;
		anchor = j.anchor;
		connectedAnchor = j.connectedAnchor;
		axis = j.axis;
		swingAxis = j.swingAxis;
		swing1Limit = j.swing1Limit;
		swing2Limit = j.swing2Limit;
		swingLimitSpring = j.swingLimitSpring;
		twistLimitSpring = j.twistLimitSpring;
		highTwistLimit = j.highTwistLimit;
		lowTwistLimit = j.lowTwistLimit;
		massScale = j.massScale;
		connectedMassScale = j.connectedMassScale;
		breakForce = j.breakForce;
		breakTorque = j.breakTorque;
		enableCollision = j.enableCollision;
		enablePreprocessing = j.enablePreprocessing;
		projectionAngle = j.projectionAngle;
		projectionDistance = j.projectionDistance;
		connectedBody = j.connectedBody;
		enableProjection = j.enableProjection;
	}
}
