using System;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class GenericReagentContainer : GeneHolder, IValuedGood, IPunObservable, ISavable
{
	public delegate void ContainerFilledAction(GenericReagentContainer container);

	[Serializable]
	public class InspectorReagent
	{
		public ScriptableReagent reagent;

		public float volume;
	}

	public enum ContainerType
	{
		OpenTop,
		Sealed,
		Mouth
	}

	public enum InjectType
	{
		Inject,
		Spray,
		Flood,
		Metabolize,
		Vacuum
	}

	[Serializable]
	public class ReagentContainerChangedEvent : UnityEvent<ReagentContents, InjectType>
	{
	}

	private static bool[,] ReagentMixMatrix = new bool[5, 3]
	{
		{ true, true, true },
		{ true, false, true },
		{ true, false, false },
		{ true, true, true },
		{ true, true, true }
	};

	public float startingMaxVolume = float.MaxValue;

	public ContainerType type;

	public ReagentContainerChangedEvent OnChange;

	public ReagentContainerChangedEvent OnFilled;

	public ReagentContainerChangedEvent OnEmpty;

	public InspectorReagent[] startingReagents;

	[SerializeField]
	private ReagentContents contents;

	private bool filled = false;

	private bool emptied = false;

	public float volume => contents.volume;

	public float maxVolume
	{
		get
		{
			return contents.GetMaxVolume();
		}
		set
		{
			contents.SetMaxVolume(value);
			OnChange.Invoke(contents, InjectType.Metabolize);
		}
	}

	public bool isFull => Mathf.Approximately(contents.volume, contents.GetMaxVolume());

	public bool isEmpty => Mathf.Approximately(contents.volume, 0f);

	public static event ContainerFilledAction containerFilled;

	public static event ContainerFilledAction containerInflated;

	public static bool IsMixable(ContainerType container, InjectType injectionType)
	{
		return ReagentMixMatrix[(int)injectionType, (int)container];
	}

	public Color GetColor()
	{
		return contents.GetColor();
	}

	public bool IsCleaningAgent()
	{
		return contents.IsCleaningAgent();
	}

	public float GetVolumeOf(ScriptableReagent reagent)
	{
		return contents.GetVolumeOf(reagent);
	}

	public float GetVolumeOf(short id)
	{
		return contents.GetVolumeOf(id);
	}

	public ReagentContents GetContents()
	{
		return contents;
	}

	protected void Awake()
	{
		if (OnChange == null)
		{
			OnChange = new ReagentContainerChangedEvent();
		}
		if (OnFilled == null)
		{
			OnFilled = new ReagentContainerChangedEvent();
		}
		if (OnEmpty == null)
		{
			OnEmpty = new ReagentContainerChangedEvent();
		}
		if (contents != null)
		{
			return;
		}
		contents = new ReagentContents(startingMaxVolume);
		if (startingReagents != null)
		{
			InspectorReagent[] array = startingReagents;
			foreach (InspectorReagent reagent in array)
			{
				AddMix(reagent.reagent, reagent.volume, InjectType.Inject);
			}
		}
	}

	public void Start()
	{
		filled = isFull;
		emptied = isEmpty;
	}

	[PunRPC]
	public ReagentContents Spill(float spillVolume)
	{
		ReagentContents spillContents = contents.Spill(spillVolume);
		OnReagentContentsChanged(InjectType.Vacuum);
		return spillContents;
	}

	private void TransferMix(GenericReagentContainer injector, float amount, InjectType injectType)
	{
		if (IsMixable(type, injectType) && base.photonView.IsMine)
		{
			ReagentContents spill = injector.Spill(amount);
			AddMix(spill, injectType);
			SetGenes(injector.GetGenes());
		}
	}

	private bool AddMix(ScriptableReagent incomingReagent, float volume, InjectType injectType)
	{
		if (!IsMixable(type, injectType) || !base.photonView.IsMine)
		{
			return false;
		}
		contents.AddMix(ReagentDatabase.GetID(incomingReagent), volume, this);
		return true;
	}

	private bool AddMix(ReagentContents incomingReagents, InjectType injectType)
	{
		if (!IsMixable(type, injectType) || !base.photonView.IsMine)
		{
			return false;
		}
		contents.AddMix(incomingReagents, this);
		OnReagentContentsChanged(injectType);
		return true;
	}

	[PunRPC]
	public void AddMixRPC(ReagentContents incomingReagents, int geneViewID)
	{
		PhotonView view = PhotonNetwork.GetPhotonView(geneViewID);
		GeneHolder geneHolder;
		if (view != null && view.TryGetComponent<Kobold>(out var kobold))
		{
			SetGenes(kobold.GetGenes());
		}
		else if (view != null && view.TryGetComponent<GeneHolder>(out geneHolder) && geneHolder.GetGenes() != null)
		{
			SetGenes(geneHolder.GetGenes());
		}
		contents.AddMix(incomingReagents, this);
		OnReagentContentsChanged(InjectType.Inject);
	}

	[PunRPC]
	public void ForceMixRPC(ReagentContents incomingReagents, int geneViewID)
	{
		PhotonView view = PhotonNetwork.GetPhotonView(geneViewID);
		GeneHolder geneHolder;
		if (view != null && view.TryGetComponent<Kobold>(out var kobold))
		{
			SetGenes(kobold.GetGenes());
		}
		else if (view != null && view.TryGetComponent<GeneHolder>(out geneHolder) && geneHolder.GetGenes() != null)
		{
			SetGenes(geneHolder.GetGenes());
		}
		maxVolume = Mathf.Max(contents.volume + incomingReagents.volume, maxVolume);
		if (TryGetComponent<Kobold>(out var kob))
		{
			Kobold kobold2 = kob;
			KoboldGenes koboldGenes = kob.GetGenes();
			float? bellySize = maxVolume;
			kobold2.SetGenes(koboldGenes.With(null, null, null, null, null, null, bellySize));
		}
		contents.AddMix(incomingReagents, this);
		OnReagentContentsChanged(InjectType.Inject);
		GenericReagentContainer.containerInflated?.Invoke(this);
	}

	public ReagentContents Peek()
	{
		return new ReagentContents(contents);
	}

	public ReagentContents Metabolize(float deltaTime)
	{
		return contents.Metabolize(deltaTime);
	}

	public void OverrideReagent(Reagent r)
	{
		contents.OverrideReagent(r.id, r.volume);
	}

	public void OverrideReagent(ScriptableReagent r, float volume)
	{
		contents.OverrideReagent(ReagentDatabase.GetID(r), volume);
	}

	public void OnReagentContentsChanged(InjectType injectType)
	{
		if (!filled && isFull)
		{
			OnFilled.Invoke(contents, injectType);
			GenericReagentContainer.containerFilled?.Invoke(this);
		}
		filled = isFull;
		OnChange.Invoke(contents, injectType);
		if (!emptied && isEmpty)
		{
			SetGenes(new KoboldGenes());
			OnEmpty.Invoke(contents, injectType);
		}
		emptied = isEmpty;
	}

	public void RefillToFullWithDefaultContents()
	{
		if (startingReagents.Length != 0)
		{
			InspectorReagent[] array = startingReagents;
			foreach (InspectorReagent item in array)
			{
				AddMix(item.reagent, item.volume, InjectType.Spray);
			}
		}
	}

	public float GetWorth()
	{
		return contents.GetValue();
	}

	public void OnValidate()
	{
		if (startingReagents != null)
		{
			float volume = 0f;
			InspectorReagent[] array = startingReagents;
			foreach (InspectorReagent reagent in array)
			{
				volume += reagent.volume;
			}
			startingMaxVolume = Mathf.Max(startingMaxVolume, volume);
		}
	}

	public override string ToString()
	{
		string blah = "[";
		foreach (ScriptableReagent reagent in ReagentDatabase.GetReagents())
		{
			if (contents.GetVolumeOf(reagent) != 0f)
			{
				blah = blah + reagent.name + ": " + contents.GetVolumeOf(reagent) + ", ";
			}
		}
		blah += "]";
		return base.ToString() + blah;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(contents);
			return;
		}
		ReagentContents newContents = (ReagentContents)stream.ReceiveNext();
		contents.Copy(newContents);
		OnReagentContentsChanged(InjectType.Metabolize);
	}

	public void Save(BinaryWriter writer)
	{
		if (contents == null)
		{
			contents = new ReagentContents(startingMaxVolume);
		}
		contents.Save(writer);
	}

	public void Load(BinaryReader reader)
	{
		if (contents == null)
		{
			contents = new ReagentContents(startingMaxVolume);
		}
		contents.Clear();
		contents.Load(reader);
		OnReagentContentsChanged(InjectType.Metabolize);
	}
}
