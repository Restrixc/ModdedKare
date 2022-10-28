using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour
{
	[SerializeField]
	private Kobold targetKobold;

	[SerializeField]
	private Sprite energyBarSprite;

	[SerializeField]
	private Color energyColor;

	[SerializeField]
	private Color deadColor;

	[SerializeField]
	private AnimationCurve flashCurve;

	private List<Image> energyBars;

	[SerializeField]
	private int size = 18;

	private void Start()
	{
		energyBars = new List<Image>();
		targetKobold.energyChanged += OnEnergyChanged;
		OnEnergyChanged(targetKobold.GetEnergy(), targetKobold.GetMaxEnergy());
	}

	private void OnEnergyChanged(float energy, float maxEnergy)
	{
		StopAllCoroutines();
		for (int i = energyBars.Count; (float)i < maxEnergy; i++)
		{
			Image img = new GameObject("EnergyBar", typeof(Image)).GetComponent<Image>();
			img.sprite = energyBarSprite;
			img.color = deadColor;
			img.preserveAspect = true;
			img.transform.SetParent(base.transform, worldPositionStays: false);
			img.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
			energyBars.Add(img);
		}
		int k = energyBars.Count - 1;
		while ((float)k > maxEnergy)
		{
			Object.Destroy(energyBars[k].gameObject);
			k--;
		}
		for (int j = 0; j < energyBars.Count; j++)
		{
			if (base.isActiveAndEnabled)
			{
				StartCoroutine(FlashChange(energyBars[j], ((float)j < energy) ? energyColor : deadColor));
			}
			else
			{
				energyBars[j].color = (((float)j < energy) ? energyColor : deadColor);
			}
		}
	}

	private IEnumerator FlashChange(Image target, Color newColor)
	{
		if (!(target.color == newColor))
		{
			float startTime = Time.unscaledTime;
			float duration = 1f;
			Color oldColor = target.color;
			while (Time.unscaledTime < startTime + duration)
			{
				float t = (Time.unscaledTime - startTime) / duration;
				float sample = flashCurve.Evaluate(t);
				target.color = Color.LerpUnclamped(oldColor, newColor, sample);
				yield return null;
			}
			target.color = newColor;
		}
	}
}
