using UnityEngine;

public class ShowerFaucetButton : UsableMachine
{
	[SerializeField]
	private FluidStream stream;

	[SerializeField]
	private Sprite useSprite;

	[SerializeField]
	private GenericReagentContainer container;

	private bool firing = false;

	public override Sprite GetSprite(Kobold k)
	{
		return useSprite;
	}

	public override bool CanUse(Kobold k)
	{
		return base.CanUse(k) && constructed;
	}

	public override void Use()
	{
		firing = !firing;
		if (firing)
		{
			stream.OnFire(container);
		}
		else
		{
			stream.OnEndFire();
		}
	}
}
