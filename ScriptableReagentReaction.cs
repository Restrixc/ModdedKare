using System;
using UnityEngine;


public class ScriptableReagentReaction : ScriptableObject
{
	[Serializable]
	public class Reactant
	{
		public ScriptableReagent reactant;

		[Range(0.01f, 10f)]
		public float coefficient;
	}

	[SerializeReferenceButton]
	[SerializeReference]
	private ReagentReaction[] reactions;

	public Reactant[] reactants;

	public Reactant[] products;

	public void DoReaction(GenericReagentContainer container)
	{
		Reactant minReactant = reactants[0];
		float minReactantVolumeRatio = container.GetVolumeOf(minReactant.reactant) / Mathf.Max(minReactant.coefficient, 0.001f);
		Reactant[] array = reactants;
		foreach (Reactant reactant in array)
		{
			if (reactant.coefficient != 0f)
			{
				float reactantVolumeRatio = container.GetVolumeOf(reactant.reactant) / reactant.coefficient;
				if (reactantVolumeRatio < minReactantVolumeRatio)
				{
					minReactant = reactant;
					minReactantVolumeRatio = reactantVolumeRatio;
				}
			}
		}
		if (!Mathf.Approximately(minReactantVolumeRatio, 0f))
		{
			Reactant[] array2 = reactants;
			foreach (Reactant reactant2 in array2)
			{
				float reactantVolume = container.GetVolumeOf(reactant2.reactant);
				container.OverrideReagent(reactant2.reactant, reactantVolume - minReactantVolumeRatio * reactant2.coefficient);
			}
			Reactant[] array3 = products;
			foreach (Reactant product in array3)
			{
				container.GetContents().AddMix(ReagentDatabase.GetID(product.reactant), minReactantVolumeRatio * product.coefficient, container);
			}
			ReagentReaction[] array4 = reactions;
			foreach (ReagentReaction reaction in array4)
			{
				reaction.React(container);
			}
		}
	}
}
