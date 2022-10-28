using UnityEngine;

namespace Vilar.IK;

public class ClassicIK : MonoBehaviour, IKSolver
{
	[SerializeField]
	private AnimationClip tpose;

	[SerializeField]
	private AnimationCurve antiPop;

	public float blendTarget;

	private float blend;

	private Transform hip;

	private Transform spine;

	private Transform chest;

	private Transform neck;

	private Transform head;

	private Transform leftShoulder;

	private Transform rightShoulder;

	private Animator animator;

	private float neckLength;

	private float chestLength;

	private float spineLength;

	private float hipLength;

	private float torsoLength;

	private float armLength;

	private Vector3 virtualHead;

	private Vector3 virtualHeadLook;

	private Vector3 virtualNeck;

	private Vector3 virtualNeckLook;

	private Vector3 virtualChest;

	private Vector3 virtualChestLook;

	private Vector3 virtualSpine;

	private Vector3 virtualSpineLook;

	private Vector3 virtualHip;

	private Vector3 virtualHipLook;

	private Vector3 virtualMid;

	private Vector3 elbowHint;

	[HideInInspector]
	public IKTargetSet targets { get; set; }

	private void Awake()
	{
	}

	private void OnDisable()
	{
		CleanUp();
	}

	public void Update()
	{
		blend = Mathf.MoveTowards(blend, blendTarget, Time.deltaTime * 2f);
		if (!Application.isPlaying)
		{
		}
	}

	public void LateUpdate()
	{
		if (Application.isPlaying && base.isActiveAndEnabled)
		{
			Solve();
		}
	}

	public void Solve()
	{
		SolveSpine();
		SolveLimb(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), animator.GetBoneTransform(HumanBodyBones.LeftHand), targets.GetLocalPosition(IKTargetSet.parts.HANDLEFT), targets.GetLocalRotation(IKTargetSet.parts.HANDLEFT), targets.GetLocalPosition(IKTargetSet.parts.ELBOWLEFT), noPop: false, kneeCorrection: false);
		SolveLimb(animator.GetBoneTransform(HumanBodyBones.RightUpperArm), animator.GetBoneTransform(HumanBodyBones.RightLowerArm), animator.GetBoneTransform(HumanBodyBones.RightHand), targets.GetLocalPosition(IKTargetSet.parts.HANDRIGHT), targets.GetLocalRotation(IKTargetSet.parts.HANDRIGHT), targets.GetLocalPosition(IKTargetSet.parts.ELBOWRIGHT), noPop: false, kneeCorrection: false);
		correctShoulder(animator.GetBoneTransform(HumanBodyBones.LeftShoulder), animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
		correctShoulder(animator.GetBoneTransform(HumanBodyBones.RightShoulder), animator.GetBoneTransform(HumanBodyBones.RightUpperArm), animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
		SolveLimb(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), animator.GetBoneTransform(HumanBodyBones.LeftHand), targets.GetLocalPosition(IKTargetSet.parts.HANDLEFT), targets.GetLocalRotation(IKTargetSet.parts.HANDLEFT), targets.GetLocalPosition(IKTargetSet.parts.ELBOWLEFT), noPop: true, kneeCorrection: false);
		SolveLimb(animator.GetBoneTransform(HumanBodyBones.RightUpperArm), animator.GetBoneTransform(HumanBodyBones.RightLowerArm), animator.GetBoneTransform(HumanBodyBones.RightHand), targets.GetLocalPosition(IKTargetSet.parts.HANDRIGHT), targets.GetLocalRotation(IKTargetSet.parts.HANDRIGHT), targets.GetLocalPosition(IKTargetSet.parts.ELBOWRIGHT), noPop: true, kneeCorrection: false);
		SolveLimb(animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg), animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), animator.GetBoneTransform(HumanBodyBones.LeftFoot), targets.GetLocalPosition(IKTargetSet.parts.FOOTLEFT), targets.GetLocalRotation(IKTargetSet.parts.FOOTLEFT), targets.GetLocalPosition(IKTargetSet.parts.KNEELEFT), noPop: true, kneeCorrection: true);
		SolveLimb(animator.GetBoneTransform(HumanBodyBones.RightUpperLeg), animator.GetBoneTransform(HumanBodyBones.RightLowerLeg), animator.GetBoneTransform(HumanBodyBones.RightFoot), targets.GetLocalPosition(IKTargetSet.parts.FOOTRIGHT), targets.GetLocalRotation(IKTargetSet.parts.FOOTRIGHT), targets.GetLocalPosition(IKTargetSet.parts.KNEERIGHT), noPop: true, kneeCorrection: true);
	}

	private void TPoseForAFrame()
	{
		if (animator == null)
		{
			animator = GetComponentInChildren<Animator>();
		}
		Vector3 position = animator.transform.localPosition;
		Quaternion rotation = animator.transform.localRotation;
		tpose.SampleAnimation(animator.gameObject, 0f);
		animator.transform.localPosition = position;
		animator.transform.localRotation = rotation;
	}

	public void Initialize()
	{
		animator = GetComponentInChildren<Animator>();
		TPoseForAFrame();
		hip = animator.GetBoneTransform(HumanBodyBones.Hips);
		spine = animator.GetBoneTransform(HumanBodyBones.Spine);
		chest = animator.GetBoneTransform(HumanBodyBones.Chest);
		neck = animator.GetBoneTransform(HumanBodyBones.Neck);
		head = animator.GetBoneTransform(HumanBodyBones.Head);
		Vector3 cachedPosition = base.transform.position;
		Quaternion cachedRotation = base.transform.rotation;
		base.transform.position = Vector3.zero;
		base.transform.rotation = Quaternion.identity;
		targets = new IKTargetSet(animator);
		leftShoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
		rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
		recalculateSpine();
		base.transform.position = cachedPosition;
		base.transform.rotation = cachedRotation;
	}

	public void CleanUp()
	{
	}

	public void SetTarget(int index, Vector3 position, Quaternion rotation)
	{
		targets.SetTarget(index, base.transform.InverseTransformPoint(position), Quaternion.Inverse(base.transform.rotation) * rotation);
	}

	private void recalculateSpine()
	{
		hipLength = Vector3.Distance(spine.position, hip.position) / hip.lossyScale.y;
		spineLength = Vector3.Distance(chest.position, spine.position) / spine.lossyScale.y;
		chestLength = Vector3.Distance(neck.position, chest.position) / chest.lossyScale.y;
		neckLength = Vector3.Distance(head.position, neck.position) / neck.lossyScale.y;
	}

	private void SolveSpine()
	{
		if (targets == null)
		{
			Initialize();
		}
		float soft = 0.2f;
		hip.position = Vector3.Lerp(hip.position, base.transform.TransformPoint(targets.GetLocalPosition(IKTargetSet.parts.HIPS)), blend);
		virtualHip = targets.GetLocalPosition(IKTargetSet.parts.HIPS);
		virtualHead = targets.GetLocalPosition(IKTargetSet.parts.HEAD);
		virtualNeck = virtualHead + targets.GetLocalRotation(IKTargetSet.parts.HEAD) * -Vector3.up * neckLength;
		virtualChest = Vector3.Lerp(virtualHead + targets.GetLocalRotation(IKTargetSet.parts.HEAD) * -Vector3.up * (neckLength + chestLength), virtualHip + targets.GetLocalRotation(IKTargetSet.parts.HIPS) * Vector3.up * (hipLength + spineLength), 0.5f);
		virtualSpine = virtualHip + targets.GetLocalRotation(IKTargetSet.parts.HIPS) * Vector3.up * hipLength;
		for (int i = 0; i < 5; i++)
		{
			virtualNeck = Vector3.Lerp(virtualNeck, (virtualHead + virtualChest) / 2f, soft);
			virtualChest = Vector3.Lerp(virtualChest, (virtualNeck + virtualSpine) / 2f, soft);
			virtualHead = Vector3.Lerp(virtualHead, targets.GetLocalPosition(IKTargetSet.parts.HEAD), soft);
			virtualNeck = Vector3.Lerp(virtualNeck, virtualHead + (virtualNeck - virtualHead).normalized * neckLength, soft * 1.5f);
			virtualChest = Vector3.Lerp(virtualChest, virtualNeck + (virtualChest - virtualNeck).normalized * chestLength, soft);
			virtualChest = Vector3.Lerp(virtualChest, virtualSpine + (virtualChest - virtualSpine).normalized * spineLength, soft);
			virtualNeck = Vector3.Lerp(virtualNeck, virtualChest + (virtualNeck - virtualChest).normalized * chestLength, soft * 1.5f);
			virtualHead = Vector3.Lerp(virtualHead, virtualNeck + (virtualHead - virtualNeck).normalized * neckLength, soft);
		}
		virtualHeadLook = targets.GetLocalRotation(IKTargetSet.parts.HEAD) * Vector3.forward;
		virtualHipLook = targets.GetLocalRotation(IKTargetSet.parts.HIPS) * Vector3.forward;
		Vector3 virtualHeadLookDown = targets.GetLocalRotation(IKTargetSet.parts.HEAD) * Vector3.down;
		Vector3 virtualHipLookDown = targets.GetLocalRotation(IKTargetSet.parts.HIPS) * Vector3.down;
		virtualNeckLook = Vector3.Lerp(virtualHeadLook, virtualHipLook, 0.3f);
		float neckDot = Vector3.Dot((virtualHead - virtualNeck).normalized, virtualNeckLook);
		virtualNeckLook = Vector3.Lerp(virtualNeckLook, Vector3.Lerp(virtualHeadLookDown, virtualHipLookDown, 0.3f), Mathf.Clamp01(neckDot));
		virtualChestLook = Vector3.Lerp(virtualHeadLook, virtualHipLook, 0.5f);
		float chestDot = Vector3.Dot((virtualNeck - virtualChest).normalized, virtualChestLook);
		virtualChestLook = Vector3.Lerp(virtualChestLook, Vector3.Lerp(virtualHeadLookDown, virtualHipLookDown, 0.5f), Mathf.Clamp01(chestDot));
		virtualSpineLook = Vector3.Lerp(virtualHeadLook, virtualHipLook, 0.75f);
		float spineDot = Vector3.Dot((virtualChest - virtualSpine).normalized, virtualSpineLook);
		virtualSpineLook = Vector3.Lerp(virtualSpineLook, Vector3.Lerp(virtualHeadLookDown, virtualHipLookDown, 0.75f), Mathf.Clamp01(spineDot));
		hip.rotation = Quaternion.Slerp(hip.rotation, base.transform.rotation * targets.GetLocalRotation(IKTargetSet.parts.HIPS), blend);
		spine.rotation = Quaternion.Slerp(spine.rotation, Quaternion.FromToRotation(chest.position - spine.position, base.transform.TransformPoint(virtualChest) - base.transform.TransformPoint(virtualSpine)) * spine.rotation, blend);
		chest.rotation = Quaternion.Slerp(chest.rotation, Quaternion.FromToRotation(neck.position - chest.position, base.transform.TransformPoint(virtualNeck) - base.transform.TransformPoint(virtualChest)) * chest.rotation, blend);
		neck.rotation = Quaternion.Slerp(neck.rotation, Quaternion.FromToRotation(head.position - neck.position, base.transform.TransformPoint(virtualHead) - base.transform.TransformPoint(virtualNeck)) * neck.rotation, blend);
		SpineTwist();
		head.rotation = Quaternion.Slerp(head.rotation, base.transform.rotation * targets.GetLocalRotation(IKTargetSet.parts.HEAD), blend);
	}

	private void SpineTwist()
	{
		spine.rotation = Quaternion.LookRotation(spine.up, base.transform.rotation * virtualSpineLook) * Quaternion.Euler(-90f, 180f, 0f);
		chest.rotation = Quaternion.LookRotation(chest.up, base.transform.rotation * virtualChestLook) * Quaternion.Euler(-90f, 180f, 0f);
		neck.rotation = Quaternion.LookRotation(neck.up, base.transform.rotation * virtualNeckLook) * Quaternion.Euler(-90f, 180f, 0f);
	}

	private void SolveLimb(Transform upper, Transform lower, Transform end, Vector3 position, Quaternion rotation, Vector3 hint, bool noPop, bool kneeCorrection)
	{
		Vector3 targetPosition = base.transform.TransformPoint(position);
		float upperLength = Vector3.Distance(upper.position, lower.position);
		float lowerLength = Vector3.Distance(lower.position, end.position);
		if (noPop)
		{
			Vector3 targetOffset = targetPosition - upper.localPosition;
			targetPosition = upper.localPosition + targetOffset.normalized * antiPop.Evaluate(targetOffset.magnitude / (upperLength + lowerLength)) * targetOffset.magnitude;
		}
		virtualMid = base.transform.TransformPoint(hint);
		for (int i = 0; i < 10; i++)
		{
			virtualMid = Vector3.Lerp(virtualMid, targetPosition + (virtualMid - targetPosition).normalized * lowerLength, 0.6f);
			virtualMid = Vector3.Lerp(virtualMid, upper.position + (virtualMid - upper.position).normalized * upperLength, 0.6f);
		}
		upper.rotation = Quaternion.Slerp(upper.rotation, Quaternion.FromToRotation(lower.position - upper.position, virtualMid - upper.position) * upper.rotation, blend);
		if (kneeCorrection)
		{
			upper.rotation = Quaternion.FromToRotation(-upper.forward, Vector3.ProjectOnPlane(virtualMid - targetPosition, upper.up).normalized) * upper.rotation;
		}
		lower.rotation = Quaternion.Slerp(lower.rotation, Quaternion.FromToRotation(end.position - lower.position, targetPosition - virtualMid) * lower.rotation, blend);
		end.rotation = Quaternion.Slerp(end.rotation, base.transform.rotation * rotation, blend);
	}

	private Vector3 estimateChestForward()
	{
		Vector3 chestUp = Vector3.Normalize(virtualChest - virtualSpine);
		Vector3 restChestForward = Vector3.Lerp(targets.GetLocalRotation(IKTargetSet.parts.HIPS) * Vector3.forward, targets.GetLocalRotation(IKTargetSet.parts.HEAD) * Vector3.forward, 0.5f);
		Vector3 leftHandOffset = Vector3.ProjectOnPlane(targets.GetLocalPosition(IKTargetSet.parts.HANDLEFT) - virtualSpine, chestUp);
		Vector3 rightHandOffset = Vector3.ProjectOnPlane(targets.GetLocalPosition(IKTargetSet.parts.HANDRIGHT) - virtualSpine, chestUp);
		Vector3 leftHandChestBias = -Vector3.Cross(leftHandOffset.normalized, chestUp);
		Vector3 rightHandChestBias = Vector3.Cross(rightHandOffset.normalized, chestUp);
		Vector3 computedChestForward = Vector3.Lerp(leftHandChestBias, rightHandChestBias, rightHandOffset.sqrMagnitude / (leftHandOffset.sqrMagnitude + rightHandOffset.sqrMagnitude));
		return Vector3.Lerp(restChestForward, computedChestForward, Mathf.Clamp01(Mathf.Pow(Mathf.Max(leftHandOffset.magnitude, rightHandOffset.magnitude) / armLength, 2f)));
	}

	private void correctShoulder(Transform shoulder, Transform upperArm, Transform lowerArm)
	{
		Vector3 relaxVector = (shoulder.position - animator.GetBoneTransform(HumanBodyBones.Neck).position).normalized;
		Vector3 upperArmVector = (lowerArm.position - upperArm.position).normalized;
		Quaternion shoulderCorrection = Quaternion.FromToRotation(upperArm.position - shoulder.position, lowerArm.position - shoulder.position);
		shoulderCorrection = Quaternion.Lerp(shoulderCorrection, Quaternion.identity, Mathf.Clamp01(Vector3.Dot(upperArmVector, relaxVector)));
		shoulder.rotation = shoulderCorrection * shoulder.rotation;
		upperArm.rotation = Quaternion.Inverse(shoulderCorrection) * upperArm.rotation;
	}

	public void ForceBlend(float value)
	{
		blendTarget = value;
		blend = value;
	}
}
