using System;
using Photon.Pun;

[Serializable]
public class MilkedInstantlyConsumption : ConsumptionDiscreteTrigger
{
	protected override void OnTrigger(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed, ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy)
	{
		KoboldGenes obj = genes;
		float? breastSize = genes.breastSize + requiredCumulativeReagent;
		genes = obj.With(null, null, null, null, null, breastSize);
		k.photonView.RPC("MilkRoutine", RpcTarget.All);
		base.OnTrigger(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
	}
}
