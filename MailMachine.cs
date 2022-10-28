using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using KoboldKare;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.VFX;
using Vilar.AnimationStation;

public class MailMachine : SuckingMachine, IAnimationStationSet
{
	[SerializeField]
	private Sprite mailSprite;

	[SerializeField]
	private List<AnimationStation> stations;

	[SerializeField]
	private Animator mailAnimator;

	[SerializeField]
	private PhotonGameObjectReference moneyPile;

	[SerializeField]
	private AudioPack sellPack;

	private AudioSource sellSource;

	[SerializeField]
	private VisualEffect poof;

	[SerializeField]
	private GameEventPhotonView soldGameEvent;

	[SerializeField]
	private Transform payoutLocation;

	private ReadOnlyCollection<AnimationStation> readOnlyStations;

	private WaitForSeconds wait;

	private List<AnimationStation> availableStations;

	protected override void Awake()
	{
		base.Awake();
		readOnlyStations = stations.AsReadOnly();
		wait = new WaitForSeconds(2f);
		availableStations = new List<AnimationStation>();
		if (sellSource == null)
		{
			sellSource = base.gameObject.AddComponent<AudioSource>();
			sellSource.playOnAwake = false;
			sellSource.maxDistance = 10f;
			sellSource.minDistance = 0.2f;
			sellSource.rolloffMode = AudioRolloffMode.Linear;
			sellSource.spatialBlend = 1f;
			sellSource.loop = false;
		}
	}

	public override Sprite GetSprite(Kobold k)
	{
		return mailSprite;
	}

	public override bool CanUse(Kobold k)
	{
		if (!constructed)
		{
			return false;
		}
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if ((Kobold)player.TagObject == k)
			{
				return false;
			}
		}
		foreach (AnimationStation station in stations)
		{
			if (station.info.user == null)
			{
				return true;
			}
		}
		return true;
	}

	public override void LocalUse(Kobold k)
	{
		availableStations.Clear();
		foreach (AnimationStation station in stations)
		{
			if (station.info.user == null)
			{
				availableStations.Add(station);
			}
		}
		if (availableStations.Count > 0)
		{
			int randomStation = Random.Range(0, availableStations.Count);
			k.photonView.RPC("BeginAnimationRPC", RpcTarget.All, base.photonView.ViewID, stations.IndexOf(availableStations[randomStation]));
			base.LocalUse(k);
		}
	}

	public override void Use()
	{
		StopAllCoroutines();
		StartCoroutine(WaitThenVoreKobold());
	}

	private IEnumerator WaitThenVoreKobold()
	{
		yield return wait;
		mailAnimator.SetTrigger("Mail");
		yield return wait;
		foreach (AnimationStation station in stations)
		{
			if (!(station.info.user == null) && station.info.user.photonView.IsMine)
			{
				base.photonView.RPC("OnSwallowed", RpcTarget.All, station.info.user.photonView.ViewID);
			}
		}
	}

	private float FloorNearestPower(float baseNum, float target)
	{
		float f;
		for (f = baseNum; f <= target; f *= baseNum)
		{
		}
		return f / baseNum;
	}

	[PunRPC]
	protected override IEnumerator OnSwallowed(int viewID)
	{
		PhotonView view = PhotonNetwork.GetPhotonView(viewID);
		float totalWorth = 0f;
		IValuedGood[] componentsInChildren = view.GetComponentsInChildren<IValuedGood>();
		foreach (IValuedGood v in componentsInChildren)
		{
			if (v != null)
			{
				float add = Mathf.Min(v.GetWorth(), 1953125f);
				totalWorth = Mathf.Min(totalWorth + add, 1953125f);
			}
		}
		soldGameEvent.Raise(view);
		poof.SendEvent("TriggerPoof");
		if (view.GetComponent<Kobold>() != null)
		{
			mailAnimator.SetTrigger("Mail");
		}
		sellPack.PlayOneShot(sellSource);
		if (view.IsMine)
		{
			yield return new WaitForSeconds(0.1f);
			if (view != null)
			{
				PhotonNetwork.Destroy(view.gameObject);
			}
			totalWorth = Mathf.Min(totalWorth, 1953125f);
			int i = 0;
			while (totalWorth > 0f)
			{
				float currentPayout = FloorNearestPower(5f, totalWorth);
				totalWorth -= currentPayout;
				totalWorth = Mathf.Max(totalWorth, 0f);
				float up = Mathf.Floor((float)i / 4f) * 0.2f;
				PhotonNetwork.Instantiate(moneyPile.photonName, payoutLocation.position + payoutLocation.forward * ((float)(i % 4) * 0.25f) + payoutLocation.up * up, payoutLocation.rotation, 0, new object[1] { currentPayout });
				i++;
			}
		}
	}

	public ReadOnlyCollection<AnimationStation> GetAnimationStations()
	{
		return readOnlyStations;
	}

	private void OnValidate()
	{
		moneyPile.OnValidate();
	}

	
}
