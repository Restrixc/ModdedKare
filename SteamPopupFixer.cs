using System;
using TMPro;
using UnityEngine;
using UnityScriptableSettings;

[RequireComponent(typeof(ScriptableSettingSpawner))]
public class SteamPopupFixer : MonoBehaviour
{
	private ScriptableSettingSpawner spawner;

	private void OnEnable()
	{
		spawner = GetComponent<ScriptableSettingSpawner>();
		ScriptableSettingSpawner scriptableSettingSpawner = spawner;
		scriptableSettingSpawner.doneSpawning = (ScriptableSettingSpawner.FinishSpawningAction)Delegate.Combine(scriptableSettingSpawner.doneSpawning, new ScriptableSettingSpawner.FinishSpawningAction(OnDoneSpawning));
	}

	private void OnDoneSpawning()
	{
		TMP_InputField[] componentsInChildren = GetComponentsInChildren<TMP_InputField>();
		foreach (TMP_InputField inputField in componentsInChildren)
		{
			inputField.gameObject.AddComponent<SteamPopupText>();
		}
	}
}
