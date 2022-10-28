using System.Collections.Generic;
using JigglePhysics;
using Naelstrof.Inflatable;
using PenetrationTech;
using UnityEngine;

public class ThirdPersonMeshDisplay : MonoBehaviour
{
	private List<GameObject> mirrorObjects = new List<GameObject>();

	private Dictionary<SkinnedMeshRenderer, SkinnedMeshRenderer> smrCopies = new Dictionary<SkinnedMeshRenderer, SkinnedMeshRenderer>();

	public Kobold kobold;

	private ProceduralDeformation proceduralDeformation;

	public LODGroup group;

	public JiggleSkin physics;

	public List<SkinnedMeshRenderer> dissolveTargets = new List<SkinnedMeshRenderer>();

	private static readonly int Head = Shader.PropertyToID("_Head");

	public void OnEnable()
	{
		proceduralDeformation = kobold.GetComponentInChildren<ProceduralDeformation>();
		RegenerateMirror();
	}

	public void OnDisable()
	{
		DestroyMirror();
	}

	public void Update()
	{
		foreach (KeyValuePair<SkinnedMeshRenderer, SkinnedMeshRenderer> pair in smrCopies)
		{
			for (int i = 0; i < pair.Key.sharedMesh.blendShapeCount; i++)
			{
				pair.Value.SetBlendShapeWeight(i, pair.Key.GetBlendShapeWeight(i));
			}
		}
	}

	private void DestroyMirror()
	{
		foreach (GameObject g in mirrorObjects)
		{
			if (g == null)
			{
				continue;
			}
			SkinnedMeshRenderer[] componentsInChildren = g.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer r in componentsInChildren)
			{
				if (kobold.koboldBodyRenderers.Contains(r))
				{
					kobold.koboldBodyRenderers.Remove(r);
				}
				if (physics.targetSkins.Contains(r))
				{
					physics.targetSkins.Remove(r);
				}
				proceduralDeformation.RemoveTargetRenderer(r);
				foreach (InflatableListener inflatable in kobold.GetAllInflatableListeners())
				{
					if (inflatable is InflatableBreast breast)
					{
						breast.RemoveTargetRenderer(r);
					}
					if (inflatable is InflatableBlendShape blendshape)
					{
						blendshape.RemoveTargetRenderer(r);
					}
					if (inflatable is InflatableBelly belly)
					{
						belly.RemoveTargetRenderer(r);
					}
				}
			}
			Object.Destroy(g);
		}
		mirrorObjects.Clear();
		foreach (SkinnedMeshRenderer s in dissolveTargets)
		{
			Material[] materials = s.GetComponent<SkinnedMeshRenderer>().materials;
			foreach (Material i in materials)
			{
				i.SetFloat(Head, 1f);
			}
			s.gameObject.layer = LayerMask.NameToLayer("Player");
		}
	}

	private void RegenerateMirror()
	{
		DestroyMirror();
		LOD[] lods = group.GetLODs();
		List<Renderer> renderers = new List<Renderer>(lods[0].renderers);
		for (int j = 0; j < renderers.Count; j++)
		{
			if (renderers[j] == null || renderers[j].gameObject == null)
			{
				renderers.RemoveAt(j);
			}
		}
		foreach (GameObject g2 in mirrorObjects)
		{
			SkinnedMeshRenderer[] componentsInChildren = g2.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer r in componentsInChildren)
			{
				if (renderers.Contains(r))
				{
					renderers.Remove(r);
				}
			}
		}
		lods[0].renderers = renderers.ToArray();
		group.SetLODs(lods);
		mirrorObjects.Clear();
		smrCopies.Clear();
		foreach (SkinnedMeshRenderer s in dissolveTargets)
		{
			if (!s.gameObject.activeInHierarchy)
			{
				continue;
			}
			GameObject g = Object.Instantiate(s.gameObject, s.transform.position, s.transform.rotation);
			mirrorObjects.Add(g);
			g.layer = LayerMask.NameToLayer("MirrorReflection");
			g.transform.parent = s.transform.parent;
			smrCopies[s] = g.GetComponent<SkinnedMeshRenderer>();
			kobold.koboldBodyRenderers.Add(smrCopies[s]);
			lods = group.GetLODs();
			renderers = new List<Renderer>(lods[0].renderers);
			for (int i = 0; i < renderers.Count; i++)
			{
				if (renderers[i] == null || renderers[i].gameObject == null)
				{
					renderers.RemoveAt(i);
				}
			}
			renderers.Add(g.GetComponent<SkinnedMeshRenderer>());
			lods[0].renderers = renderers.ToArray();
			group.SetLODs(lods);
			if (s.gameObject.name == "Body")
			{
				physics.targetSkins.Add(smrCopies[s]);
				foreach (InflatableListener inflatable in kobold.GetAllInflatableListeners())
				{
					if (inflatable is InflatableBreast breast)
					{
						breast.AddTargetRenderer(g.GetComponent<SkinnedMeshRenderer>());
					}
					if (inflatable is InflatableBlendShape blendshape)
					{
						blendshape.AddTargetRenderer(g.GetComponent<SkinnedMeshRenderer>());
					}
					if (inflatable is InflatableBelly belly)
					{
						belly.AddTargetRenderer(g.GetComponent<SkinnedMeshRenderer>());
					}
				}
				proceduralDeformation.AddTargetRenderer(g.GetComponent<SkinnedMeshRenderer>());
			}
			Material[] materials = g.GetComponent<SkinnedMeshRenderer>().materials;
			foreach (Material k in materials)
			{
				k.SetFloat(Head, 1f);
			}
			Material[] materials2 = s.materials;
			foreach (Material l in materials2)
			{
				l.SetFloat(Head, 0f);
			}
			s.gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
		}
	}
}
