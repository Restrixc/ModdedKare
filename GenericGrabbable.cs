using System;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;

public class GenericGrabbable : MonoBehaviourPun, IGrabbable
{
	[Serializable]
	public class RendererMaterialPair
	{
		public Renderer renderer;

		public Material pickedUpMaterial;

		[HideInInspector]
		public Material defaultMaterial;
	}

	public RendererMaterialPair[] rendererMaterialPairs;

	public Renderer[] renderers;

	public Transform center;

	public bool CanGrab(Kobold kobold)
	{
		return true;
	}

	[PunRPC]
	public void OnGrabRPC(int koboldID)
	{
		RendererMaterialPair[] array = rendererMaterialPairs;
		foreach (RendererMaterialPair pair in array)
		{
			if (pair.pickedUpMaterial != null)
			{
				pair.renderer.material = pair.pickedUpMaterial;
			}
		}
	}

	public void Start()
	{
		RendererMaterialPair[] array = rendererMaterialPairs;
		foreach (RendererMaterialPair pair in array)
		{
			if (pair != null && !(pair.renderer == null))
			{
				pair.defaultMaterial = pair.renderer.material;
			}
		}
		PlayAreaEnforcer.AddTrackedObject(base.photonView);
	}

	private void OnDestroy()
	{
		PlayAreaEnforcer.RemoveTrackedObject(base.photonView);
	}

	[PunRPC]
	public void OnReleaseRPC(int koboldID, Vector3 velocity)
	{
		RendererMaterialPair[] array = rendererMaterialPairs;
		foreach (RendererMaterialPair pair in array)
		{
			if (pair.pickedUpMaterial != null)
			{
				pair.renderer.material = pair.defaultMaterial;
			}
		}
	}

	public Transform GrabTransform()
	{
		return center;
	}

	private void OnValidate()
	{
		if (renderers == null)
		{
			return;
		}
		if (rendererMaterialPairs == null || rendererMaterialPairs.Length != renderers.Length)
		{
			rendererMaterialPairs = new RendererMaterialPair[renderers.Length];
			for (int i = 0; i < renderers.Length; i++)
			{
				rendererMaterialPairs[i] = new RendererMaterialPair();
				rendererMaterialPairs[i].renderer = renderers[i];
				rendererMaterialPairs[i].defaultMaterial = renderers[i].sharedMaterial;
			}
		}
		RendererMaterialPair[] array = rendererMaterialPairs;
		foreach (RendererMaterialPair pair in array)
		{
			if (pair.renderer != null && pair.defaultMaterial == null)
			{
				pair.defaultMaterial = pair.renderer.sharedMaterial;
			}
		}
	}

	
}
