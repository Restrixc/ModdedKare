using System;
using UnityEngine;

namespace Vilar.AnimationStation;

[Serializable]
public class AnimationLooper
{
	public enum TargetType
	{
		NONE,
		HEAD,
		HIP,
		HANDLEFT,
		HANDRIGHT,
		FOOTLEFT,
		FOOTRIGHT,
		ELBOWLEFT,
		ELBOWRIGHT,
		KNEELEFT,
		KNEERIGHT
	}

	public const int TARGET_COUNT = 10;

	public float speed;

	public TargetType[] attachments = new TargetType[10];

	public AnimationStation[] attachmentTargets = new AnimationStation[10];

	public float[] motionScale = new float[10];

	public float[] motionOffset = new float[10];

	public AnimationMotion[] motion = new AnimationMotion[10];

	public Vector3[] targetPositions = new Vector3[10];

	public Quaternion[] targetRotations = new Quaternion[10];

	public Vector3[] computedTargetPositions = new Vector3[10];

	public Vector3[] computedTargetVelocities = new Vector3[10];

	public Quaternion[] computedTargetRotations = new Quaternion[10];

	public static string GetTypeName(int index)
	{
		return Enum.GetNames(typeof(TargetType))[index];
	}

	public AnimationLooper(AnimationLooper copy)
	{
		speed = copy.speed;
		for (int i = 0; i < 10; i++)
		{
			attachments[i] = copy.attachments[i];
			targetPositions[i] = copy.targetPositions[i];
			targetRotations[i] = copy.targetRotations[i];
			computedTargetPositions[i] = copy.computedTargetPositions[i];
			computedTargetRotations[i] = copy.computedTargetRotations[i];
			computedTargetPositions[i] = Vector3.zero;
			computedTargetRotations[i] = Quaternion.identity;
			motion[i] = copy.motion[i];
			motionScale[i] = copy.motionScale[i];
			motionOffset[i] = copy.motionOffset[i];
		}
	}

	public AnimationLooper()
	{
		speed = 1f;
		for (int i = 0; i < 10; i++)
		{
			attachments[i] = TargetType.NONE;
			targetPositions[i] = Vector3.zero;
			targetRotations[i] = Quaternion.identity;
			computedTargetPositions[i] = Vector3.zero;
			computedTargetRotations[i] = Quaternion.identity;
			motionScale[i] = 1f;
			motionOffset[i] = 0f;
		}
	}
}
