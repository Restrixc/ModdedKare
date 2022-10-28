using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class BreedingStation : GenericUsable, IAnimationStationSet
{
	[SerializeField]
	private Sprite breedingSprite;

	[SerializeField]
	private List<AnimationStation> animationStations;

	private ReadOnlyCollection<AnimationStation> readOnlyStations;

	private void Awake()
	{
		readOnlyStations = animationStations.AsReadOnly();
	}

	public override Sprite GetSprite(Kobold k)
	{
		return breedingSprite;
	}

	public override bool CanUse(Kobold k)
	{
		foreach (AnimationStation station in animationStations)
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
		base.photonView.RequestOwnership();
		for (int i = 0; i < animationStations.Count; i++)
		{
			if (animationStations[i].info.user == null)
			{
				k.photonView.RPC("BeginAnimationRPC", RpcTarget.All, base.photonView.ViewID, i);
				break;
			}
		}
	}

	public ReadOnlyCollection<AnimationStation> GetAnimationStations()
	{
		return readOnlyStations;
	}

	
}
