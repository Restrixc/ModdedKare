using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReagentContentsDisplayUI : MonoBehaviour
{
	private enum TargetReagentContents
	{
		Belly,
		Metabolized
	}

	[SerializeField]
	private Kobold targetKobold;

	[SerializeField]
	private TargetReagentContents targetContents;

	[SerializeField]
	private RectTransform background;

	[SerializeField]
	private float volumeToPixels = 1f;

	private RectTransform rectTransform;

	[SerializeField]
	private AnimationCurve bounceCurve;

	[SerializeField]
	private Sprite imageSprite;

	private List<Reagent> reagents;

	private List<RectTransform> images;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		reagents = new List<Reagent>();
		images = new List<RectTransform>();
	}

	private void OnEnable()
	{
		switch (targetContents)
		{
		case TargetReagentContents.Belly:
			targetKobold.bellyContainer.OnChange.AddListener(OnReagentContentsChangedOther);
			OnReagentContentsChanged(targetKobold.bellyContainer.GetContents());
			break;
		case TargetReagentContents.Metabolized:
		{
			ReagentContents metabolizedContents = targetKobold.metabolizedContents;
			metabolizedContents.changed = (ReagentContents.ReagentContentsChangedAction)Delegate.Combine(metabolizedContents.changed, new ReagentContents.ReagentContentsChangedAction(OnReagentContentsChanged));
			OnReagentContentsChanged(targetKobold.metabolizedContents);
			break;
		}
		}
	}

	private int SortReagent(Reagent a, Reagent b)
	{
		return a.id.CompareTo(b.id);
	}

	private void OnReagentContentsChanged(ReagentContents contents)
	{
		OnReagentContentsChangedOther(contents, GenericReagentContainer.InjectType.Inject);
	}

	private void OnReagentContentsChangedOther(ReagentContents contents, GenericReagentContainer.InjectType injectType)
	{
		reagents.Clear();
		foreach (Reagent reagent in contents)
		{
			reagents.Add(reagent);
		}
		reagents.Sort(SortReagent);
		for (int l = images.Count; l < reagents.Count; l++)
		{
			Image targetImage = new GameObject("colorBlock", typeof(Image)).GetComponent<Image>();
			targetImage.sprite = imageSprite;
			targetImage.transform.SetParent(base.transform, worldPositionStays: false);
			targetImage.color = ReagentDatabase.GetReagent(reagents[l].id).GetColor();
			targetImage.type = Image.Type.Sliced;
			targetImage.fillCenter = true;
			targetImage.pixelsPerUnitMultiplier = 2f;
			RectTransform imageRect = targetImage.GetComponent<RectTransform>();
			imageRect.anchoredPosition = new Vector2(0f, 0.5f);
			imageRect.anchorMin = new Vector2(0f, 0f);
			imageRect.anchorMax = new Vector2(0f, 1f);
			imageRect.sizeDelta = new Vector2(0f, imageRect.sizeDelta.y);
			images.Add(targetImage.GetComponent<RectTransform>());
		}
		int k = reagents.Count;
		while (k < images.Count)
		{
			UnityEngine.Object.Destroy(images[k].gameObject);
			images.RemoveAt(k);
		}
		if (base.isActiveAndEnabled)
		{
			StopAllCoroutines();
			for (int j = 0; j < reagents.Count; j++)
			{
				images[j].GetComponent<Image>().color = ReagentDatabase.GetReagent(reagents[j].id).GetColor();
				StartCoroutine(TweenWidth(images[j], Mathf.Min(reagents[j].volume * volumeToPixels, 3000f)));
			}
			StartCoroutine(TweenWidth(rectTransform, Mathf.Min(contents.volume * volumeToPixels, 3000f)));
			StartCoroutine(TweenWidth(background, Mathf.Min(contents.GetMaxVolume() * volumeToPixels, 3000f)));
		}
		else
		{
			for (int i = 0; i < reagents.Count; i++)
			{
				images[i].GetComponent<Image>().color = ReagentDatabase.GetReagent(reagents[i].id).GetColor();
				images[i].sizeDelta = new Vector2(reagents[i].volume * volumeToPixels, images[i].sizeDelta.y);
			}
			rectTransform.sizeDelta = new Vector2(contents.volume * volumeToPixels, rectTransform.sizeDelta.y);
			background.sizeDelta = new Vector2(contents.GetMaxVolume() * volumeToPixels, background.sizeDelta.y);
		}
	}

	private IEnumerator TweenWidth(RectTransform targetImage, float targetWidth)
	{
		float startingWidth = targetImage.sizeDelta.x;
		float startTime = Time.time;
		float duration = 1f;
		while (Time.time < startTime + duration)
		{
			float t = (Time.time - startTime) / duration;
			float bounceSample = bounceCurve.Evaluate(t);
			targetImage.sizeDelta = new Vector2(Mathf.Clamp(Mathf.LerpUnclamped(startingWidth, targetWidth, bounceSample), 0f, float.MaxValue), targetImage.sizeDelta.y);
			yield return null;
		}
		targetImage.sizeDelta = new Vector2(targetWidth, targetImage.sizeDelta.y);
	}
}
