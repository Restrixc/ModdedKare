using System;
using System.IO;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class InputOptions : MonoBehaviour
{
	[SerializeField]
	private InputActionAsset controls;

	private static InputOptions instance;

	public static void SaveControls()
	{
		instance.Save();
	}

	private void Save()
	{
		string savePath = Application.persistentDataPath + "/inputBindings.json";
		FileStream file = File.Create(savePath);
		JSONNode i = JSON.Parse("{}");
		foreach (InputActionMap map in controls.actionMaps)
		{
			foreach (InputBinding binding in map.bindings)
			{
				if (!string.IsNullOrEmpty(binding.overridePath))
				{
					i[binding.id.ToString()] = binding.overridePath;
				}
			}
		}
		file.Write(Encoding.UTF8.GetBytes(i.ToString(2)), 0, i.ToString(2).Length);
		file.Close();
		Debug.Log("Saved input bindings to " + savePath);
	}

	private void Load()
	{
		try
		{
			string savePath = Application.persistentDataPath + "/inputBindings.json";
			FileStream file = File.Open(savePath, FileMode.Open);
			byte[] b = new byte[file.Length];
			file.Read(b, 0, (int)file.Length);
			file.Close();
			string data = Encoding.UTF8.GetString(b);
			JSONNode j = JSON.Parse(data);
			foreach (InputActionMap map in controls.actionMaps)
			{
				ReadOnlyArray<InputBinding> bindings = map.bindings;
				for (int i = 0; i < bindings.Count; i++)
				{
					if (j.HasKey(bindings[i].id.ToString()))
					{
						map.ApplyBindingOverride(i, new InputBinding
						{
							overridePath = j[bindings[i].id.ToString()]
						});
					}
				}
			}
		}
		catch (Exception e)
		{
			if (!(e is FileNotFoundException))
			{
				Debug.LogException(e);
			}
		}
	}

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		Load();
	}
}
