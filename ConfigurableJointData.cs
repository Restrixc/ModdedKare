using UnityEngine;

public class ConfigurableJointData : IJointData
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

	public Vector3 secondaryAxis;

	public SoftJointLimitSpring angularXLimitSpring;

	public SoftJointLimitSpring angularYZLimitSpring;

	public SoftJointLimitSpring linearLimitSpring;

	public float massScale;

	public float connectedMassScale;

	public float breakForce;

	public float breakTorque;

	public SoftJointLimit lowAngularXLimit;

	public SoftJointLimit highAngularXLimit;

	public SoftJointLimit linearLimit;

	public SoftJointLimit angularYLimit;

	public SoftJointLimit angularZLimit;

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

	public Joint Apply(Rigidbody connectedBodyOverride = null)
	{
		return Apply(gameObject, connectedBodyOverride);
	}

	public ConfigurableJoint Apply(GameObject g, Rigidbody connectedBodyOverride = null)
	{
		ConfigurableJoint i = g.AddComponent<ConfigurableJoint>();
		i.rotationDriveMode = rotationDriveMode;
		i.projectionMode = projectionMode;
		i.projectionAngle = projectionAngle;
		i.projectionDistance = projectionDistance;
		i.angularXMotion = angularXMotion;
		i.angularYMotion = angularYMotion;
		i.angularZMotion = angularZMotion;
		i.angularXDrive = angularXDrive;
		i.angularYZDrive = angularYZDrive;
		i.xMotion = xMotion;
		i.yMotion = yMotion;
		i.zMotion = zMotion;
		i.xDrive = xDrive;
		i.yDrive = yDrive;
		i.zDrive = zDrive;
		i.autoConfigureConnectedAnchor = autoConfigureConnectedAnchor;
		i.anchor = anchor;
		i.connectedAnchor = connectedAnchor;
		i.configuredInWorldSpace = configuredInWorldSpace;
		i.axis = axis;
		i.secondaryAxis = secondaryAxis;
		i.angularXLimitSpring = angularXLimitSpring;
		i.angularYZLimitSpring = angularYZLimitSpring;
		i.linearLimitSpring = linearLimitSpring;
		i.massScale = massScale;
		i.connectedMassScale = connectedMassScale;
		i.breakForce = breakForce;
		i.breakTorque = breakTorque;
		i.lowAngularXLimit = lowAngularXLimit;
		i.highAngularXLimit = highAngularXLimit;
		i.linearLimit = linearLimit;
		i.angularYLimit = angularYLimit;
		i.angularZLimit = angularZLimit;
		i.swapBodies = swapBodies;
		i.enableCollision = enableCollision;
		i.enablePreprocessing = enablePreprocessing;
		i.slerpDrive = slerpDrive;
		i.targetRotation = targetRotation;
		i.connectedBody = ((connectedBodyOverride == null) ? connectedBody : connectedBodyOverride);
		return i;
	}

	public ConfigurableJointData(ConfigurableJoint j)
	{
		gameObject = j.gameObject;
		angularXMotion = j.angularXMotion;
		angularYMotion = j.angularYMotion;
		angularZMotion = j.angularZMotion;
		angularXDrive = j.angularXDrive;
		angularYZDrive = j.angularYZDrive;
		xMotion = j.xMotion;
		yMotion = j.yMotion;
		zMotion = j.zMotion;
		xDrive = j.xDrive;
		yDrive = j.yDrive;
		zDrive = j.zDrive;
		autoConfigureConnectedAnchor = j.autoConfigureConnectedAnchor;
		anchor = j.anchor;
		connectedAnchor = j.connectedAnchor;
		configuredInWorldSpace = j.configuredInWorldSpace;
		axis = j.axis;
		secondaryAxis = j.secondaryAxis;
		angularXLimitSpring = j.angularXLimitSpring;
		angularYZLimitSpring = j.angularYZLimitSpring;
		linearLimitSpring = j.linearLimitSpring;
		massScale = j.massScale;
		connectedMassScale = j.connectedMassScale;
		breakForce = j.breakForce;
		breakTorque = j.breakTorque;
		lowAngularXLimit = j.lowAngularXLimit;
		highAngularXLimit = j.highAngularXLimit;
		linearLimit = j.linearLimit;
		angularYLimit = j.angularYLimit;
		angularZLimit = j.angularZLimit;
		swapBodies = j.swapBodies;
		enableCollision = j.enableCollision;
		enablePreprocessing = j.enablePreprocessing;
		slerpDrive = j.slerpDrive;
		targetPosition = j.targetPosition;
		targetVelocity = j.targetVelocity;
		targetAngularVelocity = j.targetAngularVelocity;
		targetRotation = j.targetRotation;
		projectionMode = j.projectionMode;
		projectionAngle = j.projectionAngle;
		projectionDistance = j.projectionDistance;
		connectedBody = j.connectedBody;
		rotationDriveMode = j.rotationDriveMode;
	}
}
