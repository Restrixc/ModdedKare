using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainAudio : MonoBehaviour
{
	[Serializable]
	public class TerrainLayerPhysicsAudioGroupTuple
	{
		public TerrainLayer layer;

		public PhysicMaterial material;

		public TerrainLayerPhysicsAudioGroupTuple(TerrainLayer l)
		{
			layer = l;
		}
	}

	private Terrain targetTerrain;

	public List<TerrainLayerPhysicsAudioGroupTuple> pairs = new List<TerrainLayerPhysicsAudioGroupTuple>();

	private void Start()
	{
		targetTerrain = GetComponent<Terrain>();
		if (Application.isEditor && pairs.Count != targetTerrain.terrainData.terrainLayers.Length)
		{
			pairs.Clear();
			TerrainLayer[] terrainLayers = targetTerrain.terrainData.terrainLayers;
			foreach (TerrainLayer i in terrainLayers)
			{
				pairs.Add(new TerrainLayerPhysicsAudioGroupTuple(i));
			}
		}
	}

	public PhysicMaterial GetMaterialAtPoint(Vector3 contactPoint)
	{
		int dominantIndex = getDominantTexture(contactPoint);
		if (dominantIndex < pairs.Count && pairs[dominantIndex] != null)
		{
			return pairs[dominantIndex].material;
		}
		return null;
	}

	private float[] getTextureMix(Vector3 worldPos)
	{
		int mapX = (int)((worldPos.x - targetTerrain.transform.position.x) / targetTerrain.terrainData.size.x * (float)targetTerrain.terrainData.alphamapWidth);
		int mapZ = (int)((worldPos.z - targetTerrain.transform.position.z) / targetTerrain.terrainData.size.z * (float)targetTerrain.terrainData.alphamapHeight);
		float[,,] splatmapData = targetTerrain.terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
		float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];
		for (int i = 0; i < cellMix.Length; i++)
		{
			cellMix[i] = splatmapData[0, 0, i];
		}
		return cellMix;
	}

	private int getDominantTexture(Vector3 worldPos)
	{
		float[] mix = getTextureMix(worldPos);
		float maxMix = 0f;
		int maxMixIndex = 0;
		for (int i = 0; i < mix.Length; i++)
		{
			if (mix[i] > maxMix)
			{
				maxMixIndex = i;
				maxMix = mix[i];
			}
		}
		return maxMixIndex;
	}
}
