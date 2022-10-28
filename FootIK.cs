using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FootIK : MonoBehaviour
{
	private Animator targetAnimator;

	public Transform leftKneeHint;

	[SerializeField]
	public Transform rightKneeHint;

	private Transform leftFoot;

	private Transform rightFoot;

	private Transform hips;

	private void Start()
	{
		targetAnimator = GetComponent<Animator>();
		leftFoot = targetAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
		rightFoot = targetAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
		hips = targetAnimator.GetBoneTransform(HumanBodyBones.Hips);
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if (base.isActiveAndEnabled)
		{
			SetFootTarget(leftFoot, leftKneeHint, hips, targetAnimator, AvatarIKGoal.LeftFoot, AvatarIKHint.LeftKnee);
			SetFootTarget(rightFoot, rightKneeHint, hips, targetAnimator, AvatarIKGoal.RightFoot, AvatarIKHint.RightKnee);
		}
	}

	private void SetFootTarget(Transform foot, Transform hintT, Transform hip, Animator a, AvatarIKGoal target, AvatarIKHint hint)
	{
		float dist = Vector3.Distance(hip.position, foot.position);
		if (Physics.Raycast(hip.position, (foot.position - hip.position).normalized, out var hit, dist, GameManager.instance.walkableGroundMask, QueryTriggerInteraction.Ignore))
		{
			a.SetIKPositionWeight(target, 1f);
			a.SetIKRotationWeight(target, 1f);
			a.SetIKHintPositionWeight(hint, 1f);
			a.SetIKPosition(target, hit.point + hit.normal * 0.05f * base.transform.lossyScale.x);
			a.SetIKHintPosition(hint, hintT.position);
			a.SetIKRotation(target, Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.AngleAxis(-90f, foot.right) * foot.rotation);
		}
		else
		{
			a.SetIKHintPositionWeight(hint, 0f);
			a.SetIKPositionWeight(target, 0f);
			a.SetIKRotationWeight(target, 0f);
		}
	}
}
