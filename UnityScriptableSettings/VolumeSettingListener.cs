using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityScriptableSettings;

public class VolumeSettingListener : MonoBehaviour
{
	public Volume v;

	public SettingInt bloomOption;

	public SettingInt blurOption;

	public SettingFloat paniniOption;

	public SettingFloat postExposure;

	public SettingInt dofOption;

	public SettingInt filmGrainOption;

	private void OnEnable()
	{
		blurOption.changed += OnBlurChanged;
		OnBlurChanged(blurOption.GetValue());
		bloomOption.changed += OnBloomChanged;
		OnBloomChanged(bloomOption.GetValue());
		paniniOption.changed += OnPaniniOptionChanged;
		OnPaniniOptionChanged(paniniOption.GetValue());
		dofOption.changed += OnDepthOfFieldChanged;
		OnDepthOfFieldChanged(dofOption.GetValue());
		filmGrainOption.changed += OnFilmGrainChanged;
		OnFilmGrainChanged(filmGrainOption.GetValue());
		postExposure.changed += OnPostExposureChanged;
		OnPostExposureChanged(postExposure.GetValue());
	}

	private void OnDisable()
	{
		blurOption.changed -= OnBlurChanged;
		bloomOption.changed -= OnBloomChanged;
		paniniOption.changed -= OnPaniniOptionChanged;
		dofOption.changed -= OnDepthOfFieldChanged;
		filmGrainOption.changed -= OnFilmGrainChanged;
		postExposure.changed -= OnPostExposureChanged;
	}

	private void OnBlurChanged(int value)
	{
		if (v.profile.TryGet<MotionBlur>(out var blur))
		{
			blur.active = value != 0;
			switch (value)
			{
			case 1:
				blur.quality.Override(MotionBlurQuality.Low);
				break;
			case 2:
				blur.quality.Override(MotionBlurQuality.Medium);
				break;
			case 3:
				blur.quality.Override(MotionBlurQuality.High);
				break;
			}
		}
	}

	private void OnBloomChanged(int value)
	{
		if (v.profile.TryGet<Bloom>(out var bloom))
		{
			bloom.active = value != 0;
			bloom.highQualityFiltering.Override(value > 1);
		}
	}

	private void OnPaniniOptionChanged(float value)
	{
		if (v.profile.TryGet<PaniniProjection>(out var proj))
		{
			proj.active = value != 0f;
			proj.distance.Override(value);
			proj.cropToFit.Override(1f);
		}
	}

	private void OnDepthOfFieldChanged(int value)
	{
		if (v.profile.TryGet<DepthOfField>(out var depth))
		{
			depth.active = value != 0;
			depth.mode.Override(DepthOfFieldMode.Bokeh);
		}
	}

	private void OnFilmGrainChanged(int value)
	{
		if (v.profile.TryGet<FilmGrain>(out var grain))
		{
			grain.active = value != 0;
		}
	}

	private void OnPostExposureChanged(float value)
	{
		if (v.profile.TryGet<ColorAdjustments>(out var adjustments))
		{
			adjustments.postExposure.Override(value);
		}
	}
}
