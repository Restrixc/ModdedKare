using Photon.Pun;
using UnityEngine;

public class GenericWeapon : MonoBehaviourPun
{
	[SerializeField]
	private Transform weaponBarrelTransform;

	[SerializeField]
	private Vector3 weaponHoldOffset;

	public virtual Transform GetWeaponBarrelTransform()
	{
		return weaponBarrelTransform;
	}

	public virtual Vector3 GetWeaponHoldPosition()
	{
		return weaponHoldOffset;
	}

	public void OnEndFire(Kobold player)
	{
		base.photonView.RPC("OnEndFireRPC", RpcTarget.All, player.photonView.ViewID);
	}

	public void OnFire(Kobold player)
	{
		base.photonView.RPC("OnFireRPC", RpcTarget.All, player.photonView.ViewID);
	}

	[PunRPC]
	protected virtual void OnFireRPC(int playerID)
	{
	}

	[PunRPC]
	protected virtual void OnEndFireRPC(int playerID)
	{
	}
}
