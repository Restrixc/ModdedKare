using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using PenetrationTech;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class OvipositionSpot : GenericUsable, IAnimationStationSet
{
	public delegate void OvipositionAction(GameObject egg);

	[SerializeField]
	private Sprite useSprite;

	[SerializeField]
	private AnimationStation station;

	[SerializeField]
	private PhotonGameObjectReference eggPrefab;

	private ScriptableReagent egg;

	private ReadOnlyCollection<AnimationStation> readOnlyStations;

	public static event OvipositionAction oviposition;

	public override Sprite GetSprite(Kobold k)
	{
		return useSprite;
	}

	public override bool CanUse(Kobold k)
	{
		return k.bellyContainer.GetVolumeOf(egg) > 5f && station.info.user == null && k.GetEnergy() >= 1f;
	}

	public override void LocalUse(Kobold k)
	{
		base.LocalUse(k);
		k.photonView.RPC("BeginAnimationRPC", RpcTarget.All, base.photonView.ViewID, 0);
	}

	public override void Use()
	{
		StartCoroutine(EggLayingRoutine());
	}

	private void Start()
	{
		List<AnimationStation> stations = new List<AnimationStation>();
		stations.Add(station);
		readOnlyStations = stations.AsReadOnly();
		egg = ReagentDatabase.GetReagent("Egg");
	}

	[PunRPC]
	private void EggLayed(int viewID)
	{
		PhotonView view = PhotonNetwork.GetPhotonView(viewID);
		if (view != null)
		{
			OvipositionSpot.oviposition?.Invoke(view.gameObject);
		}
	}

	private IEnumerator EggLayingRoutine()
	{
		yield return new WaitForSeconds(6f);
        
		Kobold i = station.info.user;
		if (i == null || !i.photonView.IsMine || !i.TryConsumeEnergy(1))
		{
			yield break;
		}
		Penetrable targetPenetrable = null;
		foreach (Kobold.PenetrableSet penetrable in i.penetratables)
		{
			if (penetrable.isFemaleExclusiveAnatomy && penetrable.penetratable.isActiveAndEnabled)
			{
				targetPenetrable = penetrable.penetratable;
			}
		}
		if (targetPenetrable == null)
		{
			foreach (Kobold.PenetrableSet penetrable2 in i.penetratables)
			{
				if (penetrable2.penetratable.isActiveAndEnabled && penetrable2.isFemaleExclusiveAnatomy && penetrable2.canLayEgg)
				{
					targetPenetrable = penetrable2.penetratable;
				}
			}
		}
		if (targetPenetrable == null)
		{
			foreach (Kobold.PenetrableSet penetrable3 in i.penetratables)
			{
				if (penetrable3.penetratable.isActiveAndEnabled && penetrable3.canLayEgg)
				{
					targetPenetrable = penetrable3.penetratable;
				}
			}
		}
		if (true)
		{
			//Used to fuck with live birth
			GameObject spawnKoblood = PhotonNetwork.InstantiateRoomObject("Kobold", targetPenetrable.transform.position + targetPenetrable.transform.forward, Quaternion.identity, 0);
			Kobold k = spawnKoblood.GetComponent<Kobold>();
			KoboldGenes mixedGenes = KoboldGenes.Mix(i.GetComponent<Kobold>().GetGenes(), i.bellyContainer.GetGenes());
			k.SetGenes(mixedGenes);
			targetPenetrable.GetComponent<Kobold>().bellyContainer.Spill(targetPenetrable.GetComponent<Kobold>().bellyContainer.volume);
		}
        else
        {
			float eggVolume = i.bellyContainer.GetVolumeOf(egg);
			i.bellyContainer.OverrideReagent(egg, 0f);
			if (targetPenetrable == null)
			{
				Debug.LogError("Kobold without a hole tried to make an egg!");
				yield break;
			}
			CatmullSpline path = targetPenetrable.GetSplinePath();
			KoboldGenes mixedGenes = KoboldGenes.Mix(i.GetComponent<Kobold>().GetGenes(), i.bellyContainer.GetGenes());
			Penetrator d = PhotonNetwork.Instantiate(eggPrefab.photonName, path.GetPositionFromT(0f), Quaternion.LookRotation(path.GetVelocityFromT(0f).normalized, Vector3.up), 0, new object[1] { mixedGenes }).GetComponentInChildren<Penetrator>();
			if (!(d == null))
			{
				ReagentContents eggContents = new ReagentContents();
				eggContents.AddMix(ReagentDatabase.GetReagent("ScrambledEgg").GetReagent(eggVolume));
				d.gameObject.GetPhotonView().RPC("ForceMixRPC", RpcTarget.All, eggContents, i.photonView.ViewID);
				base.photonView.RPC("EggLayed", RpcTarget.All, d.GetComponentInParent<PhotonView>().ViewID);
				Rigidbody body = d.GetComponentInChildren<Rigidbody>();
				body.isKinematic = true;
				d.Penetrate(targetPenetrable);
				float pushAmount = 0f;
				while (pushAmount < d.GetWorldLength())
				{
					CatmullSpline p = targetPenetrable.GetSplinePath();
					Vector3 position = p.GetPositionFromT(0f);
					Vector3 tangent = p.GetVelocityFromT(0f).normalized;
					pushAmount += Time.deltaTime * 0.15f;
					body.transform.position = position - tangent * pushAmount;
					yield return null;
				}
				body.isKinematic = false;
			}
		}
		
	}

	public ReadOnlyCollection<AnimationStation> GetAnimationStations()
	{
		return readOnlyStations;
	}

	private void OnValidate()
	{
		eggPrefab.OnValidate();
	}

	
}
