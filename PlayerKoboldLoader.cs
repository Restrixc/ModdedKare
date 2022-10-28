using UnityEngine;
using UnityScriptableSettings;

public class PlayerKoboldLoader : MonoBehaviour
{
	private static readonly string[] settingNames = new string[8] { "Hue", "Brightness", "Saturation", "BoobSize", "KoboldSize", "DickSize", "DickThickness", "BallSize" };

	public Kobold targetKobold;

	private void Start()
	{
		string[] array = settingNames;
		foreach (string settingName in array)
		{
			Setting option = SettingsManager.GetSetting(settingName);
			if (option is SettingFloat optionFloat)
			{
				optionFloat.changed -= OnValueChange;
				optionFloat.changed += OnValueChange;
				continue;
			}
			throw new UnityException("Setting " + settingName + " is not a SettingFloat");
		}
		Setting dickOption = SettingsManager.GetSetting("Dick");
		if (dickOption is SettingInt optionInt)
		{
			optionInt.changed -= OnValueChange;
			optionInt.changed += OnValueChange;
			targetKobold.SetGenes(GetPlayerGenes());
			return;
		}
		throw new UnityException("Setting Dick is not a SettingInt");
	}

	private void OnDestroy()
	{
		string[] array = settingNames;
		foreach (string settingName in array)
		{
			Setting option = SettingsManager.GetSetting(settingName);
			if (option is SettingFloat optionFloat)
			{
				optionFloat.changed -= OnValueChange;
			}
		}
		Setting dickOption = SettingsManager.GetSetting("Dick");
		if (dickOption is SettingInt optionInt)
		{
			optionInt.changed -= OnValueChange;
		}
	}

	private static KoboldGenes ProcessOption(KoboldGenes genes, SettingInt setting)
	{
		string text = setting.name;
		string text2 = text;
		if (text2 == "Dick")
		{
			genes.dickEquip = (byte)(((float)setting.GetValue() == 0f) ? byte.MaxValue : 0);
		}
		return genes;
	}

	private static KoboldGenes ProcessOption(KoboldGenes genes, SettingFloat setting)
	{
		switch (setting.name)
		{
		case "Hue":
			genes.hue = (byte)Mathf.RoundToInt(setting.GetValue() * 255f);
			break;
		case "Brightness":
			genes.brightness = (byte)Mathf.RoundToInt(setting.GetValue() * 255f);
			break;
		case "Saturation":
			genes.saturation = (byte)Mathf.RoundToInt(setting.GetValue() * 255f);
			break;
		case "DickSize":
			genes.dickSize = Mathf.Lerp(0f, 10f, setting.GetValue());
			break;
		case "BallSize":
			genes.ballSize = Mathf.Lerp(5f, 10f, setting.GetValue());
			break;
		case "DickThickness":
			genes.dickThickness = Mathf.Lerp(0.3f, 0.7f, setting.GetValue());
			break;
		case "BoobSize":
			genes.breastSize = setting.GetValue() * 30f;
			break;
		case "KoboldSize":
			genes.baseSize = setting.GetValue() * 20f;
			break;
		}
		return genes;
	}

	public static KoboldGenes GetPlayerGenes()
	{
		KoboldGenes genes = new KoboldGenes();
		string[] array = settingNames;
		foreach (string setting in array)
		{
			genes = ProcessOption(genes, SettingsManager.GetSetting(setting) as SettingFloat);
		}
		return ProcessOption(genes, SettingsManager.GetSetting("Dick") as SettingInt);
	}

	private void OnValueChange(int newValue)
	{
		targetKobold.SetGenes(GetPlayerGenes());
	}

	private void OnValueChange(float newValue)
	{
		targetKobold.SetGenes(GetPlayerGenes());
	}
}
