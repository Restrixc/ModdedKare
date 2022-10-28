using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using KoboldKare;
using Naelstrof.Inflatable;
using PenetrationTech;
using Photon.Pun;
using Photon.Realtime;
using SkinnedMeshDecals;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Kobold : GeneHolder, IGrabbable, IPunObservable, IPunInstantiateMagicCallback, ISavable, IValuedGood
{
	[Serializable]
	public class PenetrableSet
	{
		public Penetrable penetratable;

		public Rigidbody ragdollAttachBody;

		public bool isFemaleExclusiveAnatomy = false;

		public bool canLayEgg = true;
	}

	public delegate void EnergyChangedAction(float value, float maxValue);

	public delegate void KoboldSpawnAction(Kobold kobold);

	private class UsableColliderComparer : IComparer<Collider>
	{
		private Vector3 checkPoint;

		public void SetCheckPoint(Vector3 position)
		{
			checkPoint = position;
		}

		public int Compare(Collider x, Collider y)
		{
			if ((object)x == y)
			{
				return 0;
			}
			if ((object)y == null)
			{
				return 1;
			}
			if ((object)x == null)
			{
				return -1;
			}
			float closestX = Vector3.Distance(checkPoint, x.ClosestPointOnBounds(checkPoint));
			float closestY = Vector3.Distance(checkPoint, y.ClosestPointOnBounds(checkPoint));
			return closestX.CompareTo(closestY);
		}
	}

	public StatusEffect koboldStatus;

	public List<PenetrableSet> penetratables = new List<PenetrableSet>();

	public List<Transform> attachPoints = new List<Transform>();

	public AudioClip[] yowls;

	public Animator animator;

	public Rigidbody body;

	public GameEventFloat MetabolizeEvent;

	[SerializeField]
	private Inflatable belly;

	private Grabber grabber;

	[SerializeField]
	private Inflatable fatnessInflater;

	[SerializeField]
	private Inflatable sizeInflater;

	[SerializeField]
	private Inflatable boobs;

	[SerializeField]
	private Material milkSplatMaterial;

	private UsableColliderComparer usableColliderComparer;

	public ReagentContents metabolizedContents;

	[SerializeField]
	private float energy = 1f;

	[SerializeField]
	private AudioPack tummyGrumbles;

	[FormerlySerializedAs("gurglePack")]
	[SerializeField]
	private AudioPack garglePack;

	public BodyProportionSimple bodyProportion;

	public TMP_Text chatText;

	public float textSpeedPerCharacter;

	public float minTextTimeout;

	[SerializeField]
	private PhotonGameObjectReference heartPrefab;

	[HideInInspector]
	public List<DickInfo.DickSet> activeDicks = new List<DickInfo.DickSet>();

	private AudioSource gargleSource;

	private AudioSource tummyGrumbleSource;

	public List<Renderer> koboldBodyRenderers;

	[SerializeField]
	private List<Transform> nipples;

	//public string CharacterName;

	public Transform hip;

	public KoboldCharacterController controller;

	public float stimulation = 0f;

	public float stimulationMax = 30f;

	public float stimulationMin = -30f;

	public Animator koboldAnimator;

	private float lastPumpTime = 0f;

	private List<Vector3> savedJointAnchors = new List<Vector3>();

	public float arousal = 0f;

	public GameObject nippleBarbells;

	private ReagentContents consumedReagents;

	private ReagentContents addbackReagents;

	private static Collider[] colliders = new Collider[32];

	private WaitForSeconds waitSpurt;

	private bool milking = false;

	private Coroutine displayMessageRoutine;

	public Ragdoller ragdoller;

	private Color internalHBCS;

	private static readonly int BrightnessContrastSaturation = Shader.PropertyToID("_HueBrightnessContrastSaturation");

	private static readonly int Carried = Animator.StringToHash("Carried");

	private static readonly int Quaff = Animator.StringToHash("Quaff");

	public GenericReagentContainer bellyContainer { get; private set; }

	public bool grabbed { get; private set; }

	public event EnergyChangedAction energyChanged;

	public static event KoboldSpawnAction spawned;

	public IEnumerable<InflatableListener> GetAllInflatableListeners()
	{
		foreach (InflatableListener inflatableListener in belly.GetInflatableListeners())
		{
			yield return inflatableListener;
		}
		foreach (InflatableListener inflatableListener2 in fatnessInflater.GetInflatableListeners())
		{
			yield return inflatableListener2;
		}
		foreach (InflatableListener inflatableListener3 in boobs.GetInflatableListeners())
		{
			yield return inflatableListener3;
		}
	}

	[PunRPC]
	public IEnumerator MilkRoutine()
	{
		while (milking)
		{
			yield return null;
		}
		milking = true;
		int pulses = 12;
		for (int i = 0; i < pulses; i++)
		{
			foreach (Transform t in GetNipples())
			{
				if (MozzarellaPool.instance.TryInstantiate(out var mozzarella))
				{
					mozzarella.SetFollowTransform(t);
					ReagentContents alloc = new ReagentContents();
					alloc.AddMix(ReagentDatabase.GetReagent("Milk").GetReagent(GetGenes().breastSize / (float)(pulses * GetNipples().Count)));
					mozzarella.SetVolumeMultiplier(alloc.volume);
					mozzarella.SetLocalForward(Vector3.up);
					Color color = alloc.GetColor();
					mozzarella.hitCallback += delegate(RaycastHit hit, Vector3 startPos, Vector3 dir, float length, float volume)
					{
						if (base.photonView.IsMine)
						{
							GenericReagentContainer componentInParent = hit.collider.GetComponentInParent<GenericReagentContainer>();
							if (componentInParent != null && this != null)
							{
								componentInParent.photonView.RPC("AddMixRPC", RpcTarget.All, alloc.Spill(alloc.volume * 0.1f), base.photonView.ViewID);
							}
						}
						milkSplatMaterial.color = color;
						PaintDecal.RenderDecalForCollider(hit.collider, milkSplatMaterial, hit.point - hit.normal * 0.1f, Quaternion.LookRotation(hit.normal, Vector3.up) * Quaternion.AngleAxis(UnityEngine.Random.Range(-180f, 180f), Vector3.forward), Vector2.one * (volume * 4f), length);
					};
				}
				mozzarella = null;
			}
			yield return waitSpurt;
		}
		milking = false;
	}

	public List<Transform> GetNipples()
	{
		return nipples;
	}

	public void AddStimulation(float s)
	{
		stimulation += s;
		if (base.photonView.IsMine && stimulation >= stimulationMax && TryConsumeEnergy(1))
		{
			base.photonView.RPC("Cum", RpcTarget.All);
		}
	}

	[PunRPC]
	public void Cum()
	{
		if (base.photonView.IsMine && activeDicks.Count == 0)
		{
			PhotonNetwork.Instantiate(heartPrefab.photonName, hip.transform.position, Quaternion.identity, 0, new object[1] { GetGenes() });
		}
		foreach (DickInfo.DickSet dickSet in activeDicks)
		{
			dickSet.info.StartCoroutine(dickSet.info.CumRoutine(dickSet));
		}
		PumpUpDick(1f);
		stimulation = stimulationMin;
	}

	public bool TryConsumeEnergy(byte amount)
	{
		if (energy < (float)(int)amount)
		{
			return false;
		}
		SetEnergyRPC(energy - (float)(int)amount);
		if (!base.photonView.IsMine)
		{
			base.photonView.RPC("SetEnergyRPC", RpcTarget.Others, energy);
		}
		return true;
	}

	[PunRPC]
	public void SetEnergyRPC(float newEnergy)
	{
		float diff = newEnergy - energy;
		if (diff < 0f && base.photonView.IsMine)
		{
			KoboldGenes koboldGenes = GetGenes();
			float? fatSize = Mathf.Max(GetGenes().fatSize + diff, 0f);
			SetGenes(koboldGenes.With(null, null, fatSize));
		}
		energy = newEnergy;
		energy = Mathf.Max(0f, energy);
		this.energyChanged?.Invoke(energy, GetGenes().maxEnergy);
	}

	private void RecursiveSetLayer(Transform t, int fromLayer, int toLayer)
	{
		for (int i = 0; i < t.childCount; i++)
		{
			RecursiveSetLayer(t.GetChild(i), fromLayer, toLayer);
		}
		if (t.gameObject.layer == fromLayer && t.GetComponent<Collider>() != null)
		{
			t.gameObject.layer = toLayer;
		}
	}

	public float GetEnergy()
	{
		return energy;
	}

	public float GetMaxEnergy()
	{
		if (GetGenes() == null)
		{
			return 1f;
		}
		return GetGenes().maxEnergy;
	}

	private float[] GetRandomProperties(float totalBudget, int count)
	{
		float[] properties = new float[count];
		float sum = 0f;
		for (int j = 0; j < count; j++)
		{
			properties[j] = UnityEngine.Random.Range(0f, totalBudget);
			sum += properties[j];
		}
		float x = totalBudget / sum;
		for (int i = 0; i < count; i++)
		{
			properties[i] *= x;
		}
		return properties;
	}

	[PunRPC]
	public void SetDickRPC(short dickID)
	{
		KoboldGenes koboldGenes = GetGenes();
		byte? dickEquip = (byte)dickID;
		SetGenes(koboldGenes.With(null, null, null, null, null, null, null, null, null, null, null, dickEquip));
	}

	public override void SetGenes(KoboldGenes newGenes)
	{
		KoboldInventory inventory = GetComponent<KoboldInventory>();
		Equipment crotchEquipment = inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch);
		if (crotchEquipment != null && EquipmentDatabase.GetID(crotchEquipment) != newGenes.dickEquip)
		{
			inventory.RemoveEquipment(crotchEquipment, PhotonNetwork.InRoom);
		}
		if (newGenes.dickEquip != byte.MaxValue && !inventory.Contains(EquipmentDatabase.GetEquipments()[newGenes.dickEquip]))
		{
			inventory.PickupEquipment(EquipmentDatabase.GetEquipments()[newGenes.dickEquip], null);
		}
		foreach (DickInfo.DickSet dickSet2 in activeDicks)
		{
			foreach (InflatableListener inflater in dickSet2.dickSizeInflater.GetInflatableListeners())
			{
				if (inflater is InflatableDick inflatableDick)
				{
					inflatableDick.SetDickThickness(newGenes.dickThickness);
				}
			}
			dickSet2.dickSizeInflater.SetSize(0.7f + Mathf.Log(1f + newGenes.dickSize / 20f, 2f), dickSet2.info);
			dickSet2.ballSizeInflater.SetSize(0.7f + Mathf.Log(1f + newGenes.ballSize / 20f, 2f), dickSet2.info);
		}
		grabber.SetMaxGrabCount(newGenes.grabCount);
		if (ragdoller.ragdolled)
		{
			sizeInflater.SetSizeInstant(Mathf.Max(Mathf.Log(1f + newGenes.baseSize / 20f, 2f), 0.2f));
		}
		else
		{
			sizeInflater.SetSize(Mathf.Max(Mathf.Log(1f + newGenes.baseSize / 20f, 2f), 0.2f), this);
		}
		fatnessInflater.SetSize(Mathf.Log(1f + newGenes.fatSize / 20f, 2f), this);
		boobs.SetSize(Mathf.Log(1f + newGenes.breastSize / 20f, 2f), this);
		bellyContainer.maxVolume = newGenes.bellySize;
		metabolizedContents.SetMaxVolume(newGenes.metabolizeCapacitySize);
		Vector4 hbcs = new Vector4((float)(int)newGenes.hue / 255f, (float)(int)newGenes.brightness / 255f, 0.5f, (float)(int)newGenes.saturation / 255f);
		foreach (Renderer r in koboldBodyRenderers)
		{
			if (r == null)
			{
				continue;
			}
			Material[] materials = r.materials;
			foreach (Material i in materials)
			{
				i.SetVector(BrightnessContrastSaturation, hbcs);
			}
			foreach (DickInfo.DickSet dickSet in activeDicks)
			{
				foreach (RendererSubMeshMask rendererMask in dickSet.dick.GetTargetRenderers())
				{
					if (!(rendererMask.renderer == null))
					{
						Material[] materials2 = rendererMask.renderer.materials;
						foreach (Material j in materials2)
						{
							j.SetVector(BrightnessContrastSaturation, hbcs);
						}
					}
				}
			}
		}
		this.energyChanged?.Invoke(energy, newGenes.maxEnergy);
		base.SetGenes(newGenes);
	}

	private void Awake()
	{
		waitSpurt = new WaitForSeconds(1f);
		usableColliderComparer = new UsableColliderComparer();
		grabber = GetComponentInChildren<Grabber>(includeInactive: true);
		consumedReagents = new ReagentContents();
		addbackReagents = new ReagentContents();
		bellyContainer = base.gameObject.AddComponent<GenericReagentContainer>();
		bellyContainer.type = GenericReagentContainer.ContainerType.Mouth;
		metabolizedContents = new ReagentContents(20f);
		bellyContainer.maxVolume = 20f;
		base.photonView.ObservedComponents.Add(bellyContainer);
		belly.OnEnable();
		sizeInflater.OnEnable();
		boobs.OnEnable();
		fatnessInflater.OnEnable();
		if (tummyGrumbleSource == null)
		{
			tummyGrumbleSource = hip.gameObject.AddComponent<AudioSource>();
			tummyGrumbleSource.playOnAwake = false;
			tummyGrumbleSource.maxDistance = 10f;
			tummyGrumbleSource.minDistance = 0.2f;
			tummyGrumbleSource.rolloffMode = AudioRolloffMode.Linear;
			tummyGrumbleSource.spatialBlend = 1f;
			tummyGrumbleSource.loop = false;
		}
		if (gargleSource == null)
		{
			gargleSource = base.gameObject.AddComponent<AudioSource>();
			gargleSource.playOnAwake = false;
			gargleSource.maxDistance = 10f;
			gargleSource.minDistance = 0.2f;
			gargleSource.rolloffMode = AudioRolloffMode.Linear;
			gargleSource.spatialBlend = 1f;
			gargleSource.loop = true;
		}
		belly.AddListener(new InflatableSoundPack(tummyGrumbles, tummyGrumbleSource, this));
		gameObject.AddComponent<cmod.KoboldExtension>();
	}

	private void Start()
	{
		lastPumpTime = Time.timeSinceLevelLoad;
		MetabolizeEvent.AddListener(OnEventRaised);
		bellyContainer.OnChange.AddListener(OnBellyContentsChanged);
		PlayAreaEnforcer.AddTrackedObject(base.photonView);
		if (GetGenes() == null)
		{
			SetGenes(new KoboldGenes().Randomize());
		}
		modding.ReferenceLua.CallFunction("OnCharacterSpawn", new modding.KoboldHookReference { genes = this.GetGenes(), kobold = this, transform = this.transform });
		chatText.alpha = 1f;
		chatText.text = gameObject.GetComponent<cmod.KoboldExtension>().koboldName;
	}

	private void OnDestroy()
	{
		MetabolizeEvent.RemoveListener(OnEventRaised);
		bellyContainer.OnChange.RemoveListener(OnBellyContentsChanged);
		PlayAreaEnforcer.RemoveTrackedObject(base.photonView);
	}

	[PunRPC]
	public void OnGrabRPC(int koboldID)
	{
		grabbed = true;
		animator.SetBool(Carried, value: true);
		controller.frictionMultiplier = 0.1f;
		controller.enabled = false;
		if (base.photonView.IsMine)
		{
			base.photonView.RPC("StopAnimationRPC", RpcTarget.All);
		}
	}

	public void PumpUpDick(float amount)
	{
		if (amount > 0f)
		{
			lastPumpTime = Time.timeSinceLevelLoad;
		}
		arousal += amount;
		arousal = Mathf.Clamp01(arousal);
	}

	public IEnumerator ThrowRoutine()
	{
		base.photonView.RPC("PushRagdoll", RpcTarget.All);
		yield return new WaitForSeconds(3f);
		base.photonView.RPC("PopRagdoll", RpcTarget.All);
	}

	private void OnValidate()
	{
		heartPrefab.OnValidate();
	}

	public bool CanGrab(Kobold kobold)
	{
		return !controller.inputJump;
	}

	[PunRPC]
	public void OnReleaseRPC(int koboldID, Vector3 velocity)
	{
		animator.SetBool(Carried, value: false);
		controller.frictionMultiplier = 1f;
		grabbed = false;
		controller.enabled = true;
		if (!base.photonView.IsMine)
		{
			return;
		}
		Rigidbody[] ragdollBodies = ragdoller.GetRagdollBodies();
		foreach (Rigidbody b in ragdollBodies)
		{
			b.velocity = velocity;
		}
		if (velocity.magnitude > 3f)
		{
			StartCoroutine(ThrowRoutine());
			return;
		}
		int hits = Physics.OverlapSphereNonAlloc(base.transform.position, Mathf.Max(1f, Mathf.Log(1f + base.transform.localScale.x, 2f)), colliders, GameManager.instance.usableHitMask, QueryTriggerInteraction.Collide);
		usableColliderComparer.SetCheckPoint(base.transform.position);
		Array.Sort(colliders, 0, hits, usableColliderComparer);
		for (int i = 0; i < hits; i++)
		{
			Collider c = colliders[i];
			GenericUsable usable = c.GetComponentInParent<GenericUsable>();
			if (usable != null && usable.CanUse(this))
			{
				usable.LocalUse(this);
				break;
			}
		}
	}

	private void Update()
	{
		foreach (DickInfo.DickSet dick in activeDicks)
		{
			dick.bonerInflater.SetSize(arousal * 0.95f + 0.05f * Mathf.Clamp01(Mathf.Sin(Time.time * 2f)) * arousal, dick.info);
		}
	}

	private void FixedUpdate()
	{
		if (grabbed)
		{
			PumpUpDick(Time.deltaTime * 0.1f);
		}
		if (!ragdoller.ragdolled)
		{
			body.angularVelocity -= body.angularVelocity * 0.2f;
			float deflectionForgivenessDegrees = 5f;
			Vector3 cross = Vector3.Cross(body.transform.up, Vector3.up);
			float angleDiff = Mathf.Max(Vector3.Angle(body.transform.up, Vector3.up) - deflectionForgivenessDegrees, 0f);
			body.AddTorque(cross * angleDiff, ForceMode.Acceleration);
		}
		if (Time.timeSinceLevelLoad - lastPumpTime > 10f)
		{
			PumpUpDick((0f - Time.deltaTime) * 0.01f);
		}
	}

	public void SendChat(string message)
	{
		base.photonView.RPC("RPCSendChat", RpcTarget.All, message);
	}

	[PunRPC]
	public void RPCSendChat(string message)
	{
		GameManager.instance.SpawnAudioClipInWorld(yowls[UnityEngine.Random.Range(0, yowls.Length)], base.transform.position);
		if (displayMessageRoutine != null)
		{
			StopCoroutine(displayMessageRoutine);
		}
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if (!((Kobold)player.TagObject != this))
			{
				CheatsProcessor.AppendText(gameObject.GetComponent<cmod.KoboldExtension>().koboldName + ": " + message + "\n");
				CheatsProcessor.ProcessCommand(this, message);
				displayMessageRoutine = StartCoroutine(DisplayMessage(message, minTextTimeout));
				break;
			}
		}
	}

	private IEnumerator DisplayMessage(string message, float duration)
	{
		duration += (float)message.Length * textSpeedPerCharacter;
		yield return new WaitForSeconds(duration);
		float endTime = Time.time + 1f;
		while (Time.time < endTime)
		{
			
			yield return null;
		}
	}

	public void InteractTo(Vector3 worldPosition, Quaternion worldRotation)
	{
		PumpUpDick(Time.deltaTime * 0.02f);
	}

	public bool IsPenetrating(Kobold k)
	{
		return false;
	}

	public bool PhysicsGrabbable()
	{
		return true;
	}

	public Transform GrabTransform()
	{
		return hip;
	}

	private float FloorNearestPower(float baseNum, float target)
	{
		float f;
		for (f = baseNum; f <= target; f *= baseNum)
		{
		}
		return f / baseNum;
	}

	public void ProcessReagents(ReagentContents contents)
	{
		addbackReagents.Clear();
		KoboldGenes genes = GetGenes();
		float newEnergy = energy;
		float passiveEnergyGeneration = 0.025f;
		if (newEnergy < 1f)
		{
			if (ragdoller.ragdolled)
			{
				passiveEnergyGeneration *= 4f;
			}
			newEnergy = Mathf.MoveTowards(newEnergy, 1.1f, passiveEnergyGeneration);
		}
		foreach (Reagent pair in contents)
		{
			ScriptableReagent reagent = ReagentDatabase.GetReagent(pair.id);
			float processedAmount = pair.volume;
			reagent.GetConsumptionEvent().OnConsume(this, reagent, ref processedAmount, ref consumedReagents, ref addbackReagents, ref genes, ref newEnergy);
			pair.volume -= processedAmount;
		}
		bellyContainer.AddMixRPC(contents, -1);
		bellyContainer.AddMixRPC(addbackReagents, -1);
		float overflowEnergy = Mathf.Max(newEnergy - GetMaxEnergy(), 0f);
		if (overflowEnergy != 0f)
		{
			KoboldGenes koboldGenes = genes;
			float? fatSize = genes.fatSize + overflowEnergy;
			genes = koboldGenes.With(null, null, fatSize);
		}
		SetGenes(genes);
		if (Math.Abs(energy - newEnergy) > 0.001f)
		{
			energy = Mathf.Clamp(newEnergy, 0f, GetMaxEnergy());
			this.energyChanged?.Invoke(energy, GetMaxEnergy());
		}
	}

	private void OnEventRaised(float f)
	{
		if (base.photonView.IsMine)
		{
			stimulation = Mathf.MoveTowards(stimulation, 0f, f * 0.08f);
			ReagentContents vol = bellyContainer.Metabolize(f);
			ProcessReagents(vol);
		}
	}

	private IEnumerator WaitAndThenStopGargling(float time)
	{
		yield return new WaitForSeconds(time);
		gargleSource.Pause();
		gargleSource.enabled = false;
	}

	private void OnBellyContentsChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType)
	{
		belly.SetSize(Mathf.Log(1f + contents.volume / 80f, 2f), this);
		if (injectType == GenericReagentContainer.InjectType.Spray && !(bellyContainer.volume >= bellyContainer.maxVolume))
		{
			koboldAnimator.SetTrigger(Quaff);
			if (!gargleSource.enabled || !gargleSource.isPlaying)
			{
				gargleSource.enabled = true;
				garglePack.Play(gargleSource);
				gargleSource.pitch = 1f;
				StartCoroutine(WaitAndThenStopGargling(0.25f));
			}
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(GetGenes());
			stream.SendNext(arousal);
			stream.SendNext(metabolizedContents);
			stream.SendNext(consumedReagents);
			stream.SendNext(energy);
			return;
		}
		SetGenes((KoboldGenes)stream.ReceiveNext());
		arousal = (float)stream.ReceiveNext();
		metabolizedContents.Copy((ReagentContents)stream.ReceiveNext());
		consumedReagents.Copy((ReagentContents)stream.ReceiveNext());
		float newEnergy = (float)stream.ReceiveNext();
		if (Math.Abs(newEnergy - energy) > 0.01f)
		{
			energy = newEnergy;
			this.energyChanged?.Invoke(energy, GetGenes().maxEnergy);
		}
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (info.photonView.InstantiationData == null)
		{
			Kobold.spawned?.Invoke(this);
			return;
		}
		if (info.photonView.InstantiationData.Length != 0 && info.photonView.InstantiationData[0] is KoboldGenes)
		{
			SetGenes((KoboldGenes)info.photonView.InstantiationData[0]);
		}
		else
		{
			SetGenes(new KoboldGenes().Randomize());
		}
		if (info.photonView.InstantiationData.Length > 1 && info.photonView.InstantiationData[1] is bool)
		{
			if ((bool)info.photonView.InstantiationData[1])
			{
				GetComponentInChildren<KoboldAIPossession>(includeInactive: true).gameObject.SetActive(value: false);
				if (info.Sender != null)
				{
					info.Sender.TagObject = this;
				}
			}
			else
			{
				GetComponentInChildren<KoboldAIPossession>(includeInactive: true).gameObject.SetActive(value: true);
				FarmSpawnEventHandler.TriggerProduceSpawn(base.gameObject);
			}
		}
		Kobold.spawned?.Invoke(this);
	}

	public void Save(BinaryWriter writer)
	{
		GetGenes().Save(writer);
		writer.Write(arousal);
		metabolizedContents.Save(writer);
		consumedReagents.Save(writer);
		bool isPlayerControlled = (Kobold)PhotonNetwork.LocalPlayer.TagObject == this;
		writer.Write(isPlayerControlled);
	}

	public void Load(BinaryReader reader)
	{
		KoboldGenes loadedGenes = new KoboldGenes();
		loadedGenes.Load(reader);
		SetGenes(loadedGenes);
		arousal = reader.ReadSingle();
		metabolizedContents.Load(reader);
		consumedReagents.Load(reader);
		if (reader.ReadBoolean())
		{
			PhotonNetwork.LocalPlayer.TagObject = this;
			GetComponentInChildren<KoboldAIPossession>(includeInactive: true).gameObject.SetActive(value: false);
			GetComponentInChildren<PlayerPossession>(includeInactive: true).gameObject.SetActive(value: true);
		}
	}

	public float GetWorth()
	{
		KoboldGenes genes = GetGenes();
		return 5f + Mathf.Log(1f + (genes.baseSize + genes.dickSize + genes.breastSize + genes.fatSize), 2f) * 6f;
	}

	
}
