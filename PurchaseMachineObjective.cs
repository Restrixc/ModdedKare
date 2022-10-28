using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class PurchaseMachineObjective : ObjectiveWithSpaceBeam
{
	[SerializeField]
	private LocalizedString description;

	[SerializeField]
	private UsableMachine targetMachine;

	public override void Register()
	{
		base.Register();
		ConstructionContract.purchasedEvent += OnContractSold;
	}

	public override void Unregister()
	{
		base.Unregister();
		ConstructionContract.purchasedEvent -= OnContractSold;
	}

	protected override void Advance(Vector3 position)
	{
		base.Advance(position);
		TriggerComplete();
	}

	private void OnContractSold(ConstructionContract contract)
	{
		if (contract is MachineConstructionContract machineContract && machineContract.GetMachines().Contains(targetMachine))
		{
			Advance(targetMachine.transform.position);
		}
	}

	public override string GetTextBody()
	{
		return description.GetLocalizedString();
	}
}
