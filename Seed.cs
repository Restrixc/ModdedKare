using System.IO;
using Photon.Pun;
using UnityEngine;

public class Seed : GenericUsable, IValuedGood, IPunInstantiateMagicCallback
{
	[SerializeField]
	private float worth = 5f;

	[SerializeField]
	private Sprite displaySprite;

	public float _spacing = 1f;

	public ScriptablePlant plant;

	private Collider[] hitColliders = new Collider[16];

	private KoboldGenes genes;

	private bool waitingOnPlant = false;

	public override Sprite GetSprite(Kobold k)
	{
		return displaySprite;
	}

	public override bool CanUse(Kobold k)
	{
		int hitCount = Physics.OverlapSphereNonAlloc(base.transform.position, _spacing, hitColliders, GameManager.instance.plantHitMask, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < hitCount; i++)
		{
			SoilTile tile = hitColliders[i].GetComponentInParent<SoilTile>();
			if (tile != null && tile.GetPlantable())
			{
				return !waitingOnPlant;
			}
		}
		return false;
	}

	public override void LocalUse(Kobold k)
	{
		int hitCount = Physics.OverlapSphereNonAlloc(base.transform.position, _spacing, hitColliders, GameManager.instance.plantHitMask, QueryTriggerInteraction.Ignore);
		SoilTile bestTile = null;
		float bestTileDistance = float.MaxValue;
		for (int i = 0; i < hitCount; i++)
		{
			SoilTile tile = hitColliders[i].GetComponentInParent<SoilTile>();
			if (tile != null && tile.GetPlantable())
			{
				float distance = Vector3.Distance(tile.transform.position, base.transform.position);
				if (distance < bestTileDistance)
				{
					bestTile = tile;
					bestTileDistance = distance;
				}
			}
		}
		if (bestTile != null && bestTile.GetPlantable())
		{
			if (genes == null)
			{
				genes = new KoboldGenes().Randomize();
			}
			bestTile.photonView.RPC("PlantRPC", RpcTarget.All, base.photonView.ViewID, PlantDatabase.GetID(plant), genes);
		}
	}

	private void Start()
	{
		PlayAreaEnforcer.AddTrackedObject(base.photonView);
	}

	private void OnDestroy()
	{
		PlayAreaEnforcer.RemoveTrackedObject(base.photonView);
	}

	public float GetWorth()
	{
		return worth;
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (info.photonView.InstantiationData != null)
		{
			genes = (KoboldGenes)info.photonView.InstantiationData[0];
		}
		else
		{
			genes = new KoboldGenes().Randomize();
		}
	}

	public override void Save(BinaryWriter writer)
	{
		base.Save(writer);
		if (genes == null)
		{
			genes = new KoboldGenes().Randomize();
		}
		genes.Save(writer);
	}

	public override void Load(BinaryReader reader)
	{
		base.Load(reader);
		KoboldGenes loadedGenes = new KoboldGenes();
		loadedGenes.Load(reader);
		genes = loadedGenes;
	}
}
