using System;
using System.Collections.Generic;
using JigglePhysics;
using Naelstrof.Inflatable;
using UnityEngine;

[Serializable]
public class InflatableBelly : InflatableListener
{
	[SerializeField]
	private string blendShapeStartName;

	[SerializeField]
	private string blendShapeContinueName;

	[SerializeField]
	private Transform targetTransform;

	[SerializeField]
	private List<SkinnedMeshRenderer> skinnedMeshRenderers;

	[SerializeField]
	private JiggleSkin skinJiggle;

	private JiggleSkin.JiggleZone skinZone;

	private float skinZoneStartRadius;

	private List<int> blendshapeStartIDs;

	private List<int> blendshapeContinueIDs;

	public override void OnEnable()
	{
		blendshapeStartIDs = new List<int>();
		blendshapeContinueIDs = new List<int>();
		foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
		{
			int id = renderer.sharedMesh.GetBlendShapeIndex(blendShapeStartName);
			if (id == -1)
			{
				throw new UnityException("Cannot find blendshape " + blendshapeStartIDs?.ToString() + " on mesh " + renderer.sharedMesh);
			}
			blendshapeStartIDs.Add(id);
			int continueID = renderer.sharedMesh.GetBlendShapeIndex(blendShapeContinueName);
			if (continueID == -1)
			{
				throw new UnityException("Cannot find blendshape " + blendshapeStartIDs?.ToString() + " on mesh " + renderer.sharedMesh);
			}
			blendshapeContinueIDs.Add(continueID);
		}
		foreach (JiggleSkin.JiggleZone jiggleZone in skinJiggle.jiggleZones)
		{
			if (jiggleZone.target != targetTransform)
			{
				continue;
			}
			if (!(jiggleZone.jiggleSettings is JiggleSettingsBlend))
			{
				throw new UnityException("Belly jiggle settings must be a JiggleSettingsBlend");
			}
			skinZone = jiggleZone;
			skinZoneStartRadius = skinZone.radius;
			jiggleZone.jiggleSettings = UnityEngine.Object.Instantiate(jiggleZone.jiggleSettings);
			break;
		}
	}

	public void AddTargetRenderer(SkinnedMeshRenderer renderer)
	{
		if (!skinnedMeshRenderers.Contains(renderer))
		{
			skinnedMeshRenderers.Add(renderer);
			blendshapeStartIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(blendShapeStartName));
			blendshapeContinueIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(blendShapeContinueName));
		}
	}

	public void RemoveTargetRenderer(SkinnedMeshRenderer renderer)
	{
		int index = skinnedMeshRenderers.IndexOf(renderer);
		if (index != -1)
		{
			skinnedMeshRenderers.RemoveAt(index);
			blendshapeStartIDs.RemoveAt(index);
			blendshapeContinueIDs.RemoveAt(index);
		}
	}

	public override void OnSizeChanged(float newSize)
	{
		float startWeight = Mathf.Clamp01(newSize);
		for (int i = 0; i < skinnedMeshRenderers.Count; i++)
		{
			float continueWeight = Mathf.Max(0f, newSize - 1f);
			skinnedMeshRenderers[i].SetBlendShapeWeight(blendshapeStartIDs[i], startWeight * 100f);
			skinnedMeshRenderers[i].SetBlendShapeWeight(blendshapeContinueIDs[i], continueWeight * 100f);
		}
		skinZone.radius = skinZoneStartRadius + newSize * skinZoneStartRadius;
		((JiggleSettingsBlend)skinZone.jiggleSettings).normalizedBlend = Mathf.Clamp01(newSize / 2f);
	}
}
