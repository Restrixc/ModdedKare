using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class KoboldPress : UsableMachine, IAnimationStationSet
{
	[SerializeField]
	private List<AnimationStation> stations;

	[SerializeField]
	private Sprite useSprite;

	[SerializeField]
	private FluidStream stream;

	private ReadOnlyCollection<AnimationStation> readOnlyStations;

	private GenericReagentContainer container;

	protected override void Start()
	{
		base.Start();
		readOnlyStations = stations.AsReadOnly();
		container = base.gameObject.AddComponent<GenericReagentContainer>();
		container.type = GenericReagentContainer.ContainerType.Mouth;
		base.photonView.ObservedComponents.Add(container);
		container.OnChange.AddListener(OnReagentContentsChanged);
	}

	private void OnReagentContentsChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType)
	{
		stream.OnFire(container);
	}

	public override Sprite GetSprite(Kobold k)
	{
		return useSprite;
	}

	public override bool CanUse(Kobold k)
	{
		if (!constructed)
		{
			return false;
		}
		if (stations[0].info.user == null)
		{
			return k.bellyContainer.volume > 0f;
		}
		return false;
	}

	public override void LocalUse(Kobold k)
	{
		base.LocalUse(k);
		if (stations[0].info.user == null)
		{
			k.photonView.RPC("BeginAnimationRPC", RpcTarget.All, base.photonView.ViewID, 0);
		}
	}

	[PunRPC]
	public override void Use()
	{
		base.Use();
		StopAllCoroutines();
		StartCoroutine(CrusherRoutine());
	}

	private IEnumerator CrusherRoutine()
	{
		yield return new WaitForSeconds(6f);
		if (base.photonView.IsMine)
		{
			Kobold pressedKobold = stations[0].info.user;
			if (!(pressedKobold == null))
			{
				pressedKobold.photonView.RPC("Spill", RpcTarget.Others, pressedKobold.bellyContainer.volume);
				ReagentContents spilled = pressedKobold.bellyContainer.Spill(pressedKobold.bellyContainer.volume);
				container.photonView.RPC("AddMixRPC", RpcTarget.All, spilled, pressedKobold.photonView.ViewID);
			}
		}
	}

	public ReadOnlyCollection<AnimationStation> GetAnimationStations()
	{
		return readOnlyStations;
	}

	
}
