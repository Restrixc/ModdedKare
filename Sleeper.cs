using System.Collections;
using System.IO;
using KoboldKare;
using Photon.Pun;
using UnityEngine;

public class Sleeper : GenericUsable
{
	public GameEventGeneric startSleep;

	public GameEventGeneric sleep;

	public Sprite sleepSprite;

	public override Sprite GetSprite(Kobold k)
	{
		return sleepSprite;
	}

	public override bool CanUse(Kobold k)
	{
		return true;
	}

	public override void LocalUse(Kobold k)
	{
		base.photonView.RPC("RPCUse", RpcTarget.All);
	}

	public override void Use()
	{
		StopAllCoroutines();
		StartCoroutine(SleepRoutine());
	}

	private IEnumerator SleepRoutine()
	{
		startSleep.Raise(null);
		yield return new WaitForSeconds(0.5f);
		sleep.Raise(null);
		DayNightCycle.StaticSleep();
	}

	public override void Save(BinaryWriter writer)
	{
	}

	public override void Load(BinaryReader reader)
	{
	}
}
