using UnityEngine;

public static class ColorExtensions
{
	public static Color Invert(this Color rgbColor)
	{
		Color.RGBToHSV(rgbColor, out var h, out var s, out var v);
		if (v < 0.1f || v > 0.9f)
		{
			v = Mathf.MoveTowards(v, 1f - v, 0.75f);
		}
		return Color.HSVToRGB((h + 0.5f) % 1f, s, v);
	}

	public static Color With(this Color original, float? r = null, float? g = null, float? b = null, float? a = null)
	{
		return new Color(r ?? original.r, g ?? original.g, b ?? original.b, a ?? original.a);
	}
}
