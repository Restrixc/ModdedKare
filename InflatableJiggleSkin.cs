using System;
using JigglePhysics;
using Naelstrof.Inflatable;
using UnityEngine;

[Serializable]
public class InflatableJiggleSkin : InflatableListener
{
	[SerializeField]
	private JiggleSkin targetJiggleSkin;

	[SerializeField]
	private Transform targetTransform;

	private JiggleSkin.JiggleZone targetZone;

	private float defaultRadius;

	public override void OnEnable()
	{
		base.OnEnable();
		foreach (JiggleSkin.JiggleZone jiggleRig in targetJiggleSkin.jiggleZones)
		{
			if (jiggleRig.target == targetTransform)
			{
				targetZone = jiggleRig;
			}
		}
		defaultRadius = targetZone.radius;
	}

	public override void OnSizeChanged(float newSize)
	{
		base.OnSizeChanged(newSize);
		targetZone.radius = defaultRadius + newSize * 0.75f;
	}
}
