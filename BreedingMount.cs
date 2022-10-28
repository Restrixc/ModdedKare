using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class BreedingMount : UsableMachine, IAnimationStationSet
{
	[SerializeField]
	private Sprite useSprite;

	[SerializeField]
	private AnimationStation station;

	[SerializeField]
	private FluidStream stream;

	private ReadOnlyCollection<AnimationStation> stations;

	private GenericReagentContainer container;

	private void Awake()
	{
		container = base.gameObject.AddComponent<GenericReagentContainer>();
		container.type = GenericReagentContainer.ContainerType.Mouth;
		base.photonView.ObservedComponents.Add(container);
		container.OnChange.AddListener(OnReagentContentsChanged);
		List<AnimationStation> tempList = new List<AnimationStation>();
		tempList.Add(station);
		stations = tempList.AsReadOnly();
	}

	private void OnReagentContentsChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType)
	{
		base.photonView.RPC("FireStream", RpcTarget.All);
	}

	[PunRPC]
	private void FireStream()
	{
		stream.OnFire(container);
	}

	public override bool CanUse(Kobold k)
	{
		return constructed && k.GetEnergy() > 0f && station.info.user == null;
	}

	public override Sprite GetSprite(Kobold k)
	{
		return useSprite;
	}

	public override void LocalUse(Kobold k)
	{
		k.photonView.RPC("BeginAnimationRPC", RpcTarget.All, base.photonView.ViewID, 0);
	}

	public ReadOnlyCollection<AnimationStation> GetAnimationStations()
	{
		return stations;
	}

	
}
