using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class ObjectiveManager : MonoBehaviourPun, ISavable, IPunObservable, IOnPhotonViewOwnerChange, IPhotonViewCallback
{
	public delegate void ObjectiveChangedAction(DragonMailObjective newObjective);

	private static ObjectiveManager instance;

	[SerializeReferenceButton]
	[SerializeField]
	[SerializeReference]
	private List<DragonMailObjective> objectives;

	private DragonMailObjective currentObjective;

	private int currentObjectiveIndex;

	private int stars = 0;

	private event ObjectiveChangedAction objectiveChanged;

	private event ObjectiveChangedAction objectiveUpdated;

	public static int GetStars()
	{
		return instance.stars;
	}

	public static void GiveStars(int count)
	{
		instance.stars += count;
		instance.objectiveChanged?.Invoke(null);
	}

	public static bool HasMail()
	{
		return instance.currentObjective == null && instance.currentObjectiveIndex < instance.objectives.Count;
	}

	public static void GetMail()
	{
		instance.SwitchToObjective((instance.currentObjectiveIndex >= instance.objectives.Count) ? null : instance.objectives[instance.currentObjectiveIndex]);
	}

	public static void AddObjectiveSwappedListener(ObjectiveChangedAction action)
	{
		instance.objectiveChanged += action;
	}

	public static void RemoveObjectiveSwappedListener(ObjectiveChangedAction action)
	{
		instance.objectiveChanged -= action;
	}

	public static void AddObjectiveUpdatedListener(ObjectiveChangedAction action)
	{
		instance.objectiveUpdated += action;
	}

	public static void RemoveObjectiveUpdatedListener(ObjectiveChangedAction action)
	{
		instance.objectiveUpdated -= action;
	}

	public static DragonMailObjective GetCurrentObjective()
	{
		return instance.currentObjective;
	}

	public static void SkipObjective()
	{
		instance.OnObjectiveComplete(GetCurrentObjective());
		instance.SwitchToObjective((instance.currentObjectiveIndex >= instance.objectives.Count) ? null : instance.objectives[instance.currentObjectiveIndex]);
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		SwitchToObjective(null);
		LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
	}

	private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
	{
		this.objectiveUpdated?.Invoke(currentObjective);
	}

	private void OnObjectiveComplete(DragonMailObjective objective)
	{
		currentObjectiveIndex++;
		if (objective != null && objective.autoAdvance)
		{
			SwitchToObjective(objectives[currentObjectiveIndex]);
			return;
		}
		stars++;
		SwitchToObjective(null);
	}

	private void OnObjectiveUpdated(DragonMailObjective objective)
	{
		this.objectiveUpdated?.Invoke(objective);
	}

	private void SwitchToObjective(DragonMailObjective newObjective)
	{
		if (newObjective == currentObjective)
		{
			return;
		}
		if (currentObjective != null)
		{
			currentObjective.Unregister();
			currentObjective.completed -= OnObjectiveComplete;
			currentObjective.updated -= OnObjectiveUpdated;
		}
		currentObjective = newObjective;
		if (newObjective != null)
		{
			if (base.photonView.IsMine)
			{
				currentObjective.Register();
			}
			currentObjective.completed += OnObjectiveComplete;
			currentObjective.updated += OnObjectiveUpdated;
		}
		this.objectiveChanged?.Invoke(currentObjective);
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(stars);
		bool hasObjective = currentObjective != null;
		writer.Write(hasObjective);
		writer.Write(currentObjectiveIndex);
		if (hasObjective)
		{
			objectives[currentObjectiveIndex].Save(writer);
		}
	}

	public void Load(BinaryReader reader)
	{
		stars = reader.ReadInt32();
		bool hasObjective = reader.ReadBoolean();
		currentObjectiveIndex = reader.ReadInt32();
		SwitchToObjective(hasObjective ? objectives[currentObjectiveIndex] : null);
		if (hasObjective)
		{
			objectives[currentObjectiveIndex].Load(reader);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			bool hasObjective2 = currentObjective != null;
			stream.SendNext(stars);
			stream.SendNext(hasObjective2);
			stream.SendNext(currentObjectiveIndex);
		}
		else
		{
			stars = (int)stream.ReceiveNext();
			bool hasObjective = (bool)stream.ReceiveNext();
			currentObjectiveIndex = (int)stream.ReceiveNext();
			SwitchToObjective(hasObjective ? GetCurrentObjective() : null);
		}
		objectives[currentObjectiveIndex].OnPhotonSerializeView(stream, info);
	}

	private void OnValidate()
	{
		foreach (DragonMailObjective objective in objectives)
		{
			objective?.OnValidate();
		}
	}

	public void OnOwnerChange(Player newOwner, Player previousOwner)
	{
		if (currentObjective != null)
		{
			SwitchToObjective(currentObjective);
		}
	}
}
