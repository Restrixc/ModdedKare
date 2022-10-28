using KoboldKare;
using UnityEngine;

[RequireComponent(typeof(KoboldSeeker))]
[RequireComponent(typeof(GenericReagentContainer))]
public class ReagentHunterAI : Enemy
{
	private GenericReagentContainer GRC;

	public float targetFluidsDesired;

	public GameEventGeneric midnightEvent;

	public GameEventGeneric nightEvent;

	public KoboldSeeker trackingAI;
}
