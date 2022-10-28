using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.VFX;
using Vilar.AnimationStation;

public class BrainSwapperMachine : UsableMachine, IAnimationStationSet
{
	public delegate void BodySwapAction(Kobold a, Kobold b);

	[SerializeField]
	private Sprite sleepingSprite;

	[SerializeField]
	private List<AnimationStation> stations;

	[SerializeField]
	private VisualEffect lightning;

	[SerializeField]
	private AudioPack thunderSound;

	[SerializeField]
	private AudioPack brainSwapSound;

	private AudioSource brainSwapSoundSource;

	private ReadOnlyCollection<AnimationStation> readOnlyStations;

	public static event BodySwapAction bodySwapped;

	private void Awake()
	{
		readOnlyStations = stations.AsReadOnly();
		if (brainSwapSoundSource == null)
		{
			brainSwapSoundSource = base.gameObject.AddComponent<AudioSource>();
			brainSwapSoundSource.playOnAwake = false;
			brainSwapSoundSource.maxDistance = 10f;
			brainSwapSoundSource.minDistance = 0.2f;
			brainSwapSoundSource.rolloffMode = AudioRolloffMode.Linear;
			brainSwapSoundSource.spatialBlend = 1f;
			brainSwapSoundSource.loop = false;
		}
	}

	public override Sprite GetSprite(Kobold k)
	{
		return sleepingSprite;
	}

	public override bool CanUse(Kobold k)
	{
		if (!constructed)
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
		base.photonView.RPC("SwapAfterTime", RpcTarget.All);
	}

	[PunRPC]
	private IEnumerator SwapAfterTime()
	{
		yield return new WaitForSeconds(4f);
		if (!base.photonView.IsMine || stations[0].info.user == null || stations[1].info.user == null)
		{
			yield break;
		}
		Player aPlayer = null;
		Player bPlayer = null;
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if ((Kobold)player.TagObject == stations[0].info.user)
			{
				aPlayer = player;
			}
			if ((Kobold)player.TagObject == stations[1].info.user)
			{
				bPlayer = player;
			}
		}
		base.photonView.RPC("AssignKobolds", RpcTarget.AllBufferedViaServer, stations[0].info.user.photonView.ViewID, stations[1].info.user.photonView.ViewID, bPlayer?.ActorNumber ?? (-1), aPlayer?.ActorNumber ?? (-1), stations[1].info.user.GetComponent<MoneyHolder>().GetMoney(), stations[0].info.user.GetComponent<MoneyHolder>().GetMoney());
		stations[0].info.user.photonView.RPC("StopAnimationRPC", RpcTarget.All);
		stations[1].info.user.photonView.RPC("StopAnimationRPC", RpcTarget.All);
	}

	[PunRPC]
	public void AssignKobolds(int aViewID, int bViewID, int playerIDA, int playerIDB, float moneyA, float moneyB)
	{
		thunderSound.PlayOneShot(brainSwapSoundSource);
		lightning.Play();
		brainSwapSound.Play(brainSwapSoundSource);
		PhotonView aView = PhotonNetwork.GetPhotonView(aViewID);
		PhotonView bView = PhotonNetwork.GetPhotonView(bViewID);
		Player[] playerList = PhotonNetwork.PlayerList;
		Player aPlayer = null;
		Player bPlayer = null;
		Player[] array = playerList;
		foreach (Player player in array)
		{
			if (player.ActorNumber == playerIDA)
			{
				aPlayer = player;
			}
			if (player.ActorNumber == playerIDB)
			{
				bPlayer = player;
			}
		}
		if (aPlayer == null && bPlayer == null)
		{
			return;
		}
		if (aView != null)
		{
			if (aPlayer == PhotonNetwork.LocalPlayer)
			{
				aView.GetComponentInChildren<PlayerPossession>(includeInactive: true).gameObject.SetActive(value: true);
				aView.GetComponentInChildren<KoboldAIPossession>(includeInactive: true).gameObject.SetActive(value: false);
			}
			else
			{
				aView.GetComponentInChildren<PlayerPossession>(includeInactive: true).gameObject.SetActive(value: false);
				aView.GetComponentInChildren<KoboldAIPossession>(includeInactive: true).gameObject.SetActive(value: true);
			}
			if (aView.TryGetComponent<MoneyHolder>(out var aMoneyHolder))
			{
				aMoneyHolder.SetMoney(moneyA);
			}
			if (aView.TryGetComponent<Kobold>(out var aKobold) && aPlayer != null)
			{
				aPlayer.TagObject = aKobold;
			}
		}
		if (bView != null)
		{
			if (bPlayer == PhotonNetwork.LocalPlayer)
			{
				bView.GetComponentInChildren<PlayerPossession>(includeInactive: true).gameObject.SetActive(value: true);
				bView.GetComponentInChildren<KoboldAIPossession>(includeInactive: true).gameObject.SetActive(value: false);
			}
			else
			{
				bView.GetComponentInChildren<PlayerPossession>(includeInactive: true).gameObject.SetActive(value: false);
				bView.GetComponentInChildren<KoboldAIPossession>(includeInactive: true).gameObject.SetActive(value: true);
			}
			if (bView.TryGetComponent<MoneyHolder>(out var bMoneyHolder))
			{
				bMoneyHolder.SetMoney(moneyB);
			}
			if (bView.TryGetComponent<Kobold>(out var bKobold) && bPlayer != null)
			{
				bPlayer.TagObject = bKobold;
			}
		}
		if (aView != null && bView != null)
		{
			BrainSwapperMachine.bodySwapped?.Invoke(aView.GetComponent<Kobold>(), bView.GetComponent<Kobold>());
		}
	}

	public ReadOnlyCollection<AnimationStation> GetAnimationStations()
	{
		return readOnlyStations;
	}

	
}
