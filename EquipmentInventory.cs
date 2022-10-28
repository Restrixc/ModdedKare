using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentInventory
{
	public enum EquipmentChangeSource
	{
		Misc,
		Drop,
		Pickup,
		Network
	}

	[Serializable]
	public class EquipmentGameObjectsPair
	{
		public Equipment equipment;

		public GameObject[] gameObjects;

		public EquipmentGameObjectsPair(Equipment e, GameObject[] objects)
		{
			equipment = e;
			gameObjects = objects;
		}
	}

	public delegate void EquipmentChangedHandler(EquipmentInventory inventory, EquipmentChangeSource source);

	public Kobold kobold;

	public List<EquipmentGameObjectsPair> equipment = new List<EquipmentGameObjectsPair>();

	public event EquipmentChangedHandler EquipmentChangedEvent;

	public EquipmentInventory(Kobold k)
	{
		kobold = k;
	}

	public void AddEquipment(Equipment e, EquipmentChangeSource source)
	{
		AddEquipment(e, null, source);
		this.EquipmentChangedEvent?.Invoke(this, source);
	}

	public void AddEquipment(Equipment e, GameObject groundPrefab, EquipmentChangeSource source)
	{
		if (e.slot != Equipment.EquipmentSlot.Misc)
		{
			for (int i = 0; i < equipment.Count; i++)
			{
				if (equipment[i].equipment.slot == e.slot)
				{
					RemoveEquipment(equipment[i].equipment, EquipmentChangeSource.Drop);
				}
			}
		}
		equipment.Add(new EquipmentGameObjectsPair(e, e.OnEquip(kobold, groundPrefab)));
		this.EquipmentChangedEvent?.Invoke(this, source);
	}

	public void RemoveEquipment(int id, EquipmentChangeSource source, bool dropOnGround = true)
	{
		if (id < 0 || id >= equipment.Count)
		{
			return;
		}
		Equipment e = equipment[id].equipment;
		e.OnUnequip(kobold, dropOnGround);
		if (equipment[id].gameObjects != null)
		{
			GameObject[] gameObjects = equipment[id].gameObjects;
			foreach (GameObject g in gameObjects)
			{
				UnityEngine.Object.Destroy(g);
			}
		}
		equipment.RemoveAt(id);
		this.EquipmentChangedEvent?.Invoke(this, source);
	}

	public void Clear(EquipmentChangeSource source, bool dropOnGround = true)
	{
		int i = 0;
		while (i < equipment.Count)
		{
			RemoveEquipment(equipment[i], source, dropOnGround);
		}
		this.EquipmentChangedEvent?.Invoke(this, source);
	}

	public void RemoveEquipment(Equipment e, EquipmentChangeSource source, bool dropOnGround = true, bool removeAllInstances = false)
	{
		for (int i = 0; i < equipment.Count; i++)
		{
			if (!(equipment[i].equipment == e))
			{
				continue;
			}
			e.OnUnequip(kobold, dropOnGround);
			if (equipment[i].gameObjects != null)
			{
				GameObject[] gameObjects = equipment[i].gameObjects;
				foreach (GameObject g in gameObjects)
				{
					UnityEngine.Object.Destroy(g);
				}
			}
			equipment.RemoveAt(i);
			if (!removeAllInstances)
			{
				this.EquipmentChangedEvent?.Invoke(this, source);
				return;
			}
		}
		this.EquipmentChangedEvent?.Invoke(this, source);
	}

	public void RemoveEquipment(EquipmentGameObjectsPair e, EquipmentChangeSource source, bool dropOnGround = true)
	{
		e.equipment.OnUnequip(kobold, dropOnGround);
		if (e.gameObjects != null)
		{
			GameObject[] gameObjects = e.gameObjects;
			foreach (GameObject g in gameObjects)
			{
				UnityEngine.Object.Destroy(g);
			}
		}
		equipment.Remove(e);
		this.EquipmentChangedEvent?.Invoke(this, source);
	}
}
