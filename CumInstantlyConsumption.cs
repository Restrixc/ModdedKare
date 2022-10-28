using System;
using Photon.Pun;

[Serializable]
public class CumInstantlyConsumption : ConsumptionDiscreteTrigger
{
	protected override void OnTrigger(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed, ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy)
	{
		KoboldGenes obj = genes;
		float? ballSize = genes.ballSize + requiredCumulativeReagent;
		genes = obj.With(null, null, null, ballSize);
		k.photonView.RPC("Cum", RpcTarget.All);
		base.OnTrigger(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
	}
}
