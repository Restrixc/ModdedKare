using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonMouseOver : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler
{
	public enum ButtonTypes
	{
		Default,
		MainMenu,
		Option,
		Save,
		NoScale
	}

	public enum EventType
	{
		Hover,
		Click
	}

	private Vector3 defaultLocalScale;

	private Button internalAttachedButton;

	private WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

	public EventType lastEvent;

	public ButtonTypes buttonType;

	private Button attachedButton
	{
		get
		{
			if (internalAttachedButton == null)
			{
				internalAttachedButton = GetComponent<Button>();
			}
			return internalAttachedButton;
		}
	}

	private void Start()
	{
		defaultLocalScale = base.transform.localScale;
	}

	public IEnumerator ScaleBack(float scaleDuration)
	{
		if (buttonType != ButtonTypes.NoScale)
		{
			float startTime = Time.unscaledTime;
			while (base.isActiveAndEnabled && attachedButton.interactable && startTime + scaleDuration > Time.unscaledTime)
			{
				base.transform.localScale = Vector3.Lerp(base.transform.localScale, defaultLocalScale, (Time.unscaledTime - startTime) / scaleDuration);
				yield return endOfFrame;
			}
			base.transform.localScale = defaultLocalScale;
		}
	}

	public IEnumerator ScaleUp(float scaleDuration)
	{
		if (buttonType != ButtonTypes.NoScale)
		{
			float startTime = Time.unscaledTime;
			while (base.isActiveAndEnabled && attachedButton.interactable && startTime + scaleDuration > Time.unscaledTime)
			{
				base.transform.localScale = Vector3.Lerp(base.transform.localScale, defaultLocalScale * 1.1f, (Time.unscaledTime - startTime) / scaleDuration);
				yield return endOfFrame;
			}
			base.transform.localScale = defaultLocalScale * 1.1f;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (attachedButton.interactable)
		{
			StopAllCoroutines();
			StartCoroutine(ScaleUp(0.3f));
			lastEvent = EventType.Hover;
			PlaySFX();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (attachedButton.interactable)
		{
			StopAllCoroutines();
			StartCoroutine(ScaleBack(0.3f));
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (attachedButton.interactable)
		{
			StopAllCoroutines();
			StartCoroutine(ScaleUp(0.3f));
			lastEvent = EventType.Hover;
			PlaySFX();
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (attachedButton.interactable)
		{
			StopAllCoroutines();
			StartCoroutine(ScaleBack(0.3f));
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
		lastEvent = EventType.Click;
		PlaySFX();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		lastEvent = EventType.Click;
		PlaySFX();
	}

	private void PlaySFX()
	{
		GameManager.instance.PlayUISFX(this, lastEvent);
	}
}
