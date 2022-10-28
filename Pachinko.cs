using System;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

[RequireComponent(typeof(AudioSource))]
public class Pachinko : GenericUsable
{
	[Serializable]
	public class Prize
	{
		public Transform location;

		public UnityEvent prizeGet;

		public ScriptablePurchasable prizeSpawn;

		public VisualEffect spawnVFX;

		public void Spawn()
		{
			GameObject gobj = UnityEngine.Object.Instantiate(prizeSpawn.display, location);
			ScriptablePurchasable.DisableAllButGraphics(gobj);
			spawnVFX = UnityEngine.Object.Instantiate(spawnVFX, location);
		}

		public void Claim()
		{
			if (prizeSpawn.spawnPrefab != null)
			{
				GameObject award = PhotonNetwork.Instantiate(prizeSpawn.spawnPrefab.photonName, location.position, location.rotation, 0);
				if (award.GetComponent<Rigidbody>() != null)
				{
					award.GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * 10f, ForceMode.VelocityChange);
				}
			}
			prizeGet.Invoke();
			spawnVFX.Play();
		}
	}

	[SerializeField]
	[Header("Pachinko!")]
	private MoneyFloater floater;

	[SerializeField]
	private Sprite displaySprite;

	public float playCost = 50f;

	public PhotonGameObjectReference pachinkoBallPrefab;

	public Transform ballSpawnPoint;

	private GameObject activeBall;

	[SerializeField]
	public ConstantForce constantForce;

	private AudioSource audioSrc;

	[Header("Audio Setup")]
	[Space(10f)]
	public AudioClip wonPrize;

	public AudioClip ballReset;

	public AudioClip hitPin;

	public AudioClip gameStart;

	[Space(10f)]
	[Header("Prize Setup")]
	[Space(10f)]
	public List<Prize> prizes = new List<Prize>();

	public override Sprite GetSprite(Kobold k)
	{
		return displaySprite;
	}

	private void Start()
	{
		floater.SetBounds(GetComponent<Renderer>().bounds);
		floater.SetText(playCost.ToString());
		audioSrc = GetComponent<AudioSource>();
		foreach (Prize prize in prizes)
		{
			prize.Spawn();
		}
	}

	public override void LocalUse(Kobold k)
	{
		k.GetComponent<MoneyHolder>().ChargeMoney(playCost);
		base.photonView.RPC("RPCUse", RpcTarget.All);
	}

	public override bool CanUse(Kobold k)
	{
		return (k == null || k.GetComponent<MoneyHolder>().HasMoney(playCost)) && activeBall == null;
	}

	public override void Use()
	{
		StartGame();
	}

	public void StartGame()
	{
		if (base.photonView.IsMine)
		{
			SpawnBall();
			audioSrc.clip = gameStart;
			audioSrc.Play();
		}
	}

	public void ResetGame()
	{
		if (base.photonView.IsMine && activeBall != null)
		{
			PhotonNetwork.Destroy(activeBall);
		}
	}

	private void DestroyBall()
	{
		if (base.photonView.IsMine)
		{
			PhotonNetwork.Destroy(activeBall);
		}
	}

	private void SpawnBall()
	{
		if (base.photonView.IsMine)
		{
			activeBall = PhotonNetwork.Instantiate(pachinkoBallPrefab.photonName, ballSpawnPoint.position, Quaternion.identity, 0, new object[1] { base.photonView.ViewID });
			activeBall.GetComponent<Rigidbody>().velocity = constantForce.force;
		}
	}

	private void DistributePrize(int listIdx)
	{
		if (base.photonView.IsMine)
		{
			audioSrc.clip = wonPrize;
			audioSrc.Play();
			prizes[listIdx].Claim();
		}
	}

	public void ReachedBottom(PachinkoBallZone src)
	{
		DestroyBall();
		DistributePrize(src.zoneID);
	}

	public void BallStuck()
	{
		audioSrc.PlayOneShot(ballReset);
		ResetGame();
	}

	public void HitPin()
	{
		audioSrc.PlayOneShot(hitPin);
	}

	private void OnValidate()
	{
		pachinkoBallPrefab.OnValidate();
	}

	public override void Save(BinaryWriter writer)
	{
	}

	public override void Load(BinaryReader reader)
	{
	}
}
