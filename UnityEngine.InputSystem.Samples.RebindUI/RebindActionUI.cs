using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace UnityEngine.InputSystem.Samples.RebindUI;

public class RebindActionUI : MonoBehaviour
{
	[Serializable]
	public class UpdateBindingUIEvent : UnityEvent<RebindActionUI, string, string, string>
	{
	}

	[Serializable]
	public class InteractiveRebindEvent : UnityEvent<RebindActionUI, InputActionRebindingExtensions.RebindingOperation>
	{
	}

	[SerializeField]
	[Tooltip("Reference to action that is to be rebound from the UI.")]
	private InputActionReference m_Action;

	[SerializeField]
	private string m_BindingId;

	[SerializeField]
	private InputBinding.DisplayStringOptions m_DisplayStringOptions;

	[SerializeField]
	[Tooltip("Text label that will receive the name of the action. Optional. Set to None to have the rebind UI not show a label for the action.")]
	private TextMeshProUGUI m_ActionLabel;

	[Tooltip("Text label that will receive the current, formatted binding string.")]
	[SerializeField]
	private TextMeshProUGUI m_BindingText;

	[Tooltip("If there's an image in our database, we'll use that instead")]
	[SerializeField]
	private Image m_BindingImage;

	[Tooltip("Optional UI that will be shown while a rebind is in progress.")]
	[SerializeField]
	private GameObject m_RebindOverlay;

	[Tooltip("Optional text label that will be updated with prompt for user input.")]
	[SerializeField]
	private TextMeshProUGUI m_RebindText;

	[Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying bindings in custom ways, e.g. using images instead of text.")]
	[SerializeField]
	private UpdateBindingUIEvent m_UpdateBindingUIEvent;

	[Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, to implement custom UI behavior while a rebind is in progress. It can also be used to further customize the rebind.")]
	[SerializeField]
	private InteractiveRebindEvent m_RebindStartEvent;

	[SerializeField]
	[Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
	private InteractiveRebindEvent m_RebindStopEvent;

	private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;

	private static List<RebindActionUI> s_RebindActionUIs;

	public InputActionReference actionReference
	{
		get
		{
			return m_Action;
		}
		set
		{
			m_Action = value;
			UpdateActionLabel();
			UpdateBindingDisplay();
		}
	}

	public string bindingId
	{
		get
		{
			return m_BindingId;
		}
		set
		{
			m_BindingId = value;
			UpdateBindingDisplay();
		}
	}

	public InputBinding.DisplayStringOptions displayStringOptions
	{
		get
		{
			return m_DisplayStringOptions;
		}
		set
		{
			m_DisplayStringOptions = value;
			UpdateBindingDisplay();
		}
	}

	public TextMeshProUGUI actionLabel
	{
		get
		{
			return m_ActionLabel;
		}
		set
		{
			m_ActionLabel = value;
			UpdateActionLabel();
		}
	}

	public TextMeshProUGUI bindingText
	{
		get
		{
			return m_BindingText;
		}
		set
		{
			m_BindingText = value;
			UpdateBindingDisplay();
		}
	}

	public Image bindingImage
	{
		get
		{
			return m_BindingImage;
		}
		set
		{
			m_BindingImage = value;
			UpdateBindingDisplay();
		}
	}

	public TextMeshProUGUI rebindPrompt
	{
		get
		{
			return m_RebindText;
		}
		set
		{
			m_RebindText = value;
		}
	}

	public GameObject rebindOverlay
	{
		get
		{
			return m_RebindOverlay;
		}
		set
		{
			m_RebindOverlay = value;
		}
	}

	public UpdateBindingUIEvent updateBindingUIEvent
	{
		get
		{
			if (m_UpdateBindingUIEvent == null)
			{
				m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
			}
			return m_UpdateBindingUIEvent;
		}
	}

	public InteractiveRebindEvent startRebindEvent
	{
		get
		{
			if (m_RebindStartEvent == null)
			{
				m_RebindStartEvent = new InteractiveRebindEvent();
			}
			return m_RebindStartEvent;
		}
	}

	public InteractiveRebindEvent stopRebindEvent
	{
		get
		{
			if (m_RebindStopEvent == null)
			{
				m_RebindStopEvent = new InteractiveRebindEvent();
			}
			return m_RebindStopEvent;
		}
	}

	public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

	public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
	{
		bindingIndex = -1;
		action = m_Action?.action;
		if (action == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(m_BindingId))
		{
			return false;
		}
		Guid bindingId = new Guid(m_BindingId);
		bindingIndex = action.bindings.IndexOf((InputBinding x) => x.id == bindingId);
		if (bindingIndex == -1)
		{
			Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
			return false;
		}
		return true;
	}

	public void UpdateBindingDisplay()
	{
		if (m_BindingText != null)
		{
			m_BindingText.text = string.Empty;
		}
		string displayString = string.Empty;
		string deviceLayoutName = null;
		string controlPath = null;
		InputAction action = m_Action?.action;
		if (action != null)
		{
			int bindingIndex = action.bindings.IndexOf((InputBinding x) => x.id.ToString() == m_BindingId);
			if (bindingIndex != -1)
			{
				displayString = action.bindings[bindingIndex].effectivePath;
			}
		}
		if (GameManager.instance != null)
		{
			GameManager.instance.StartCoroutine(WaitThenCheckKey(displayString));
		}
		m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
	}

	private IEnumerator WaitThenCheckKey(string key)
	{
		if (key != "")
		{
			yield return LocalizationSettings.InitializationOperation;
			bindingImage.sprite = null;
			bindingImage.color = new Color(1f, 1f, 1f, 0f);
			m_BindingText.text = key;
			AsyncOperationHandle<Sprite> localizedAssetAsync = LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<Sprite>("InputTexturesTable", key);
			localizedAssetAsync.Completed += delegate(AsyncOperationHandle<Sprite> asyncOp)
			{
				m_BindingText.text = "";
				bindingImage.color = new Color(1f, 1f, 1f, 1f);
				bindingImage.sprite = asyncOp.Result;
				bindingImage.preserveAspect = true;
			};
		}
	}

	public void ResetToDefault()
	{
		if (!ResolveActionAndBinding(out var action, out var bindingIndex))
		{
			return;
		}
		if (action.bindings[bindingIndex].isComposite)
		{
			for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
			{
				action.RemoveBindingOverride(i);
			}
		}
		else
		{
			action.RemoveBindingOverride(bindingIndex);
		}
		UpdateBindingDisplay();
	}

	public void Start()
	{
		UpdateBindingDisplay();
	}

	public void StartInteractiveRebind()
	{
		if (!ResolveActionAndBinding(out var action, out var bindingIndex))
		{
			return;
		}
		if (action.bindings[bindingIndex].isComposite)
		{
			int firstPartIndex = bindingIndex + 1;
			if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
			{
				PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
			}
		}
		else
		{
			PerformInteractiveRebind(action, bindingIndex);
		}
	}

	private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
	{
		GetComponent<Button>().interactable = false;
		m_RebindOperation?.Cancel();
		action.Disable();
		m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex).OnCancel(delegate(InputActionRebindingExtensions.RebindingOperation operation)
		{
			GetComponent<Button>().interactable = true;
			m_RebindStopEvent?.Invoke(this, operation);
			m_RebindOverlay?.SetActive(value: false);
			action.Enable();
			UpdateBindingDisplay();
			CleanUp();
		}).OnComplete(delegate(InputActionRebindingExtensions.RebindingOperation operation)
		{
			GetComponent<Button>().interactable = true;
			m_RebindOverlay?.SetActive(value: false);
			m_RebindStopEvent?.Invoke(this, operation);
			action.Enable();
			UpdateBindingDisplay();
			CleanUp();
			if (allCompositeParts)
			{
				int num = bindingIndex + 1;
				if (num < action.bindings.Count && action.bindings[num].isPartOfComposite)
				{
					PerformInteractiveRebind(action, num, allCompositeParts: true);
				}
			}
		});
		string partName = null;
		if (action.bindings[bindingIndex].isPartOfComposite)
		{
			partName = "Binding '" + action.bindings[bindingIndex].name + "'. ";
		}
		m_RebindOverlay?.SetActive(value: true);
		if (m_RebindText != null)
		{
			string text = ((!string.IsNullOrEmpty(m_RebindOperation.expectedControlType)) ? (partName + "Waiting for " + m_RebindOperation.expectedControlType + " input...") : (partName + "Waiting for input..."));
			m_RebindText.text = text;
		}
		if (m_RebindOverlay == null && m_RebindText == null && m_RebindStartEvent == null && m_BindingText != null)
		{
			m_BindingText.text = "<Waiting...>";
		}
		m_RebindStartEvent?.Invoke(this, m_RebindOperation);
		m_RebindOperation.Start();
		void CleanUp()
		{
			m_RebindOperation?.Dispose();
			m_RebindOperation = null;
		}
	}

	protected void OnEnable()
	{
		if (s_RebindActionUIs == null)
		{
			s_RebindActionUIs = new List<RebindActionUI>();
		}
		s_RebindActionUIs.Add(this);
		if (s_RebindActionUIs.Count == 1)
		{
			InputSystem.onActionChange += OnActionChange;
		}
		UpdateBindingDisplay();
	}

	protected void OnDisable()
	{
		m_RebindOperation?.Dispose();
		m_RebindOperation = null;
		s_RebindActionUIs.Remove(this);
		if (s_RebindActionUIs.Count == 0)
		{
			s_RebindActionUIs = null;
			InputSystem.onActionChange -= OnActionChange;
		}
	}

	private static void OnActionChange(object obj, InputActionChange change)
	{
		if (change != InputActionChange.BoundControlsChanged)
		{
			return;
		}
		InputAction action = obj as InputAction;
		InputActionMap actionMap = action?.actionMap ?? (obj as InputActionMap);
		InputActionAsset actionAsset = actionMap?.asset ?? (obj as InputActionAsset);
		for (int i = 0; i < s_RebindActionUIs.Count; i++)
		{
			RebindActionUI component = s_RebindActionUIs[i];
			InputAction referencedAction = component.actionReference?.action;
			if (referencedAction != null && (referencedAction == action || referencedAction.actionMap == actionMap || referencedAction.actionMap?.asset == actionAsset))
			{
				component.UpdateBindingDisplay();
			}
		}
	}

	private void UpdateActionLabel()
	{
		if (m_ActionLabel != null)
		{
			InputAction action = m_Action?.action;
			m_ActionLabel.text = ((action != null) ? action.name : string.Empty);
		}
	}
}
