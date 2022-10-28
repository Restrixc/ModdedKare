using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class GrinderManager : UsableMachine, IAnimationStationSet
{
	public delegate void GrindedObjectAction(ReagentContents contents);

	[SerializeField]
	private Sprite onSprite;

	[SerializeField]
	private List<Collider> cylinderColliders;

	public AudioSource grindSound;

	public Animator animator;

	public AudioSource deny;

	[SerializeField]
	private AnimationStation station;

	private ReadOnlyCollection<AnimationStation> stations;

	[SerializeField]
	private FluidStream fluidStream;

	private HashSet<PhotonView> grindedThingsCache;

	private bool grinding;

	[SerializeField]
	private Collider grindingCollider;

	[SerializeField]
	private GenericReagentContainer container;

	public static event GrindedObjectAction grindedObject;

	public override Sprite GetSprite(Kobold k)
	{
		return onSprite;
	}

	public override bool CanUse(Kobold k)
	{
		return k.GetEnergy() >= 1f && !grinding && station.info.user == null && constructed;
	}

	public override void LocalUse(Kobold k)
	{
		k.photonView.RPC("BeginAnimationRPC", RpcTarget.All, base.photonView.ViewID, 0);
		base.LocalUse(k);
	}

	[PunRPC]
	private void BeginGrind()
	{
		grinding = true;
		animator.SetBool("Grinding", value: true);
		grindSound.enabled = true;
		grindSound.Play();
		foreach (Collider cylinderCollider in cylinderColliders)
		{
			cylinderCollider.enabled = false;
		}
	}

	[PunRPC]
	private void StopGrind()
	{
		grinding = false;
		animator.SetBool("Grinding", value: false);
		grindSound.Stop();
		grindSound.enabled = false;
		foreach (Collider cylinderCollider in cylinderColliders)
		{
			cylinderCollider.enabled = true;
		}
	}

	private IEnumerator WaitThenConsumeEnergy()
	{
		yield return new WaitForSeconds(8f);
		if (base.photonView.IsMine && !(station.info.user == null) && station.info.user.TryConsumeEnergy(1))
		{
			station.info.user.photonView.RPC("StopAnimationRPC", RpcTarget.All);
			base.photonView.RPC("BeginGrind", RpcTarget.All);
			yield return new WaitForSeconds(18f);
			base.photonView.RPC("StopGrind", RpcTarget.All);
		}
	}

	protected override void Start()
	{
		base.Start();
		grindedThingsCache = new HashSet<PhotonView>();
		List<AnimationStation> tempList = new List<AnimationStation>();
		tempList.Add(station);
		stations = tempList.AsReadOnly();
		container.OnChange.AddListener(OnReagentsChanged);
	}

	private void OnReagentsChanged(ReagentContents contents, GenericReagentContainer.InjectType inject)
	{
		if (fluidStream.isActiveAndEnabled)
		{
			fluidStream.OnFire(container);
		}
	}

	[PunRPC]
	public override void Use()
	{
		StopCoroutine("WaitThenConsumeEnergy");
		StartCoroutine("WaitThenConsumeEnergy");
	}

	private IEnumerator WaitAndThenClear()
	{
		yield return new WaitForSeconds(2f);
		grindedThingsCache.Clear();
	}

	[PunRPC]
	private void Grind(ReagentContents incomingContents, KoboldGenes genes)
	{
		GrinderManager.grindedObject?.Invoke(incomingContents);
		container.AddMixRPC(incomingContents, base.photonView.ViewID);
		container.SetGenes(genes);
		fluidStream.OnFire(container);
	}

	private void HandleCollision(Collider other)
	{
		if (!grinding || other.isTrigger || !Physics.ComputePenetration(grindingCollider, grindingCollider.transform.position, grindingCollider.transform.rotation, other, other.transform.position, other.transform.rotation, out var _, out var distance) || distance == 0f)
		{
			return;
		}
		PhotonView view = other.GetComponentInParent<PhotonView>();
		if (view == null || grindedThingsCache.Contains(view))
		{
			return;
		}
		grindedThingsCache.Add(view);
		StopCoroutine("WaitAndThenClear");
		StartCoroutine("WaitAndThenClear");
		Kobold kobold = other.GetComponentInParent<Kobold>();
		if (kobold != null)
		{
			kobold.StartCoroutine(RagdollForTime(kobold));
			Rigidbody[] allComponents = other.GetAllComponents<Rigidbody>();
			foreach (Rigidbody r in allComponents)
			{
				r.AddExplosionForce(400f, base.transform.position + Vector3.down * 5f, 100f);
			}
			if (!deny.isPlaying)
			{
				deny.Play();
			}
		}
		else if (view.IsMine)
		{
			GenericReagentContainer genericReagentContainer = view.GetComponentInChildren<GenericReagentContainer>();
			if (genericReagentContainer != null)
			{
				base.photonView.RPC("Grind", RpcTarget.All, genericReagentContainer.GetContents(), genericReagentContainer.GetGenes());
			}
			IDamagable d = view.GetComponentInParent<IDamagable>();
			if (d != null)
			{
				d.Damage(d.GetHealth() + 1f);
			}
			else
			{
				PhotonNetwork.Destroy(view.gameObject);
			}
		}
	}

	private IEnumerator RagdollForTime(Kobold kobold)
	{
		kobold.photonView.RPC("PushRagdoll", RpcTarget.All);
		yield return new WaitForSeconds(3f);
		kobold.photonView.RPC("PopRagdoll", RpcTarget.All);
	}

	private void OnTriggerEnter(Collider other)
	{
		HandleCollision(other);
	}

	private void OnTriggerStay(Collider other)
	{
		HandleCollision(other);
	}

	public ReadOnlyCollection<AnimationStation> GetAnimationStations()
	{
		return stations;
	}

	public override void Load(BinaryReader reader)
	{
		base.Load(reader);
		bool newGrinding = reader.ReadBoolean();
		if (!grinding && newGrinding)
		{
			BeginGrind();
		}
		else if (grinding && !newGrinding)
		{
			StopGrind();
		}
	}

	public override void Save(BinaryWriter writer)
	{
		base.Save(writer);
		writer.Write(grinding);
	}

	
}
