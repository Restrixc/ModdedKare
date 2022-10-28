using System;
using UnityEngine;

namespace JPBotelho;

public class CatmullRom
{
	[Serializable]
	public struct CatmullRomPoint
	{
		public Vector3 position;

		public Vector3 tangent;

		public Vector3 normal;

		public CatmullRomPoint(Vector3 position, Vector3 tangent, Vector3 normal)
		{
			this.position = position;
			this.tangent = tangent;
			this.normal = normal;
		}
	}

	public static CatmullRomPoint Evaluate(Vector3 start, Vector3 tanPoint1, Vector3 tanPoint2, Vector3 end, float t)
	{
		Vector3 position = CalculatePosition(start, tanPoint1, tanPoint2, end, t);
		Vector3 tangent = CalculateTangent(start, tanPoint1, tanPoint2, end, t);
		Vector3 normal = NormalFromTangent(tangent);
		return new CatmullRomPoint(position, tangent, normal);
	}

	public static Vector3 CalculatePosition(Vector3 start, Vector3 tanPoint1, Vector3 tanPoint2, Vector3 end, float t)
	{
		return (2f * t * t * t - 3f * t * t + 1f) * start + (t * t * t - 2f * t * t + t) * tanPoint1 + (-2f * t * t * t + 3f * t * t) * end + (t * t * t - t * t) * tanPoint2;
	}

	public static Vector3 CalculateTangent(Vector3 start, Vector3 tanPoint1, Vector3 tanPoint2, Vector3 end, float t)
	{
		return ((6f * t * t - 6f * t) * start + (3f * t * t - 4f * t + 1f) * tanPoint1 + (-6f * t * t + 6f * t) * end + (3f * t * t - 2f * t) * tanPoint2).normalized;
	}

	public static float ApproximateLength(Vector3 start, Vector3 tanPoint1, Vector3 tanPoint2, Vector3 end, int subdiv)
	{
		Vector3 lastPoint = CalculatePosition(start, tanPoint1, tanPoint2, end, 0f);
		float length = 0f;
		for (int i = 1; i < subdiv; i++)
		{
			float t = (float)i / (float)subdiv;
			Vector3 nextPoint = CalculatePosition(start, tanPoint1, tanPoint2, end, t);
			length += Vector3.Distance(lastPoint, nextPoint);
			lastPoint = nextPoint;
		}
		return length;
	}

	public static Vector3 NormalFromTangent(Vector3 tangent)
	{
		return Vector3.Cross(tangent, Vector3.up).normalized / 2f;
	}

	public static Vector3 ClosestPointOnCurve(Vector3 pt, Vector3 start, Vector3 tanPoint1, Vector3 tanPoint2, Vector3 end, out float bestT, int ndivs)
	{
		Vector3 result = default(Vector3);
		float bestDistance = 0f;
		bestT = 0f;
		for (int i = 0; i <= ndivs; i++)
		{
			float t = (float)i / (float)ndivs;
			Vector3 p = CalculatePosition(start, tanPoint1, tanPoint2, end, t);
			float dissq = Vector3.Distance(p, pt);
			if (i == 0 || dissq < bestDistance)
			{
				bestDistance = dissq;
				bestT = t;
				result = p;
			}
		}
		return result;
	}
}
