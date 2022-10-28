using System.Collections.Generic;
using UnityEngine;

public class EquipmentDatabase : MonoBehaviour
{
	private static EquipmentDatabase instance;

	private Dictionary<string, Equipment> equipmentDictionary = new Dictionary<string, Equipment>();

	public List<Equipment> equipments;

	private static EquipmentDatabase GetInstance()
	{
		if (instance == null)
		{
			instance = Object.FindObjectOfType<EquipmentDatabase>();
		}
		return instance;
	}

	public void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(this);
		}
		else
		{
			instance = this;
		}
		foreach (Equipment equipment in equipments)
		{
			equipmentDictionary.Add(equipment.name, equipment);
		}
	}

	public static Equipment GetEquipment(string name)
	{
		if (GetInstance().equipmentDictionary.ContainsKey(name))
		{
			return GetInstance().equipmentDictionary[name];
		}
		throw new UnityException("Failed to find equipment with name " + name);
	}

	public static Equipment GetEquipment(short id)
	{
		return GetInstance().equipments[id];
	}

	public static short GetID(Equipment equipment)
	{
		return (short)GetInstance().equipments.IndexOf(equipment);
	}

	public static List<Equipment> GetEquipments()
	{
		return GetInstance().equipments;
	}
}
