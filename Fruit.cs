using System.IO;
using System.Runtime.CompilerServices;
using Naelstrof.Inflatable;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

public class Fruit : MonoBehaviourPun, IDamagable, IAdvancedInteractable, IPunObservable, ISavable, IGrabbable, ISpoilable, IPunInstantiateMagicCallback
{
	[SerializeField]
	private VisualEffect itemParticles;

	private Rigidbody body;

	[SerializeField]
	private VisualEffect gibSplash;

	private float health = 100f;

	public GenericReagentContainer.InspectorReagent startingReagent;

	private GenericReagentContainer container;

	private Renderer[] renderers;

	[SerializeField]
	private Inflatable fruitInflater;

	[SerializeField]
	private bool startFrozen = true;

	[SerializeField]
	private AudioPack gibSound;

	[SerializeField]
	private Transform centerTransform;

	private void SetFrozen(bool frozen)
	{
		itemParticles.enabled = frozen;
		body.constraints = (frozen ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None);
	}

	private bool GetFrozen()
	{
		return itemParticles.enabled;
	}

	private void Awake()
	{
		body = GetComponent<Rigidbody>();
		renderers = GetComponentsInChildren<Renderer>();
		container = base.gameObject.AddComponent<GenericReagentContainer>();
		container.type = GenericReagentContainer.ContainerType.Mouth;
		InflatableTransform inflatableTransform = new InflatableTransform();
		inflatableTransform.SetTransform(base.transform);
		fruitInflater.AddListener(inflatableTransform);
		fruitInflater.OnEnable();
		container.OnChange.AddListener(OnReagentContentsChanged);
		container.GetContents().AddMix(startingReagent.reagent.GetReagent(startingReagent.volume), container);
		OnReagentContentsChanged(container.GetContents(), GenericReagentContainer.InjectType.Inject);
		base.photonView.ObservedComponents.Add(container);
		if (centerTransform == null)
		{
			centerTransform = base.transform;
		}
	}

	private void OnReagentContentsChanged(ReagentContents contents, GenericReagentContainer.InjectType type)
	{
		fruitInflater.SetSize(Mathf.Log(1f + contents.volume / 20f, 2f), this);
	}

	private void Start()
	{
		SpoilableHandler.AddSpoilable(this);
		SetFrozen(startFrozen);
		PlayAreaEnforcer.AddTrackedObject(base.photonView);
	}

	private void OnDestroy()
	{
		SpoilableHandler.RemoveSpoilable(this);
		PlayAreaEnforcer.RemoveTrackedObject(base.photonView);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.rigidbody != null && !collision.rigidbody.isKinematic && collision.impulse.magnitude > 0.1f)
		{
			SetFrozen(frozen: false);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(GetFrozen());
			stream.SendNext(health);
		}
		else
		{
			SetFrozen((bool)stream.ReceiveNext());
			health = (float)stream.ReceiveNext();
		}
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(GetFrozen());
		writer.Write(health);
	}

	public void Load(BinaryReader reader)
	{
		SetFrozen(reader.ReadBoolean());
		health = reader.ReadSingle();
	}

	public void InteractTo(Vector3 worldPosition, Quaternion worldRotation)
	{
	}

	public void OnInteract(Kobold k)
	{
		SetFrozen(frozen: false);
	}

	public void OnEndInteract()
	{
	}

	public bool PhysicsGrabbable()
	{
		return true;
	}

	public float GetHealth()
	{
		return health;
	}

	private void Die()
	{
		GameObject obj = Object.Instantiate(gibSplash.gameObject);
		obj.transform.position = base.transform.position;
		VisualEffect effect = obj.GetComponentInChildren<VisualEffect>();
		effect.SetVector4("Color", container.GetColor());
		GameManager.instance.SpawnAudioClipInWorld(gibSound, base.transform.position);
		Object.Destroy(obj, 5f);
		if (base.photonView.IsMine)
		{
			PhotonNetwork.Destroy(base.photonView.gameObject);
		}
	}

	[PunRPC]
	public void Damage(float amount)
	{
		health -= amount;
		if (health <= 0f)
		{
			Die();
			health = 0f;
		}
	}

	public void Heal(float amount)
	{
		health += amount;
	}

	[PunRPC]
	public void OnGrabRPC(int koboldID)
	{
		SetFrozen(frozen: false);
	}

	public bool CanGrab(Kobold kobold)
	{
		return true;
	}

	[PunRPC]
	public void OnReleaseRPC(int koboldId, Vector3 velocity)
	{
	}

	public Transform GrabTransform()
	{
		return centerTransform;
	}

	public void OnSpoil()
	{
		Die();
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		FarmSpawnEventHandler.TriggerProduceSpawn(base.gameObject);
	}

	
}
