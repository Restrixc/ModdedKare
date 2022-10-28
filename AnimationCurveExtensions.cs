using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AnimationCurveExtensions
{
	private const float Delta = 1E-06f;

	public static float Differentiate(this AnimationCurve curve, float x)
	{
		return curve.Differentiate(x, curve[0].time, curve[curve.length - 1].time);
	}

	public static float Differentiate(this AnimationCurve curve, float x, float xMin, float xMax)
	{
		float x2 = Mathf.Max(xMin, x - 1E-06f);
		float x3 = Mathf.Min(xMax, x + 1E-06f);
		float y1 = curve.Evaluate(x2);
		float y2 = curve.Evaluate(x3);
		return (y2 - y1) / (x3 - x2);
	}

	private static IEnumerable<float> GetPointSlopes(AnimationCurve curve, int resolution)
	{
		for (int i = 0; i < resolution; i++)
		{
			yield return curve.Differentiate((float)i / (float)resolution);
		}
	}

	public static AnimationCurve Derivative(this AnimationCurve curve, int resolution = 100, float smoothing = 0.05f)
	{
		float[] slopes = GetPointSlopes(curve, resolution).ToArray();
		List<Vector2> curvePoints = slopes.Select((float s, int i) => new Vector2((float)i / (float)resolution, s)).ToList();
		List<Vector2> simplifiedCurvePoints = new List<Vector2>();
		if (smoothing > 0f)
		{
			LineUtility.Simplify(curvePoints, smoothing, simplifiedCurvePoints);
		}
		else
		{
			simplifiedCurvePoints = curvePoints;
		}
		AnimationCurve derivative = new AnimationCurve(simplifiedCurvePoints.Select((Vector2 v) => new Keyframe(v.x, v.y)).ToArray());
		int j = 0;
		for (int len = derivative.keys.Length; j < len; j++)
		{
			derivative.SmoothTangents(j, 0f);
		}
		return derivative;
	}

	public static float Integrate(this AnimationCurve curve, float xStart = 0f, float xEnd = 1f, int sliceCount = 100)
	{
		float sliceWidth = (xEnd - xStart) / (float)sliceCount;
		float accumulatedTotal = (curve.Evaluate(xStart) + curve.Evaluate(xEnd)) / 2f;
		for (int i = 1; i < sliceCount; i++)
		{
			accumulatedTotal += curve.Evaluate(xStart + (float)i * sliceWidth);
		}
		return sliceWidth * accumulatedTotal;
	}
}
