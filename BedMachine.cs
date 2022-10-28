using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class BedMachine : UsableMachine, IAnimationStationSet
{
	[SerializeField]
	private Sprite sleepingSprite;

	[SerializeField]
	private List<AnimationStation> stations;

	private ReadOnlyCollection<AnimationStation> readOnlyStations;

	private WaitForSeconds energyGrantPeriod;

	private void Awake()
	{
		readOnlyStations = stations.AsReadOnly();
		energyGrantPeriod = new WaitForSeconds(1f);
	}

	public override Sprite GetSprite(Kobold k)
	{
		return sleepingSprite;
	}

	public override bool CanUse(Kobold k)
	{
		if (k.GetEnergy() >= 1f)
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
		base.photonView.RPC("Sleep", RpcTarget.All, k.photonView.ViewID);
	}

	[PunRPC]
	private void Sleep(int targetID)
	{
		PhotonView view = PhotonNetwork.GetPhotonView(targetID);
		if (view != null && view.TryGetComponent<Kobold>(out var kobold))
		{
			StartCoroutine(SleepRoutine(kobold));
		}
	}

	private IEnumerator SleepRoutine(Kobold k)
	{
		bool stillSleeping = true;
		float startStimulation = k.stimulation;
		while (k != null && stillSleeping && k.GetEnergy() < 1f)
		{
			if (k.photonView.IsMine)
			{
				k.SetEnergyRPC(Mathf.Min(k.GetEnergy() + 0.2f, 1f));
				k.stimulation = Mathf.MoveTowards(startStimulation, 0f, 1f);
			}
			stillSleeping = false;
			foreach (AnimationStation station in GetAnimationStations())
			{
				if (station.info.user == k)
				{
					stillSleeping = true;
				}
			}
			yield return energyGrantPeriod;
		}
		if (stillSleeping && k.photonView.IsMine)
		{
			k.photonView.RPC("StopAnimationRPC", RpcTarget.All);
		}
	}

	public ReadOnlyCollection<AnimationStation> GetAnimationStations()
	{
		return readOnlyStations;
	}

}
