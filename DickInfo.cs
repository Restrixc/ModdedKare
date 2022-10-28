using System;
using System.Collections;
using System.Collections.Generic;
using JigglePhysics;
using Naelstrof.Easing;
using Naelstrof.Inflatable;
using PenetrationTech;
using Photon.Pun;
using SkinnedMeshDecals;
using UnityEngine;

public class DickInfo : MonoBehaviour
{
	public delegate void CumThroughAction(Penetrable penetrable);

	private class KoboldDickListener : PenetratorListener
	{
		private readonly Kobold attachedKobold;

		private DickSet dickSet;

		private float lastDepthDist;

		private Penetrable penetrableMem;

		public KoboldDickListener(Kobold kobold, DickSet set)
		{
			attachedKobold = kobold;
			dickSet = set;
		}

		public override void OnPenetrationStart(Penetrable penetrable)
		{
			base.OnPenetrationStart(penetrable);
			penetrableMem = penetrable;
		}

		protected override void OnPenetrationDepthChange(float depthDist)
		{
			base.OnPenetrationDepthChange(depthDist);
			float movementAmount = depthDist - lastDepthDist;
			attachedKobold.PumpUpDick(Mathf.Abs(movementAmount * 0.15f));
			attachedKobold.AddStimulation(Mathf.Abs(movementAmount));
			lastDepthDist = depthDist;
			ClipListener clipListener = (ClipListener)penetrableMem.listeners.Find((PenetrableListener o) => o is ClipListener);
			dickSet.inside = depthDist != 0f && (depthDist < penetrableMem.GetSplinePath().arcLength || (clipListener != null && !clipListener.GetAllowForAllTheWayThrough()));
			dickSet.overpenetrated = depthDist >= penetrableMem.GetSplinePath().arcLength;
		}
	}

	[Serializable]
	public class DickSet
	{
		public Transform dickContainer;

		public Penetrator dick;

		public Inflatable ballSizeInflater;

		public Inflatable dickSizeInflater;

		public Inflatable bonerInflater;

		public Equipment.AttachPoint attachPoint;

		public Material cumSplatProjectorMaterial;

		public Vector3 attachPosition;

		public AudioPack cumSoundPack;

		[HideInInspector]
		public DickInfo info;

		public HumanBodyBones parent;

		[HideInInspector]
		public Transform parentTransform;

		public bool inside { get; set; }

		public bool overpenetrated { get; set; }

		public void Destroy()
		{
			UnityEngine.Object.Destroy(dick.gameObject);
		}
	}

	private static readonly int BrightnessContrastSaturation = Shader.PropertyToID("_HueBrightnessContrastSaturation");

	private Kobold attachedKobold;

	private bool cumming = false;

	[HideInInspector]
	public int equipmentInstanceID;

	public List<DickSet> dicks = new List<DickSet>();

	public static event CumThroughAction cumThrough;

	public void Awake()
	{
		foreach (DickSet set in dicks)
		{
			set.info = this;
			set.bonerInflater.OnEnable();
			set.dickSizeInflater.OnEnable();
			set.ballSizeInflater.OnEnable();
		}
	}

	public void RemoveFrom(Kobold k)
	{
		foreach (DickSet set2 in dicks)
		{
			if (!k.activeDicks.Contains(set2))
			{
				return;
			}
		}
		foreach (DickSet set in dicks)
		{
			k.activeDicks.Remove(set);
			foreach (Kobold.PenetrableSet penset in k.penetratables)
			{
				set.dick.RemoveIgnorePenetrable(penset.penetratable);
			}
		}
		bool shouldReenableVagina = true;
		foreach (DickSet dick in k.activeDicks)
		{
			if (dick.parent == HumanBodyBones.Hips)
			{
				shouldReenableVagina = false;
			}
		}
		if (shouldReenableVagina)
		{
			foreach (Kobold.PenetrableSet hole in k.penetratables)
			{
				if (hole.isFemaleExclusiveAnatomy)
				{
					hole.penetratable.gameObject.SetActive(value: true);
				}
			}
		}
		if (k == attachedKobold)
		{
			attachedKobold = null;
		}
	}

	public IEnumerator CumRoutine(DickSet set)
	{
		while (cumming)
		{
			yield return null;
		}
		cumming = true;
		float ballSize = attachedKobold.GetGenes().ballSize;
		float pulsesSample = (1f - 1f / (ballSize / 100f + 1f)) * 60f + 5f;
		int pulses = Mathf.CeilToInt(pulsesSample);
		float pulseDuration = 0.8f;
		for (int i = 0; i < pulses; i++)
		{
			GameManager.instance.SpawnAudioClipInWorld(set.cumSoundPack, set.dick.transform.position);
			float pulseStartTime = Time.time;
			while (Time.time < pulseStartTime + pulseDuration)
			{
				float t = (Time.time - pulseStartTime) / pulseDuration;
				foreach (RendererSubMeshMask renderTarget in set.dick.GetTargetRenderers())
				{
					Mesh mesh = ((SkinnedMeshRenderer)renderTarget.renderer).sharedMesh;
					float easingStart = Mathf.Clamp01(Easing.Cubic.InOut(1f - Mathf.Abs(t - 0.25f) * 4f));
					float easingMiddle = Mathf.Clamp01(Easing.Cubic.InOut(1f - Mathf.Abs(t - 0.5f) * 4f));
					float easingEnd = Mathf.Clamp01(Easing.Cubic.InOut(1f - Mathf.Abs(t - 0.75f) * 4f));
					((SkinnedMeshRenderer)renderTarget.renderer).SetBlendShapeWeight(mesh.GetBlendShapeIndex("Cum0"), easingStart * 100f);
					((SkinnedMeshRenderer)renderTarget.renderer).SetBlendShapeWeight(mesh.GetBlendShapeIndex("Cum1"), easingMiddle * 100f);
					((SkinnedMeshRenderer)renderTarget.renderer).SetBlendShapeWeight(mesh.GetBlendShapeIndex("Cum2"), easingEnd * 100f);
				}
				yield return null;
			}
			foreach (RendererSubMeshMask renderTarget2 in set.dick.GetTargetRenderers())
			{
				Mesh mesh2 = ((SkinnedMeshRenderer)renderTarget2.renderer).sharedMesh;
				((SkinnedMeshRenderer)renderTarget2.renderer).SetBlendShapeWeight(mesh2.GetBlendShapeIndex("Cum0"), 0f);
				((SkinnedMeshRenderer)renderTarget2.renderer).SetBlendShapeWeight(mesh2.GetBlendShapeIndex("Cum1"), 0f);
				((SkinnedMeshRenderer)renderTarget2.renderer).SetBlendShapeWeight(mesh2.GetBlendShapeIndex("Cum2"), 0f);
			}
			if (!set.dick.TryGetPenetrable(out var pennedHole) || !set.inside || pennedHole.GetComponentInParent<GenericReagentContainer>() == null)
			{
				if (set.overpenetrated)
				{
					DickInfo.cumThrough?.Invoke(pennedHole);
				}
				if (!MozzarellaPool.instance.TryInstantiate(out var mozzarella))
				{
					continue;
				}
				ReagentContents alloc = new ReagentContents();
				alloc.AddMix(ReagentDatabase.GetReagent("Cum").GetReagent(attachedKobold.GetGenes().ballSize / (float)pulses));
				mozzarella.SetVolumeMultiplier(alloc.volume * 2f);
				mozzarella.hitCallback += delegate(RaycastHit hit, Vector3 startPos, Vector3 dir, float length, float volume)
				{
					if (!(attachedKobold == null))
					{
						if (attachedKobold.photonView.IsMine)
						{
							GenericReagentContainer componentInParent = hit.collider.GetComponentInParent<GenericReagentContainer>();
							if (componentInParent != null && attachedKobold != null)
							{
								componentInParent.photonView.RPC("AddMixRPC", RpcTarget.All, alloc.Spill(alloc.volume * 0.1f), attachedKobold.photonView.ViewID);
							}
						}
						if (alloc.volume > 0f)
						{
							set.cumSplatProjectorMaterial.color = alloc.GetColor();
						}
						PaintDecal.RenderDecalForCollider(hit.collider, set.cumSplatProjectorMaterial, hit.point - hit.normal * 0.1f, Quaternion.LookRotation(hit.normal, Vector3.up) * Quaternion.AngleAxis(UnityEngine.Random.Range(-180f, 180f), Vector3.forward), Vector2.one * (volume * 4f), length);
					}
				};
				mozzarella.SetFollowPenetrator(set.dick);
			}
			else
			{
				PaintDecal.RenderDecalInSphere(pennedHole.GetSplinePath().GetPositionFromT(0f), rotation: Quaternion.LookRotation(pennedHole.GetSplinePath().GetVelocityFromT(0f), Vector3.up), radius: set.dick.transform.lossyScale.x * 0.25f, projector: set.cumSplatProjectorMaterial, hitMask: GameManager.instance.decalHitMask);
				GenericReagentContainer container = pennedHole.GetComponentInParent<GenericReagentContainer>();
				if (attachedKobold.photonView.IsMine)
				{
					ReagentContents alloc2 = new ReagentContents();
					alloc2.AddMix(ReagentDatabase.GetReagent("Cum").GetReagent(attachedKobold.GetGenes().ballSize / (float)pulses));
					container.photonView.RPC("AddMixRPC", RpcTarget.All, alloc2, attachedKobold.photonView.ViewID);
				}
				pennedHole = null;
			}
		}
		cumming = false;
	}

	public void AttachTo(Kobold k)
	{
		attachedKobold = k;
		bool animatorWasEnabled = k.animator.enabled;
		k.animator.enabled = true;
		foreach (DickSet set2 in dicks)
		{
			Vector3 scale = set2.dickContainer.localScale;
			set2.parentTransform = k.animator.GetBoneTransform(set2.parent);
			while (set2.parentTransform == null)
			{
				set2.parentTransform = k.animator.GetBoneTransform(set2.parent);
			}
			set2.info = this;
			set2.dickContainer.parent = k.attachPoints[(int)set2.attachPoint];
			set2.dickContainer.localScale = scale;
			set2.dickContainer.transform.localPosition = -set2.attachPosition;
			set2.dickContainer.transform.localRotation = Quaternion.identity;
			foreach (Kobold.PenetrableSet penset in k.penetratables)
			{
				set2.dick.AddIgnorePenetrable(penset.penetratable);
			}
			if (set2.parent == HumanBodyBones.Hips)
			{
				foreach (Kobold.PenetrableSet hole in attachedKobold.penetratables)
				{
					if (hole.isFemaleExclusiveAnatomy)
					{
						hole.penetratable.gameObject.SetActive(value: false);
					}
				}
			}
			set2.dick.listeners.Add(new KoboldDickListener(k, set2));
			k.activeDicks.Add(set2);
		}
		foreach (DickSet set in dicks)
		{
			JiggleRigBuilder[] componentsInChildren = set.dick.GetComponentsInChildren<JiggleRigBuilder>();
			foreach (JiggleRigBuilder rig in componentsInChildren)
			{
				rig.enabled = true;
			}
		}
		k.animator.enabled = animatorWasEnabled;
	}
}
