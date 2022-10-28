using System;
using UnityEngine;

namespace Vilar.IK;

[Serializable]
public class IKTargetSet
{
	public enum parts
	{
		HEAD,
		HIPS,
		HANDLEFT,
		HANDRIGHT,
		FOOTLEFT,
		FOOTRIGHT,
		ELBOWRIGHT,
		ELBOWLEFT,
		KNEERIGHT,
		KNEELEFT
	}

	public Matrix4x4[] targets;

	public Matrix4x4[] anchors;

	public IKTargetSet(Animator animator)
	{
		targets = new Matrix4x4[Enum.GetValues(typeof(parts)).Length];
		anchors = new Matrix4x4[Enum.GetValues(typeof(parts)).Length];
		Matrix4x4 localRoot = CreateMatrixFromLocalTransform(animator.transform, Matrix4x4.identity);
		targets[0] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.Head), localRoot, Quaternion.identity);
		targets[1] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.Hips), localRoot, Quaternion.identity);
		targets[2] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftHand), localRoot, Quaternion.Euler(0f, -90f, 0f));
		targets[3] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightHand), localRoot, Quaternion.Euler(0f, 90f, 0f));
		targets[4] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftFoot), localRoot, Quaternion.identity);
		targets[5] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightFoot), localRoot, Quaternion.identity);
		targets[7] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), localRoot, Quaternion.Euler(0f, -90f, 0f));
		targets[6] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), localRoot, Quaternion.Euler(0f, 90f, 0f));
		targets[9] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), localRoot, Quaternion.identity);
		targets[8] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg), localRoot, Quaternion.identity);
		anchors[0] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.Head), localRoot * targets[0]);
		anchors[1] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.Hips), localRoot * targets[1]);
		anchors[2] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftHand), localRoot * targets[2]);
		anchors[3] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightHand), localRoot * targets[3]);
		anchors[4] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftFoot), localRoot * targets[4]);
		anchors[5] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightFoot), localRoot * targets[5]);
		anchors[7] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), localRoot * targets[7]);
		anchors[6] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), localRoot * targets[6]);
		anchors[9] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), localRoot * targets[9]);
		anchors[8] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg), localRoot * targets[8]);
	}

	public void SetTarget(int index, Vector3 position, Quaternion rotation)
	{
		targets[index] = Matrix4x4.TRS(position, rotation.normalized, Vector3.one);
	}

	public Vector3 GetLocalPosition(parts part)
	{
		return GetLocalPosition((int)part);
	}

	public Vector3 GetLocalPosition(int part)
	{
		return targets[part] * anchors[part] * new Vector4(0f, 0f, 0f, 1f);
	}

	public Quaternion GetLocalRotation(parts part)
	{
		return GetLocalRotation((int)part);
	}

	public Quaternion GetLocalRotation(int part)
	{
		return (targets[part] * anchors[part]).rotation;
	}

	private Matrix4x4 CreateMatrixFromLocalTransform(Transform t, Matrix4x4 parent)
	{
		return parent.inverse * Matrix4x4.TRS(t.position, t.rotation, Vector3.one);
	}

	private Matrix4x4 CreateMatrixFromLocalTransform(Transform t, Matrix4x4 parent, Quaternion rotationOverride)
	{
		return parent.inverse * Matrix4x4.TRS(t.position, rotationOverride, Vector3.one);
	}
}
