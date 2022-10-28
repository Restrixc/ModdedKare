using System;
using System.Collections;
using UnityEngine;

public class EnergyBarDisplay : MonoBehaviour
{
	[SerializeField]
	private Renderer energyBar;

	[SerializeField]
	private Renderer energyBarContainer;

	[SerializeField]
	private AnimationCurve bounceCurve;

	private float desiredValue;

	private float desiredMaxValue;

	private bool animating = false;

	private static readonly int Value = Shader.PropertyToID("_Value");

	private static readonly int MaxValue = Shader.PropertyToID("_MaxValue");

	private static readonly int ColorID = Shader.PropertyToID("_Color");

	private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

	private Kobold kobold;

	private void Start()
	{
		kobold = GetComponentInParent<Kobold>();
		kobold.energyChanged += OnEnergyChanged;
		Material material = energyBar.material;
		int colorID = ColorID;
		Color color = energyBar.material.GetColor(ColorID);
		float? a = 0f;
		material.SetColor(colorID, color.With(null, null, null, a));
		Material material2 = energyBarContainer.material;
		int baseColorID = BaseColorID;
		Color color2 = energyBarContainer.material.GetColor(BaseColorID);
		a = 0f;
		material2.SetColor(baseColorID, color2.With(null, null, null, a));
		OnEnergyChanged(kobold.GetEnergy(), kobold.GetMaxEnergy());
	}

	private void OnDestroy()
	{
		kobold.energyChanged -= OnEnergyChanged;
	}

	private void OnDisable()
	{
		Material material = energyBar.material;
		int colorID = ColorID;
		Color color = energyBar.material.GetColor(ColorID);
		float? a = 0f;
		material.SetColor(colorID, color.With(null, null, null, a));
		Material material2 = energyBarContainer.material;
		int baseColorID = BaseColorID;
		Color color2 = energyBarContainer.material.GetColor(BaseColorID);
		a = 0f;
		material2.SetColor(baseColorID, color2.With(null, null, null, a));
	}

	private void OnEnergyChanged(float value, float maxValue)
	{
		if (!(Math.Abs(desiredValue - value) < 0.01f) || !(Math.Abs(desiredMaxValue - maxValue) < 0.01f))
		{
			desiredValue = value;
			desiredMaxValue = maxValue;
			if (!animating)
			{
				StopAllCoroutines();
				StartCoroutine(EnergyAnimation(energyBar.material.GetFloat(Value), energyBar.material.GetFloat(MaxValue)));
			}
		}
	}

	private IEnumerator EnergyAnimation(float fromValue, float fromMaxValue)
	{
		animating = true;
		energyBar.enabled = true;
		energyBarContainer.enabled = true;
		float startTime = Time.time;
		float duration = 2f;
		float? a;
		while (Time.time < startTime + duration)
		{
			float t2 = (Time.time - startTime) / duration;
			float sample = bounceCurve.Evaluate(t2);
			float lerpValue = Mathf.Lerp(fromValue, desiredValue, sample);
			float lerpMaxValue = Mathf.Lerp(fromMaxValue, desiredMaxValue, sample);
			base.transform.localScale = new Vector3(lerpMaxValue * 0.5f, 0.25f, 0.25f);
			Material material = energyBar.material;
			int colorID = ColorID;
			Color color = energyBar.material.GetColor(ColorID);
			a = 1f;
			material.SetColor(colorID, color.With(null, null, null, a));
			Material material2 = energyBarContainer.material;
			int baseColorID = BaseColorID;
			Color color2 = energyBarContainer.material.GetColor(BaseColorID);
			a = 1f;
			material2.SetColor(baseColorID, color2.With(null, null, null, a));
			energyBar.material.SetFloat(Value, lerpValue);
			energyBar.material.SetFloat(MaxValue, lerpMaxValue);
			yield return null;
		}
		base.transform.localScale = new Vector3(desiredMaxValue * 0.5f, 0.25f, 0.25f);
		energyBar.material.SetFloat(Value, desiredValue);
		energyBar.material.SetFloat(MaxValue, desiredMaxValue);
		animating = false;
		yield return new WaitForSeconds(3f);
		float startFadeTime = Time.time;
		float fadeDuration = 1f;
		while (Time.time < startFadeTime + fadeDuration)
		{
			float t = (Time.time - startFadeTime) / fadeDuration;
			Material material3 = energyBar.material;
			int colorID2 = ColorID;
			Color color3 = energyBar.material.GetColor(ColorID);
			a = 1f - t;
			material3.SetColor(colorID2, color3.With(null, null, null, a));
			Material material4 = energyBarContainer.material;
			int baseColorID2 = BaseColorID;
			Color color4 = energyBarContainer.material.GetColor(BaseColorID);
			a = 1f - t;
			material4.SetColor(baseColorID2, color4.With(null, null, null, a));
			yield return null;
		}
		Material material5 = energyBar.material;
		int colorID3 = ColorID;
		Color color5 = energyBar.material.GetColor(ColorID);
		a = 0f;
		material5.SetColor(colorID3, color5.With(null, null, null, a));
		Material material6 = energyBarContainer.material;
		int baseColorID3 = BaseColorID;
		Color color6 = energyBarContainer.material.GetColor(BaseColorID);
		a = 0f;
		material6.SetColor(baseColorID3, color6.With(null, null, null, a));
		energyBar.enabled = false;
		energyBarContainer.enabled = false;
	}
}
