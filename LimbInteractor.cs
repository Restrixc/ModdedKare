using System.Runtime.CompilerServices;
using UnityEngine;

public class LimbInteractor : MonoBehaviour, IAdvancedInteractable
{
	public enum Hand
	{
		Left,
		Right
	}

	private HandIK handHandler;

	public Hand handTarget;

	public Vector3 handForward = Vector3.up;

	public Vector3 handUp = Vector3.forward;

	private Vector3 handRight = Vector3.right;

	private RopeJointConstraint joint;

	private bool ragdolled;

	private Kobold kobold;

	private Rigidbody koboldLimb;

	public Transform rootedTransform;

	public float _maxDistance = 1f;

	public float _springStrength = 400f;

	[Range(0f, 1f)]
	public float _dampStrength = 0.1f;

	private ConfigurableJoint otherJoint;

	private Quaternion originalRot;

	private bool grabbed = false;

	public ConfigurableJoint AddJoint(Rigidbody hitBody, Vector3 worldAnchor)
	{
		ConfigurableJoint joint = hitBody.gameObject.AddComponent<ConfigurableJoint>();
		joint.autoConfigureConnectedAnchor = false;
		JointDrive drive = joint.xDrive;
		drive.positionSpring = _springStrength;
		drive.positionDamper = 2f;
		joint.xDrive = drive;
		joint.yDrive = drive;
		joint.zDrive = drive;
		joint.enablePreprocessing = false;
		joint.configuredInWorldSpace = true;
		joint.anchor = hitBody.transform.InverseTransformPoint(worldAnchor);
		joint.connectedAnchor = worldAnchor;
		return joint;
	}

	public void OnInteract(Kobold k)
	{
		if (!(joint != null) && !(otherJoint != null))
		{
			grabbed = true;
			joint = base.gameObject.AddComponent<RopeJointConstraint>();
			joint.anchor = base.transform.position;
			joint.connectedBody = kobold.body;
			joint.connectedAnchor = kobold.body.transform.InverseTransformPoint(rootedTransform.position);
			joint.springStrength = _springStrength;
			joint.dampStrength = _dampStrength;
			joint.maxDistance = _maxDistance;
			Vector3.OrthoNormalize(ref handForward, ref handUp, ref handRight);
			originalRot = Quaternion.AngleAxis(90f, handRight) * Quaternion.Inverse(Quaternion.LookRotation(handUp, handForward));
		}
	}

	public void Start()
	{
		kobold = GetComponentInParent<Kobold>();
		handHandler = GetComponentInParent<HandIK>();
		koboldLimb = GetComponentInParent<Rigidbody>();
		kobold.ragdoller.RagdollEvent += RagdollEvent;
	}

	public void OnDestroy()
	{
		if (kobold != null)
		{
			kobold.ragdoller.RagdollEvent -= RagdollEvent;
		}
	}

	public void OnEndInteract()
	{
		grabbed = false;
		if (otherJoint != null)
		{
			Object.Destroy(otherJoint);
			otherJoint = null;
		}
		if ((bool)joint)
		{
			Object.Destroy(joint);
			joint = null;
		}
		handHandler.UnsetIKTarget((int)handTarget);
	}

	public void InteractTo(Vector3 worldPosition, Quaternion worldRotation)
	{
		if (joint != null)
		{
			joint.anchor = worldPosition;
			joint.connectedAnchor = kobold.body.transform.InverseTransformPoint(rootedTransform.position);
			handHandler.SetIKTarget((int)handTarget, worldPosition, worldRotation * originalRot);
		}
		else
		{
			handHandler.UnsetIKTarget((int)handTarget);
		}
	}

	public bool ShowHand()
	{
		return true;
	}

	public bool PhysicsGrabbable()
	{
		return ragdolled;
	}

	public void RagdollEvent(bool ragdolled)
	{
		this.ragdolled = ragdolled;
		if (ragdolled)
		{
			if (joint != null)
			{
				joint.enabled = false;
			}
			if (koboldLimb != null && grabbed)
			{
				if (otherJoint != null)
				{
					Object.Destroy(otherJoint);
				}
				otherJoint = AddJoint(koboldLimb, base.transform.position);
			}
		}
		else
		{
			if (otherJoint != null)
			{
				Object.Destroy(otherJoint);
			}
			if (joint != null)
			{
				joint.enabled = true;
			}
		}
	}

	
}
