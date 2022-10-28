using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vilar.IK;

public class PhysicsIK : MonoBehaviour, IKSolver
{
	public class CustomJoint
	{
		public Rigidbody body;

		public Vector3 anchor;

		public Vector3 targetVelocity;

		public Vector3 targetWorldPosition;

		public float strength;

		public float damping = 0.3f;

		public bool rotationEnabled;

		public Quaternion targetRotation = Quaternion.identity;

		public ConfigurableJoint joint;
	}

	[Serializable]
	public class LimbOrientation
	{
		public IKTargetSet.parts part;

		public Vector3 forward = Vector3.forward;

		public Vector3 up = Vector3.up;

		[HideInInspector]
		public Vector3 right = Vector3.right;
	}

	private static LimbOrientation defaultOrientation = new LimbOrientation();

	public List<LimbOrientation> orientations;

	public Animator animator;

	public Kobold kobold;

	private CustomJoint[] joints = new CustomJoint[10];

	private bool IKEnabled = false;

	[HideInInspector]
	public IKTargetSet targets { get; set; }

	public void AddJoint(int index, Transform body, Vector3 worldAnchor, float strength, bool rotationEnabled = true)
	{
		LimbOrientation lo = GetOrientation((IKTargetSet.parts)index);
		CustomJoint newJoint = new CustomJoint();
		newJoint.body = body.GetComponentInParent<Rigidbody>();
		newJoint.anchor = newJoint.body.transform.InverseTransformPoint(worldAnchor);
		newJoint.strength = strength;
		newJoint.rotationEnabled = rotationEnabled;
		if (rotationEnabled)
		{
			Quaternion savedRotation = newJoint.body.transform.rotation;
			newJoint.body.transform.rotation = Quaternion.identity;
			newJoint.joint = newJoint.body.gameObject.AddComponent<ConfigurableJoint>();
			newJoint.body.transform.rotation = savedRotation;
			JointDrive slerpd = newJoint.joint.slerpDrive;
			slerpd.positionSpring = strength * 10f;
			newJoint.joint.slerpDrive = slerpd;
			newJoint.joint.rotationDriveMode = RotationDriveMode.Slerp;
			newJoint.joint.configuredInWorldSpace = true;
		}
		joints[index] = newJoint;
	}

	public void ForceBlend(float value)
	{
	}

	public LimbOrientation GetOrientation(IKTargetSet.parts part)
	{
		foreach (LimbOrientation lo in orientations)
		{
			if (lo.part == part)
			{
				return lo;
			}
		}
		return defaultOrientation;
	}

	public void Initialize()
	{
		CleanUp();
		foreach (LimbOrientation lo in orientations)
		{
			Vector3.OrthoNormalize(ref lo.forward, ref lo.up, ref lo.right);
		}
		float strength = 5f;
		AddJoint(0, animator.GetBoneTransform(HumanBodyBones.Head), animator.GetBoneTransform(HumanBodyBones.Head).position, strength * 2f);
		AddJoint(2, animator.GetBoneTransform(HumanBodyBones.LeftHand), animator.GetBoneTransform(HumanBodyBones.LeftHand).position, strength * 0.5f);
		AddJoint(7, animator.GetBoneTransform(HumanBodyBones.LeftHand).parent.parent, animator.GetBoneTransform(HumanBodyBones.LeftHand).parent.position, strength / 2f, rotationEnabled: false);
		AddJoint(3, animator.GetBoneTransform(HumanBodyBones.RightHand), animator.GetBoneTransform(HumanBodyBones.RightHand).position, strength * 0.5f);
		AddJoint(6, animator.GetBoneTransform(HumanBodyBones.RightHand).parent.parent, animator.GetBoneTransform(HumanBodyBones.RightHand).parent.position, strength / 2f, rotationEnabled: false);
		AddJoint(4, animator.GetBoneTransform(HumanBodyBones.LeftFoot), animator.GetBoneTransform(HumanBodyBones.LeftFoot).position, strength * 0.5f);
		AddJoint(9, animator.GetBoneTransform(HumanBodyBones.LeftFoot).parent.parent, animator.GetBoneTransform(HumanBodyBones.LeftFoot).parent.position, strength / 2f, rotationEnabled: false);
		AddJoint(5, animator.GetBoneTransform(HumanBodyBones.RightFoot), animator.GetBoneTransform(HumanBodyBones.RightFoot).position, strength * 0.5f);
		AddJoint(8, animator.GetBoneTransform(HumanBodyBones.RightFoot).parent.parent, animator.GetBoneTransform(HumanBodyBones.RightFoot).parent.position, strength / 2f, rotationEnabled: false);
		AddJoint(1, animator.GetBoneTransform(HumanBodyBones.Hips), animator.GetBoneTransform(HumanBodyBones.Hips).position, strength);
		kobold.ragdoller.PushRagdoll();
		IKEnabled = true;
	}

	public void CleanUp()
	{
		CustomJoint[] array = joints;
		foreach (CustomJoint i in array)
		{
			if (i != null && i.joint != null)
			{
				UnityEngine.Object.Destroy(i.joint);
			}
		}
		IKEnabled = false;
		kobold.ragdoller.PopRagdoll();
	}

	public void SetTarget(int index, Vector3 position, Quaternion rotation)
	{
		if (joints[index] != null)
		{
			joints[index].targetWorldPosition = position;
			joints[index].targetRotation = rotation;
		}
	}

	public void FixedUpdate()
	{
		if (!IKEnabled)
		{
			return;
		}
		for (int i = 0; i < 10; i++)
		{
			Vector3 bodyPos = joints[i].body.transform.TransformPoint(joints[i].anchor);
			Vector3 targetPos = joints[i].targetWorldPosition;
			Vector3 linearForce = (targetPos - bodyPos) * joints[i].strength;
			LimbOrientation lo = GetOrientation((IKTargetSet.parts)i);
			joints[i].body.velocity *= 1f - joints[i].damping;
			float wrongDirForce = Mathf.Clamp01(0f - Vector3.Dot(joints[i].body.velocity.normalized, linearForce.normalized));
			joints[i].body.AddForce(linearForce - joints[i].body.velocity * wrongDirForce, ForceMode.VelocityChange);
			joints[i].body.velocity = Vector3.Lerp(joints[i].body.velocity, joints[i].targetVelocity, 0.9f);
			if (joints[i].rotationEnabled)
			{
				Quaternion adjust = Quaternion.LookRotation(lo.forward, lo.up);
				joints[i].joint.targetRotation = Quaternion.Inverse(joints[i].targetRotation * Quaternion.Inverse(adjust));
			}
		}
	}

	public void Solve()
	{
	}
}
