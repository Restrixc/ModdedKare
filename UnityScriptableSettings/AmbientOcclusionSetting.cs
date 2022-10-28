using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityScriptableSettings;


public class AmbientOcclusionSetting : SettingLocalizedDropdown
{
	public UniversalRendererData forwardRenderer;

	public override void SetValue(int value)
	{
		foreach (ScriptableRendererFeature feature in forwardRenderer.rendererFeatures)
		{
			if (feature.name.Contains("AmbientOcclusion"))
			{
				feature.SetActive(value != 0);
				break;
			}
		}
		base.SetValue(value);
	}
}
