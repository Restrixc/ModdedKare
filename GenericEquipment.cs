using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GenericEquipment : GenericUsable, IPunObservable, IOnPhotonViewOwnerChange, IPhotonViewCallback
{
	public Equipment representedEquipment;

	[SerializeField]
	private Sprite displaySprite;

	protected Kobold tryingToEquip;

	private bool equipOnTouch = false;

	public void TriggerAttachOnTouch(float duration)
	{
		StartCoroutine(AttachOnTouch(duration));
	}

	public override Sprite GetSprite(Kobold k)
	{
		return displaySprite;
	}

	public override void LocalUse(Kobold k)
	{
		Equip(k);
	}

	protected virtual void Equip(Kobold k)
	{
		if (!(k == null))
		{
			if (k.photonView.IsMine && !base.photonView.IsMine && tryingToEquip == null)
			{
				tryingToEquip = k;
				base.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
			}
			if (k.photonView.IsMine && base.photonView.IsMine)
			{
				KoboldInventory inventory = k.GetComponent<KoboldInventory>();
				inventory.PickupEquipment(representedEquipment, base.gameObject);
				PhotonNetwork.Destroy(base.photonView.gameObject);
			}
		}
	}

	private IEnumerator AttachOnTouch(float duration)
	{
		equipOnTouch = true;
		yield return new WaitForSeconds(duration);
		equipOnTouch = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (equipOnTouch && collision != null && !(collision.rigidbody == null))
		{
			Kobold kobold = collision.rigidbody.GetComponentInParent<Kobold>();
			if (kobold != null)
			{
				Equip(kobold);
				equipOnTouch = false;
			}
		}
	}

	public void OnOwnerChange(Player newOwner, Player previousOwner)
	{
		if (newOwner == PhotonNetwork.LocalPlayer && tryingToEquip != null)
		{
			Equip(tryingToEquip);
		}
		tryingToEquip = null;
	}
}
