using SkinnedMeshDecals;
using UnityEngine;

namespace UnityScriptableSettings;


public class DecalQualitySetting : SettingFloatClamped
{
	public override void SetValue(float val)
	{
		PaintDecal.SetMemoryBudgetMB(val.Remap(0f, 1f, Mathf.Min(64f, (float)SystemInfo.graphicsMemorySize * 0.025f), Mathf.Min(2048f, (float)SystemInfo.graphicsMemorySize * 0.2f)));
		PaintDecal.SetTexelsPerMeter(val.Remap(0f, 1f, 32f, 512f));
		PaintDecal.SetDilation(val > 0.5f);
		base.SetValue(val);
	}
}
