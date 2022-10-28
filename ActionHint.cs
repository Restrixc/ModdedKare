using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ActionHint : MonoBehaviour
{
	private enum State
	{
		KeyboardMouse,
		Gamepad
	}

	public InputActionReference action;

	public TextMeshProUGUI text;

	public Image image;

	private bool switching = false;

	private State state = State.KeyboardMouse;

	private void SwitchToIndex(State bindingIndex)
	{
		switching = true;
		state = bindingIndex;
		string displayString = action.action.bindings[(int)bindingIndex].path;
		GameManager.StartCoroutineStatic(WaitThenCheckKey(displayString));
	}

	private void OnActionChange(object obj, InputActionChange change)
	{
		if (change == InputActionChange.BoundControlsChanged)
		{
			SwitchToIndex(state);
		}
	}

	private void OnEnable()
	{
		InputSystem.onActionChange -= OnActionChange;
		InputSystem.onActionChange += OnActionChange;
		SwitchToIndex(state);
	}

	private void OnDisable()
	{
		InputSystem.onActionChange -= OnActionChange;
	}

	public void Update()
	{
		if (!switching)
		{
			if (Gamepad.current != null && (Gamepad.current.leftStick.IsActuated(0.25f) || Gamepad.current.rightStick.IsActuated(0.25f) || Gamepad.current.buttonSouth.IsPressed()))
			{
				SwitchToIndex(State.Gamepad);
			}
			else if ((!Keyboard.current.CheckStateIsAtDefaultIgnoringNoise() || Gamepad.current == null) && state == State.Gamepad)
			{
				SwitchToIndex(State.KeyboardMouse);
			}
		}
	}

	private IEnumerator WaitThenCheckKey(string key)
	{
		if (key != "")
		{
			AsyncOperationHandle<UnityEngine.Localization.Locale> otherAsync = LocalizationSettings.SelectedLocaleAsync;
			yield return new WaitUntil(() => otherAsync.IsDone);
			if (otherAsync.IsValid() && otherAsync.Result != null)
			{
				yield return LocalizationSettings.InitializationOperation;
				AsyncOperationHandle<Sprite> asyncOp = LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<Sprite>("InputTexturesTable", key);
				yield return new WaitUntil(() => asyncOp.IsDone);
				if (asyncOp.IsValid() && asyncOp.Result != null)
				{
					text.text = "";
					if (image != null)
					{
						image.color = new Color(1f, 1f, 1f, 1f);
						image.sprite = asyncOp.Result;
						image.preserveAspect = true;
					}
				}
				else
				{
					text.text = key;
					if (image != null)
					{
						image.color = new Color(1f, 1f, 1f, 0f);
					}
				}
			}
		}
		switching = false;
	}
}
