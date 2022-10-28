using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class MilkingTable : UsableMachine, IAnimationStationSet
{
	[SerializeField]
	private Sprite milkingSprite;

	[SerializeField]
	private List<AnimationStation> stations;

	private ReadOnlyCollection<AnimationStation> readOnlyStations;

	[SerializeField]
	private FluidStream stream;

	private GenericReagentContainer container;

	private void Awake()
	{
		readOnlyStations = stations.AsReadOnly();
		container = base.gameObject.AddComponent<GenericReagentContainer>();
		container.type = GenericReagentContainer.ContainerType.Mouth;
		container.OnChange.AddListener(OnReagentContainerChangedEvent);
		base.photonView.ObservedComponents.Add(container);
	}

	private void OnReagentContainerChangedEvent(ReagentContents contents, GenericReagentContainer.InjectType injectType)
	{
		stream.OnFire(container);
	}

	public override Sprite GetSprite(Kobold k)
	{
		return milkingSprite;
	}

	public override bool CanUse(Kobold k)
	{
		if (k.GetEnergy() < 1f || !constructed)
		{
			return false;
		}
		foreach (AnimationStation station in stations)
		{
			if (station.info.user == null)
			{
				return true;
			}
		}
		return false;
	}

	public override void LocalUse(Kobold k)
	{
		for (int i = 0; i < stations.Count; i++)
		{
			if (stations[i].info.user == null)
			{
				k.photonView.RPC("BeginAnimationRPC", RpcTarget.All, base.photonView.ViewID, i);
				break;
			}
		}
		base.LocalUse(k);
	}

	public override void Use()
	{
		StopAllCoroutines();
		StartCoroutine(WaitThenMilk());
	}

	private IEnumerator WaitThenMilk()
	{
		yield return new WaitForSeconds(6f);
		if (!base.photonView.IsMine)
		{
			yield break;
		}
		for (int j = 0; j < stations.Count; j++)
		{
			if (stations[j].info.user == null || stations[j].info.user.GetEnergy() <= 0f)
			{
				yield break;
			}
		}
		for (int i = 0; i < stations.Count; i++)
		{
			if (!stations[i].info.user.TryConsumeEnergy(1))
			{
				yield break;
			}
		}
		stations[0].info.user.photonView.RPC("MilkRoutine", RpcTarget.All);
	}

	public ReadOnlyCollection<AnimationStation> GetAnimationStations()
	{
		return readOnlyStations;
	}

	
}
