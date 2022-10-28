using System.Collections.Generic;
using UnityEngine;

public class PlantDatabase : MonoBehaviour
{
	private static PlantDatabase instance;

	public List<ScriptablePlant> plants;

	private static PlantDatabase GetInstance()
	{
		if (instance == null)
		{
			instance = Object.FindObjectOfType<PlantDatabase>();
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
	}

	public static ScriptablePlant GetPlant(string name)
	{
		foreach (ScriptablePlant plant in GetInstance().plants)
		{
			if (plant.name == name)
			{
				return plant;
			}
		}
		return null;
	}

	public static ScriptablePlant GetPlant(short id)
	{
		return GetInstance().plants[id];
	}

	public static short GetID(ScriptablePlant plant)
	{
		return (short)GetInstance().plants.IndexOf(plant);
	}
}
