using System;
using TMPro;
using UnityEngine.UI;

namespace UnityEngine.InputSystem.Samples.RebindUI;

public class GamepadIconsExample : MonoBehaviour
{
	[Serializable]
	public struct GamepadIcons
	{
		public Sprite buttonSouth;

		public Sprite buttonNorth;

		public Sprite buttonEast;

		public Sprite buttonWest;

		public Sprite startButton;

		public Sprite selectButton;

		public Sprite leftTrigger;

		public Sprite rightTrigger;

		public Sprite leftShoulder;

		public Sprite rightShoulder;

		public Sprite dpad;

		public Sprite dpadUp;

		public Sprite dpadDown;

		public Sprite dpadLeft;

		public Sprite dpadRight;

		public Sprite leftStick;

		public Sprite rightStick;

		public Sprite leftStickPress;

		public Sprite rightStickPress;

		public Sprite GetSprite(string controlPath)
		{
			return controlPath switch
			{
				"buttonSouth" => buttonSouth, 
				"buttonNorth" => buttonNorth, 
				"buttonEast" => buttonEast, 
				"buttonWest" => buttonWest, 
				"start" => startButton, 
				"select" => selectButton, 
				"leftTrigger" => leftTrigger, 
				"rightTrigger" => rightTrigger, 
				"leftShoulder" => leftShoulder, 
				"rightShoulder" => rightShoulder, 
				"dpad" => dpad, 
				"dpad/up" => dpadUp, 
				"dpad/down" => dpadDown, 
				"dpad/left" => dpadLeft, 
				"dpad/right" => dpadRight, 
				"leftStick" => leftStick, 
				"rightStick" => rightStick, 
				"leftStickPress" => leftStickPress, 
				"rightStickPress" => rightStickPress, 
				_ => null, 
			};
		}
	}

	public GamepadIcons xbox;

	public GamepadIcons ps4;

	protected void OnEnable()
	{
		RebindActionUI[] rebindUIComponents = base.transform.GetComponentsInChildren<RebindActionUI>();
		RebindActionUI[] array = rebindUIComponents;
		foreach (RebindActionUI component in array)
		{
			component.updateBindingUIEvent.AddListener(OnUpdateBindingDisplay);
			component.UpdateBindingDisplay();
		}
	}

	protected void OnUpdateBindingDisplay(RebindActionUI component, string bindingDisplayString, string deviceLayoutName, string controlPath)
	{
		if (!string.IsNullOrEmpty(deviceLayoutName) && !string.IsNullOrEmpty(controlPath))
		{
			Sprite icon = null;
			if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad"))
			{
				icon = ps4.GetSprite(controlPath);
			}
			else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad"))
			{
				icon = xbox.GetSprite(controlPath);
			}
			TextMeshProUGUI textComponent = component.bindingText;
			Transform imageGO = textComponent.transform.parent.Find("ActionBindingIcon");
			Image imageComponent = imageGO.GetComponent<Image>();
			if (icon != null)
			{
				textComponent.gameObject.SetActive(value: false);
				imageComponent.sprite = icon;
				imageComponent.gameObject.SetActive(value: true);
			}
			else
			{
				textComponent.gameObject.SetActive(value: true);
				imageComponent.gameObject.SetActive(value: false);
			}
		}
	}
}
