using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

[Serializable]
public class MaxEnergyGivingConsumption : ConsumptionDiscreteTrigger
{
	[SerializeField]
	private PhotonGameObjectReference floaterInfoPrefab;

	protected override void OnTrigger(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed, ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy)
	{
		genes.maxEnergy = (int)(byte)Mathf.Min(genes.maxEnergy + 1f, 255f);
		GameObject obj = PhotonNetwork.Instantiate(floaterInfoPrefab.photonName, k.transform.position + Vector3.up * 0.5f, Quaternion.identity, 0);
		obj.GetPhotonView().StartCoroutine(DestroyInSeconds(obj));
		base.OnTrigger(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
	}

	private IEnumerator DestroyInSeconds(GameObject obj)
	{
		yield return new WaitForSeconds(5f);
		PhotonNetwork.Destroy(obj);
	}

	public override void OnValidate()
	{
		base.OnValidate();
		floaterInfoPrefab.OnValidate();
	}
}
