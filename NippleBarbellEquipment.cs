using UnityEngine;

public class NippleBarbellEquipment : Equipment
{
	public override GameObject[] OnEquip(Kobold k, GameObject groundPrefab)
	{
		k.nippleBarbells.gameObject.SetActive(value: true);
		return base.OnEquip(k, groundPrefab);
	}

	public override GameObject OnUnequip(Kobold k, bool dropOnGround = true)
	{
		k.nippleBarbells.gameObject.SetActive(value: false);
		return base.OnUnequip(k, dropOnGround);
	}
}
