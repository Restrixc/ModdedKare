using Photon.Pun;
using UnityEngine;

public class WateringCanWeapon : GenericWeapon
{
	[SerializeField]
	private Animator weaponAnimator;

	[SerializeField]
	private FluidStream stream;

	[SerializeField]
	private GenericReagentContainer container;

	private static readonly int Fire = Animator.StringToHash("Fire");

	[PunRPC]
	protected override void OnFireRPC(int viewID)
	{
		base.OnFireRPC(viewID);
		weaponAnimator.SetBool(Fire, value: true);
		stream.OnFire(container);
	}

	[PunRPC]
	protected override void OnEndFireRPC(int viewID)
	{
		base.OnEndFireRPC(viewID);
		weaponAnimator.SetBool(Fire, value: false);
		stream.OnEndFire();
	}
}
