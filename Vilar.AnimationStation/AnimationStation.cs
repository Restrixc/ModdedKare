using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Vilar.IK;

namespace Vilar.AnimationStation;

[ExecuteAlways]
[SelectionBase]
public class AnimationStation : MonoBehaviourPun
{
	[HideInInspector]
	public float progress;

	[HideInInspector]
	public int selectedLoop = 0;

	[HideInInspector]
	public List<AnimationLooper> loops;

	[HideInInspector]
	public int editSelection = 0;

	[HideInInspector]
	public float animProgress = 0f;

	private Vector3 lastScale;

	private float modifiedProgress;

	private Vector3 lookAtPosition;

	public GameObject previewCharacter;

	private GameObject previewCharacterInstance;

	private IKSolver previewCharacterIKSolver;

	[HideInInspector]
	public AnimationStationHashSet linkedStations = new AnimationStationHashSet();

	private Dictionary<HumanBodyBones, Quaternion> restPoseCache;

	public AnimationStationInfo info;

	private Vector2 hipOffset;

	public bool isPreviewAssigned => previewCharacter != null;

	public static string GetTypeName(int index)
	{
		return Enum.GetNames(typeof(AnimationLooper.TargetType))[index];
	}

	public void SetHipOffset(Vector2 hipOffset)
	{
		this.hipOffset = hipOffset;
	}

	private void Update()
	{
		if (info.user != null)
		{
			Advance(Time.deltaTime);
		}
	}

	public void SetProgress(float newProgress)
	{
		foreach (AnimationStation linkedStation in linkedStations.hashSet)
		{
			linkedStation.progress = newProgress;
		}
	}

	public void UpdatePreview(float dT)
	{
		TryInstantiatePreview();
		Advance(dT);
		SetPreview();
	}

	private void Awake()
	{
		info = new AnimationStationInfo();
	}

	public void OnStartAnimation(Kobold user)
	{
		info.user = user;
		foreach (AnimationStation linkedStation in linkedStations.hashSet)
		{
			linkedStation.animProgress = 0f;
		}
	}

	private void TryInstantiatePreview()
	{
		if (!(previewCharacterInstance == null) || !(previewCharacter != null) || !(base.transform != null))
		{
			return;
		}
		previewCharacterInstance = UnityEngine.Object.Instantiate(previewCharacter, base.transform);
		previewCharacterInstance.hideFlags = HideFlags.HideAndDontSave;
		previewCharacterInstance.GetComponentInChildren<Animator>().enabled = false;
		restPoseCache = new Dictionary<HumanBodyBones, Quaternion>();
		foreach (HumanBodyBones humanBodyBone in Enum.GetValues(typeof(HumanBodyBones)))
		{
			if (humanBodyBone != HumanBodyBones.LastBone && previewCharacterInstance.GetComponentInChildren<Animator>().GetBoneTransform(humanBodyBone) != null)
			{
				restPoseCache.Add(humanBodyBone, previewCharacterInstance.GetComponentInChildren<Animator>().GetBoneTransform(humanBodyBone).localRotation);
			}
		}
		previewCharacterIKSolver = previewCharacterInstance.GetComponentInChildren<IKSolver>();
		previewCharacterIKSolver.Initialize();
	}

	private void OnDisable()
	{
		DestroyPreview();
	}

	public void Advance(float time)
	{
		if (loops != null && loops.Count > 0)
		{
			if (animProgress < 0f)
			{
				animProgress = 0f;
			}
			modifiedProgress = Mathf.Clamp(progress, 0f, loops.Count - 1);
			float blendedSpeed = loops[Mathf.FloorToInt(modifiedProgress)].speed;
			if (modifiedProgress < (float)(loops.Count - 1))
			{
				blendedSpeed = Mathf.Lerp(blendedSpeed, loops[Mathf.CeilToInt(modifiedProgress)].speed, Mathf.Repeat(modifiedProgress, 1f));
			}
			animProgress = Mathf.Repeat(animProgress + time * blendedSpeed, 100f);
			for (int i = 0; i < 10; i++)
			{
				SetTargetPosition(i);
			}
		}
	}

	private void SetTargetPosition(int index)
	{
		for (int i = 0; i < ((!(modifiedProgress < (float)(loops.Count - 1))) ? 1 : 2); i++)
		{
			AnimationLooper currentLoop = loops[Mathf.FloorToInt(modifiedProgress) + i];
			Vector3 targetPosition = base.transform.TransformPoint(currentLoop.targetPositions[index]);
			Quaternion targetRotation = base.transform.rotation * currentLoop.targetRotations[index];
			if (currentLoop.motion[index] != null)
			{
				targetPosition += targetRotation * Vector3.right * (currentLoop.motion[index].X.Evaluate(Mathf.Repeat((animProgress + currentLoop.motion[index].offset.x * 0.01f + currentLoop.motionOffset[index]) * currentLoop.motion[index].timescale.x, 1f)) * (currentLoop.motion[index].scale.x * 0.01f * currentLoop.motionScale[index]));
				targetPosition += targetRotation * Vector3.up * (currentLoop.motion[index].Y.Evaluate(Mathf.Repeat((animProgress + currentLoop.motion[index].offset.y * 0.01f + currentLoop.motionOffset[index]) * currentLoop.motion[index].timescale.y, 1f)) * (currentLoop.motion[index].scale.y * 0.01f * currentLoop.motionScale[index]));
				targetPosition += targetRotation * Vector3.forward * (currentLoop.motion[index].Z.Evaluate(Mathf.Repeat((animProgress + currentLoop.motion[index].offset.z * 0.01f + currentLoop.motionOffset[index]) * currentLoop.motion[index].timescale.z, 1f)) * (currentLoop.motion[index].scale.z * 0.01f * currentLoop.motionScale[index]));
				targetRotation *= Quaternion.Euler(currentLoop.motion[index].RX.Evaluate(Mathf.Repeat((animProgress + currentLoop.motion[index].offsetR.x * 0.01f + currentLoop.motionOffset[index]) * currentLoop.motion[index].timescaleR.x, 1f)) * (currentLoop.motion[index].scaleR.x * currentLoop.motionScale[index]), 0f, 0f);
				targetRotation *= Quaternion.Euler(0f, currentLoop.motion[index].RY.Evaluate((animProgress + currentLoop.motion[index].offsetR.y * 0.01f + currentLoop.motionOffset[index]) * currentLoop.motion[index].timescaleR.y % 1f) * (currentLoop.motion[index].scaleR.y * currentLoop.motionScale[index]), 0f);
				targetRotation *= Quaternion.Euler(0f, 0f, currentLoop.motion[index].RZ.Evaluate((animProgress + currentLoop.motion[index].offsetR.z * 0.01f + currentLoop.motionOffset[index]) * currentLoop.motion[index].timescaleR.z % 1f) * (currentLoop.motion[index].scaleR.z * currentLoop.motionScale[index]));
			}
			if (currentLoop.attachments[index] != 0)
			{
				AnimationLooper attachLoop = currentLoop;
				Quaternion attachRotation = base.transform.rotation;
				if (currentLoop.attachmentTargets[index] != null)
				{
					attachLoop = currentLoop.attachmentTargets[index].loops[Mathf.FloorToInt(modifiedProgress) + i];
					attachRotation = currentLoop.attachmentTargets[index].transform.rotation;
				}
				int attachmentIndex = (int)(currentLoop.attachments[index] - 1);
				if (attachLoop.motion[attachmentIndex] != null && attachLoop != null)
				{
					targetPosition += attachRotation * attachLoop.targetRotations[attachmentIndex] * Vector3.right * (attachLoop.motion[attachmentIndex].X.Evaluate(Mathf.Repeat((animProgress + attachLoop.motion[attachmentIndex].offset.x * 0.01f + attachLoop.motionOffset[attachmentIndex]) * attachLoop.motion[attachmentIndex].timescale.x, 1f)) * attachLoop.motion[attachmentIndex].scale.x * 0.01f * attachLoop.motionScale[attachmentIndex]);
					targetPosition += attachRotation * attachLoop.targetRotations[attachmentIndex] * Vector3.up * (attachLoop.motion[attachmentIndex].Y.Evaluate(Mathf.Repeat((animProgress + attachLoop.motion[attachmentIndex].offset.y * 0.01f + attachLoop.motionOffset[attachmentIndex]) * attachLoop.motion[attachmentIndex].timescale.y, 1f)) * attachLoop.motion[attachmentIndex].scale.y * 0.01f * attachLoop.motionScale[attachmentIndex]);
					targetPosition += attachRotation * attachLoop.targetRotations[attachmentIndex] * Vector3.forward * (attachLoop.motion[attachmentIndex].Z.Evaluate(Mathf.Repeat((animProgress + attachLoop.motion[attachmentIndex].offset.z * 0.01f + attachLoop.motionOffset[attachmentIndex]) * attachLoop.motion[attachmentIndex].timescale.z, 1f)) * attachLoop.motion[attachmentIndex].scale.z * 0.01f * attachLoop.motionScale[attachmentIndex]);
				}
			}
			if (index == 1)
			{
				targetPosition += targetRotation * (new Vector3(hipOffset.x, 0f, hipOffset.y) * 0.25f);
			}
			currentLoop.computedTargetPositions[index] = targetPosition;
			currentLoop.computedTargetRotations[index] = targetRotation;
		}
	}

	[ContextMenu("Zero All Data")]
	public void ZeroData()
	{
		previewCharacterIKSolver.Initialize();
		previewCharacterInstance.transform.position = Vector3.zero;
		previewCharacterInstance.transform.rotation = Quaternion.identity;
		previewCharacterInstance.transform.position = base.transform.position;
		previewCharacterInstance.transform.rotation = base.transform.rotation;
		for (int j = 0; j < 10; j++)
		{
			loops[selectedLoop].targetPositions[j] = previewCharacterIKSolver.targets.GetLocalPosition(j);
			loops[selectedLoop].targetRotations[j] = previewCharacterIKSolver.targets.GetLocalRotation(j);
		}
		for (int i = 0; i < 10; i++)
		{
			loops[0].targetRotations[i] = Quaternion.identity;
		}
	}

	public void AddLoop()
	{
		if (loops == null)
		{
			loops = new List<AnimationLooper>();
		}
		AnimationLooper loop = ((loops.Count != 0) ? new AnimationLooper(loops[selectedLoop]) : new AnimationLooper());
		loops.Add(loop);
		selectedLoop = loops.Count - 1;
		progress = selectedLoop;
		if (loops.Count == 1)
		{
			ZeroData();
		}
	}

	public void RemoveLoop()
	{
		loops.RemoveAt(loops.Count - 1);
		if (selectedLoop > loops.Count - 1)
		{
			selectedLoop = loops.Count - 1;
		}
		progress = selectedLoop;
	}

	[ContextMenu("Add Joint Data")]
	public void AddJointData()
	{
		for (int loopIndex = 0; loopIndex < loops.Count; loopIndex++)
		{
			for (int j = 6; j < 10; j++)
			{
				loops[loopIndex].targetPositions[j] = previewCharacterIKSolver.targets.GetLocalPosition(j);
				loops[loopIndex].targetRotations[j] = Quaternion.Inverse(base.transform.rotation) * previewCharacterIKSolver.targets.GetLocalRotation(j);
			}
			for (int i = 6; i < 10; i++)
			{
				loops[loopIndex].targetRotations[i] = Quaternion.identity;
			}
		}
	}

	public Vector3 ComputeTargetPosition(int index)
	{
		Vector3 blendedPosition = loops[Mathf.FloorToInt(modifiedProgress)].computedTargetPositions[index];
		if (modifiedProgress < (float)(loops.Count - 1))
		{
			blendedPosition = Vector3.Lerp(blendedPosition, loops[Mathf.CeilToInt(modifiedProgress)].computedTargetPositions[index], Mathf.Repeat(modifiedProgress, 1f));
		}
		return blendedPosition;
	}

	public void SetLookAtPosition(Vector3 worldPoint)
	{
		lookAtPosition = worldPoint;
	}

	private Quaternion LookAtRotation(Quaternion currentHeadRotation)
	{
		Vector3 blendedHeadPos = Vector3.Lerp(loops[Mathf.FloorToInt(modifiedProgress)].computedTargetPositions[0], loops[Mathf.CeilToInt(modifiedProgress)].computedTargetPositions[0], Mathf.Repeat(modifiedProgress, 1f));
		return Quaternion.Lerp(Quaternion.FromToRotation(toDirection: (lookAtPosition - blendedHeadPos).normalized, fromDirection: currentHeadRotation * Vector3.forward) * currentHeadRotation, currentHeadRotation, 0.5f);
	}

	public Quaternion ComputeTargetRotation(int index)
	{
		Quaternion blendedRotation = loops[Mathf.FloorToInt(modifiedProgress)].computedTargetRotations[index];
		if (modifiedProgress >= (float)(loops.Count - 1))
		{
			if (index == 0 && Application.isPlaying)
			{
				return LookAtRotation(blendedRotation);
			}
			return blendedRotation;
		}
		blendedRotation = Quaternion.Lerp(loops[Mathf.FloorToInt(modifiedProgress)].computedTargetRotations[index], loops[Mathf.CeilToInt(modifiedProgress)].computedTargetRotations[index], Mathf.Repeat(modifiedProgress, 1f));
		if (index == 0 && Application.isPlaying)
		{
			return LookAtRotation(blendedRotation);
		}
		return blendedRotation;
	}

	public void SetPreview()
	{
		if (!(previewCharacterInstance != null) || previewCharacterIKSolver == null)
		{
			return;
		}
		previewCharacterInstance.transform.position = base.transform.position;
		previewCharacterInstance.transform.rotation = base.transform.rotation;
		previewCharacterIKSolver.ForceBlend(1f);
		foreach (KeyValuePair<HumanBodyBones, Quaternion> p in restPoseCache)
		{
			previewCharacterInstance.GetComponentInChildren<Animator>().GetBoneTransform(p.Key).localRotation = p.Value;
		}
		for (int i = 0; i < 10; i++)
		{
			previewCharacterIKSolver.SetTarget(i, ComputeTargetPosition(i), ComputeTargetRotation(i));
		}
		previewCharacterIKSolver.Solve();
	}

	public void DestroyPreview()
	{
		UnityEngine.Object.DestroyImmediate(previewCharacterInstance);
		previewCharacterInstance = null;
	}

	public void SetCharacter(IKSolver IK)
	{
		for (int i = 0; i < 10; i++)
		{
			IK.SetTarget(i, ComputeTargetPosition(i), ComputeTargetRotation(i));
		}
	}

	public static HashSet<AnimationStation> UpdateLinks(HashSet<AnimationStation> before, HashSet<AnimationStation> after)
	{
		before.ExceptWith(after);
		foreach (AnimationStation orphanStation in before)
		{
			orphanStation.linkedStations.hashSet.Clear();
			orphanStation.DestroyPreview();
		}
		foreach (AnimationStation station in after)
		{
			station.linkedStations.hashSet.Clear();
			station.linkedStations.hashSet.UnionWith(after);
		}
		after.UnionWith(before);
		return after;
	}

	public void LinkStation(AnimationStation stationToLink)
	{
		HashSet<AnimationStation> before = new HashSet<AnimationStation>(linkedStations.hashSet);
		linkedStations.hashSet.Add(stationToLink);
		HashSet<AnimationStation> after = new HashSet<AnimationStation>(linkedStations.hashSet);
		after.UnionWith(stationToLink.linkedStations.hashSet);
		UpdateLinks(before, after);
	}

	private void OnValidate()
	{
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "ico_fug.png", allowScaling: true);
	}

	private void OnDestroy()
	{
		foreach (AnimationStation linkedStation in linkedStations.hashSet)
		{
			if (linkedStation != this && linkedStation != null)
			{
				linkedStation.linkedStations.hashSet.Remove(this);
			}
		}
		DestroyPreview();
	}
}
