using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

public class Equipment : ScriptableObject
{
	public enum EquipmentSlot
	{
		Misc = -1,
		Crotch,
		Neck,
		Head,
		Nipples,
		Tail,
		Feet,
		Butt,
		Hands
	}

	public enum AttachPoint
	{
		Misc = -1,
		Crotch,
		Neck,
		Head,
		LeftNipple,
		RightNipple,
		TailBase,
		LeftCalf,
		RightCalf,
		LeftHand,
		RightHand,
		LeftForearm,
		RightForearm
	}

	public Sprite sprite;

	public bool hideInFirstPersonWhenWorn;

	public bool canManuallyUnequip = true;

	public EquipmentSlot slot;

	public PhotonGameObjectReference groundPrefab;

	public LocalizedString localizedName;

	public LocalizedString localizedDescription;

	public virtual GameObject[] OnEquip(Kobold k, GameObject groundPrefab)
	{
		if (slot != EquipmentSlot.Misc && groundPrefab != null)
		{
			while (k.GetComponent<KoboldInventory>().GetEquipmentInSlot(slot) != null)
			{
				k.GetComponent<KoboldInventory>().RemoveEquipment(slot, dropOnGround: true);
			}
		}
		return null;
	}

	public virtual GameObject OnUnequip(Kobold k, bool dropOnGround = true)
	{
		if (k.photonView.IsMine && groundPrefab.gameObject != null && dropOnGround)
		{
			return PhotonNetwork.Instantiate(groundPrefab.photonName, k.transform.position, Quaternion.identity, 0);
		}
		return null;
	}

	private void OnValidate()
	{
		groundPrefab.OnValidate();
	}
}
