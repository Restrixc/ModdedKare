using UnityEngine;

public class DickEquipment : Equipment
{
	public GameObject dickPrefab;

	public override GameObject[] OnEquip(Kobold k, GameObject groundPrefab)
	{
		base.OnEquip(k, groundPrefab);
		GenericReagentContainer container = ((groundPrefab == null) ? null : groundPrefab.GetComponentInChildren<GenericReagentContainer>());
		GameObject[] stuff = new GameObject[1];
		DickInfo info = (stuff[0] = Object.Instantiate(dickPrefab, k.transform.position, k.transform.rotation)).GetComponentInChildren<DickInfo>();
		if (info == null)
		{
			throw new UnityException("Dick equipment is missing the DickInfo monobehavior. It's needed to be able to equip!");
		}
		info.equipmentInstanceID = GetInstanceID();
		info.AttachTo(k);
		return stuff;
	}

	public override GameObject OnUnequip(Kobold k, bool dropOnGround = true)
	{
		for (int i = 0; i < k.activeDicks.Count; i++)
		{
			if (k.activeDicks[i].info.equipmentInstanceID == GetInstanceID())
			{
				k.activeDicks[i].info.RemoveFrom(k);
				break;
			}
		}
		return base.OnUnequip(k, dropOnGround);
	}
}
