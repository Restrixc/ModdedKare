using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Kobold))]
public class KoboldInventory : MonoBehaviourPun, IPunObservable, ISavable
{
	public delegate void EquipmentChangedEvent(List<Equipment> newEquipment);

	private Dictionary<Equipment, List<GameObject[]>> equipmentDisplays = new Dictionary<Equipment, List<GameObject[]>>();

	private static List<Equipment> staticIncomingEquipment = new List<Equipment>();

	private List<Equipment> equipment = new List<Equipment>();

	public EquipmentChangedEvent equipmentChanged;

	private Kobold kobold;

	public int Count => equipment.Count;

	private void Awake()
	{
		kobold = GetComponent<Kobold>();
	}

	public List<Equipment> GetAllEquipment()
	{
		return new List<Equipment>(equipment);
	}

	public int GetEquipmentInstanceCount(Equipment thing)
	{
		int count = 0;
		for (int i = 0; i < equipment.Count; i++)
		{
			if (equipment[i] == thing)
			{
				count++;
			}
		}
		return count;
	}

	public Equipment GetEquipmentInSlot(Equipment.EquipmentSlot slot)
	{
		foreach (Equipment e in equipment)
		{
			if (e.slot == slot)
			{
				return e;
			}
		}
		return null;
	}

	[PunRPC]
	public void PickupEquipmentRPC(short equipmentID, int groundPrefabID)
	{
		PhotonView view = PhotonNetwork.GetPhotonView(groundPrefabID);
		Equipment equip = EquipmentDatabase.GetEquipment(equipmentID);
		PickupEquipment(equip, (view == null) ? null : view.gameObject);
	}

	public void PickupEquipment(Equipment thing, GameObject groundPrefab)
	{
		GameObject[] displays = thing.OnEquip(kobold, groundPrefab);
		if (!equipmentDisplays.ContainsKey(thing))
		{
			equipmentDisplays[thing] = new List<GameObject[]>();
		}
		equipmentDisplays[thing].Add(displays);
		equipment.Add(thing);
		equipmentChanged?.Invoke(equipment);
	}

	public bool Contains(Equipment thing)
	{
		return equipment.Contains(thing);
	}

	public void RemoveEquipment(Equipment.EquipmentSlot slot, bool dropOnGround)
	{
		Equipment e = GetEquipmentInSlot(slot);
		if (e != null)
		{
			RemoveEquipment(e, dropOnGround);
		}
	}

	public void RemoveEquipment(Equipment thing, bool dropOnGround)
	{
		if (equipment.Contains(thing))
		{
			equipment.Remove(thing);
			thing.OnUnequip(kobold, dropOnGround);
			GameObject[] array = equipmentDisplays[thing][0];
			foreach (GameObject obj in array)
			{
				Object.Destroy(obj);
			}
			equipmentDisplays[thing].RemoveAt(0);
			equipmentChanged?.Invoke(equipment);
		}
	}

	private void ReplaceEquipmentWith(List<Equipment> newEquipment)
	{
		bool same = newEquipment.Count == equipment.Count;
		for (int i = 0; i < equipment.Count && same; i++)
		{
			if (equipment[i] != newEquipment[i])
			{
				same = false;
			}
		}
		if (same)
		{
			return;
		}
		while (equipment.Count != 0)
		{
			RemoveEquipment(equipment[0], dropOnGround: false);
		}
		foreach (Equipment e in newEquipment)
		{
			PickupEquipment(e, null);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext((byte)equipment.Count);
			{
				foreach (Equipment e in equipment)
				{
					stream.SendNext(EquipmentDatabase.GetID(e));
				}
				return;
			}
		}
		byte equipmentCount = (byte)stream.ReceiveNext();
		staticIncomingEquipment.Clear();
		for (int i = 0; i < equipmentCount; i++)
		{
			staticIncomingEquipment.Add(EquipmentDatabase.GetEquipment((short)stream.ReceiveNext()));
		}
		ReplaceEquipmentWith(staticIncomingEquipment);
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(equipment.Count);
		foreach (Equipment e in equipment)
		{
			writer.Write(EquipmentDatabase.GetID(e));
		}
	}

	public void Load(BinaryReader reader)
	{
		int count = reader.ReadInt32();
		staticIncomingEquipment.Clear();
		for (int i = 0; i < count; i++)
		{
			staticIncomingEquipment.Add(EquipmentDatabase.GetEquipment(reader.ReadInt16()));
		}
		ReplaceEquipmentWith(staticIncomingEquipment);
	}
}
