using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Samples.RebindUI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public class RebindSpawner : MonoBehaviour
{
	[Serializable]
	public class RebindActionNamePair
	{
		public InputActionReference control;

		public LocalizedString controlName;
	}

	public List<RebindActionNamePair> rebindActionNamePairs = new List<RebindActionNamePair>();

	public GameObject rebindPrefab;

	public GameObject rebindUI;

	public TextMeshProUGUI rebindUIText;

	private Dictionary<RebindActionNamePair, GameObject> controlUI = new Dictionary<RebindActionNamePair, GameObject>();

	private IEnumerator StartRoutine()
	{
		yield return LocalizationSettings.InitializationOperation;
		yield return new WaitForSecondsRealtime(0.25f);
		controlUI.Clear();
		foreach (RebindActionNamePair r in rebindActionNamePairs)
		{
			GameObject i = UnityEngine.Object.Instantiate(rebindPrefab);
			i.transform.SetParent(base.transform, worldPositionStays: false);
			i.transform.localScale = Vector3.one;
			i.GetComponentInChildren<TextMeshProUGUI>().text = r.controlName.GetLocalizedString();
			controlUI[r] = i;
			int id = 0;
			RebindActionUI[] componentsInChildren = i.GetComponentsInChildren<RebindActionUI>();
			foreach (RebindActionUI rebinder in componentsInChildren)
			{
				rebinder.actionReference = r.control;
				rebinder.bindingId = r.control.action.bindings[id++].id.ToString();
				rebinder.rebindOverlay = rebindUI;
				rebinder.rebindPrompt = rebindUIText;
				rebinder.UpdateBindingDisplay();
			}
		}
		LocalizationSettings.SelectedLocaleChanged += StringChanged;
		StringChanged(null);
	}

	private void Start()
	{
		GameManager.instance.StartCoroutine(StartRoutine());
	}

	public void RefreshDisplay()
	{
		foreach (KeyValuePair<RebindActionNamePair, GameObject> item in controlUI)
		{
			RebindActionUI[] componentsInChildren = item.Value.GetComponentsInChildren<RebindActionUI>();
			foreach (RebindActionUI rebinder in componentsInChildren)
			{
				rebinder.UpdateBindingDisplay();
			}
		}
	}

	private IEnumerator ChangeStrings()
	{
		AsyncOperationHandle<UnityEngine.Localization.Locale> otherAsync = LocalizationSettings.SelectedLocaleAsync;
		yield return new WaitUntil(() => otherAsync.IsDone);
		yield return new WaitForSecondsRealtime(1f);
		if (!(otherAsync.Result != null))
		{
			yield break;
		}
		foreach (RebindActionNamePair r in rebindActionNamePairs)
		{
			controlUI[r].GetComponentInChildren<TextMeshProUGUI>().text = r.controlName.GetLocalizedString();
		}
	}

	public void StringChanged(UnityEngine.Localization.Locale locale)
	{
		StopAllCoroutines();
		GameManager.instance.StartCoroutine(ChangeStrings());
	}
}
