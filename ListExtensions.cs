using System;
using System.Collections.Generic;

public static class ListExtensions
{
	public static float Sum(this IEnumerable<float> source)
	{
		if (source == null)
		{
			throw new Exception("Null list");
		}
		double sum = 0.0;
		foreach (float v in source)
		{
			sum += (double)v;
		}
		return (float)sum;
	}
}
