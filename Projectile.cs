using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using SkinnedMeshDecals;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile : GeneHolder, IPunObservable, ISavable, IPunInstantiateMagicCallback
{
	private Vector3 velocity;

	[SerializeField]
	private GameObject splash;

	[SerializeField]
	private GameObject projectile;

	[SerializeField]
	private AudioPack splashSound;

	[SerializeField]
	private VisualEffect splashEffect;

	[SerializeField]
	private Renderer projectileBlob;

	[SerializeField]
	private Material decalProjector;

	[SerializeField]
	private Material decalProjectorSubtractive;

	private ReagentContents contents;

	private HashSet<Collider> ignoreColliders;

	private static Collider[] colliders = new Collider[32];

	private static RaycastHit[] raycastHits = new RaycastHit[32];

	private static HashSet<GenericReagentContainer> hitContainers = new HashSet<GenericReagentContainer>();

	private bool splashed = false;

	private static readonly int FluidColor = Shader.PropertyToID("_FluidColor");

	private AudioSource splashSoundSource;

	private static readonly int ColorID = Shader.PropertyToID("_Color");

	private void Awake()
	{
		if (ignoreColliders == null)
		{
			ignoreColliders = new HashSet<Collider>();
		}
	}

	private void OnDestroy()
	{
		PlayAreaEnforcer.RemoveTrackedObject(base.photonView);
	}

	private void Update()
	{
		if (splashed)
		{
			return;
		}
		velocity += Physics.gravity * Time.deltaTime;
		int hits = Physics.SphereCastNonAlloc(base.transform.position, 0.25f, velocity.normalized, raycastHits, velocity.magnitude * Time.deltaTime, GameManager.instance.waterSprayHitMask, QueryTriggerInteraction.Ignore);
		int closestHit = -1;
		float closestDistance = float.MaxValue;
		for (int i = 0; i < hits; i++)
		{
			if (!ignoreColliders.Contains(raycastHits[i].collider) && !(raycastHits[i].point == Vector3.zero) && raycastHits[i].distance < closestDistance)
			{
				closestHit = i;
				closestDistance = raycastHits[i].distance;
			}
		}
		if (closestHit != -1)
		{
			base.transform.position = raycastHits[closestHit].point + raycastHits[closestHit].normal * 0.1f;
			OnSplash();
		}
		else
		{
			base.transform.position += velocity * Time.deltaTime;
			base.transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
		}
	}

	public void LaunchFrom(Rigidbody body)
	{
		ignoreColliders = new HashSet<Collider>();
		Collider[] componentsInChildren = body.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			ignoreColliders.Add(collider);
		}
	}

	private void Start()
	{
		splashSoundSource = base.gameObject.AddComponent<AudioSource>();
		splashSoundSource.maxDistance = 20f;
		splashSoundSource.minDistance = 0.25f;
		splashSoundSource.spatialBlend = 1f;
		splashSoundSource.playOnAwake = false;
		splashSoundSource.loop = false;
		splashSoundSource.rolloffMode = AudioRolloffMode.Linear;
		PlayAreaEnforcer.AddTrackedObject(base.photonView);
	}

	private void OnSplash()
	{
		Color color = contents.GetColor();
		decalProjector.SetColor(ColorID, color);
		decalProjectorSubtractive.SetColor(ColorID, color);
		PaintDecal.RenderDecalInSphere(base.transform.position, 1f, contents.IsCleaningAgent() ? decalProjectorSubtractive : decalProjector, Quaternion.identity, GameManager.instance.decalHitMask);
		splash.SetActive(value: true);
		projectile.SetActive(value: false);
		splashed = true;
		if (base.photonView.IsMine)
		{
			hitContainers.Clear();
			int hits = Physics.OverlapSphereNonAlloc(base.transform.position, 1f, colliders, GameManager.instance.waterSprayHitMask);
			for (int i = 0; i < hits; i++)
			{
				GenericReagentContainer container = colliders[i].GetComponentInParent<GenericReagentContainer>();
				if (container != null)
				{
					hitContainers.Add(container);
				}
			}
			float perVolume = contents.volume / (float)hitContainers.Count;
			foreach (GenericReagentContainer container2 in hitContainers)
			{
				container2.photonView.RPC("AddMixRPC", RpcTarget.All, contents.Spill(perVolume), base.photonView.ViewID);
			}
		}
		if (base.photonView.IsMine)
		{
			StartCoroutine(DestroyAfterTime());
		}
		base.transform.rotation = Quaternion.identity;
		if (splashSoundSource != null)
		{
			splashSound.Play(splashSoundSource);
		}
	}

	private IEnumerator DestroyAfterTime()
	{
		yield return new WaitForSeconds(5f);
		PhotonNetwork.Destroy(base.photonView);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(velocity);
			stream.SendNext(splashed);
			return;
		}
		velocity = (Vector3)stream.ReceiveNext();
		bool newSplash = (bool)stream.ReceiveNext();
		if (!splashed && newSplash)
		{
			OnSplash();
		}
		splashed = newSplash;
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(velocity.x);
		writer.Write(velocity.y);
		writer.Write(velocity.z);
		contents.Save(writer);
		bool hasGenes = GetGenes() != null;
		writer.Write(hasGenes);
		if (hasGenes)
		{
			GetGenes().Save(writer);
		}
		writer.Write(splashed);
	}

	public void Load(BinaryReader reader)
	{
		float vx = reader.ReadSingle();
		float vy = reader.ReadSingle();
		float vz = reader.ReadSingle();
		velocity = new Vector3(vx, vy, vz);
		contents = new ReagentContents();
		contents.Load(reader);
		if (reader.ReadBoolean())
		{
			KoboldGenes loadedGenes = new KoboldGenes();
			loadedGenes.Load(reader);
			SetGenes(loadedGenes);
		}
		if (reader.ReadBoolean() && !splashed)
		{
			OnSplash();
		}
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (!(info.photonView == null) && info.photonView.InstantiationData != null && info.photonView.InstantiationData[0] is ReagentContents)
		{
			contents = (ReagentContents)info.photonView.InstantiationData[0];
			Color color = contents.GetColor();
			splashEffect.SetVector4("Color", new Vector4(color.r, color.g, color.b, color.a));
			projectileBlob.material.SetColor(FluidColor, color);
			velocity = (Vector3)info.photonView.InstantiationData[1];
			splashed = false;
			SetGenes((KoboldGenes)info.photonView.InstantiationData[2]);
		}
	}
}
