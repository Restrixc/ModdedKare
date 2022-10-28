using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;
using Vilar.AnimationStation;

public class Toilet : GenericUsable, IAnimationStationSet
{
	[SerializeField]
	private Sprite useSprite;

	[SerializeField]
	private AnimationStation station;

	[SerializeField]
	private AudioPack sparkles;

	[SerializeField]
	private AudioPack flush;

	[SerializeField]
	private VisualEffect effect;

	private ReadOnlyCollection<AnimationStation> readOnlyStations;

	private AudioSource source;

	public override Sprite GetSprite(Kobold k)
	{
		return useSprite;
	}

	public override bool CanUse(Kobold k)
	{
		return station.info.user == null;
	}

	public override void LocalUse(Kobold k)
	{
		base.LocalUse(k);
		k.photonView.RPC("BeginAnimationRPC", RpcTarget.All, base.photonView.ViewID, 0);
	}

	public override void Use()
	{
		base.Use();
		StopAllCoroutines();
		StartCoroutine(ToiletRoutine());
	}

	private IEnumerator ToiletRoutine()
	{
		yield return new WaitForSeconds(4f);
		source.enabled = true;
		sparkles.Play(source);
		effect.gameObject.SetActive(value: true);
		yield return new WaitForSeconds(6f);
		source.Pause();
		flush.PlayOneShot(source);
		Kobold i = station.info.user;
		if (i != null)
		{
			i.bellyContainer.Spill(i.bellyContainer.volume);
			i.photonView.RPC("StopAnimationRPC", RpcTarget.All);
		}
		effect.gameObject.SetActive(value: false);
		yield return new WaitForSeconds(4f);
		source.Stop();
		source.enabled = false;
	}

	private void Start()
	{
		List<AnimationStation> stations = new List<AnimationStation>();
		stations.Add(station);
		readOnlyStations = stations.AsReadOnly();
		if (source == null)
		{
			source = base.gameObject.AddComponent<AudioSource>();
			source.playOnAwake = false;
			source.maxDistance = 10f;
			source.minDistance = 0.2f;
			source.rolloffMode = AudioRolloffMode.Linear;
			source.spatialBlend = 1f;
			source.loop = true;
		}
		source.enabled = false;
	}

	public ReadOnlyCollection<AnimationStation> GetAnimationStations()
	{
		return readOnlyStations;
	}

	
}
