using System;
using UnityEngine;

[Serializable]
public class LayLargeEggObjective : BreedKoboldObjective
{
	protected override void OnOviposit(GameObject egg)
	{
		if (egg.GetComponent<GenericReagentContainer>().volume > 60f)
		{
			Advance(egg.transform.position);
		}
	}
}
