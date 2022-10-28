using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NavigateSelectSomething : MonoBehaviour
{
	[SerializeField]
	private InputActionReference navigate;

	private void OnEnable()
	{
		navigate.action.performed += OnPerformed;
	}

	private void OnDisable()
	{
		navigate.action.performed -= OnPerformed;
	}

	private void OnPerformed(InputAction.CallbackContext ctx)
	{
		if (Cursor.lockState == CursorLockMode.Locked || (!(EventSystem.current.currentSelectedGameObject == null) && EventSystem.current.currentSelectedGameObject.activeInHierarchy && EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().IsInteractable()))
		{
			return;
		}
		Selectable[] array = Object.FindObjectsOfType<Selectable>();
		foreach (Selectable selectable in array)
		{
			if (selectable.IsInteractable() && selectable.isActiveAndEnabled)
			{
				EventSystem.current.SetSelectedGameObject(selectable.gameObject);
			}
		}
	}
}
