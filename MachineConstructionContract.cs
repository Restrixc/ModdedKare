using UnityEngine;

public class MachineConstructionContract : ConstructionContract
{
	[SerializeField]
	private UsableMachine[] machines;

	public UsableMachine[] GetMachines()
	{
		return machines;
	}

	protected override void SetState(bool purchased)
	{
		base.SetState(purchased);
		UsableMachine[] array = machines;
		foreach (UsableMachine machine in array)
		{
			machine.SetConstructed(purchased);
		}
	}
}
