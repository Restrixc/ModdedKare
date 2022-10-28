using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using SkinnedMeshDecals;
using UnityEngine;
using UnityEngine.Events;

public class GenericFluidVolume : MonoBehaviourPun
{
	private static List<Renderer> staticTempRenderers = new List<Renderer>();

	private static List<Renderer> staticRenderers = new List<Renderer>();

	public float fillRate = 1f;

	public Transform fluidScaler;

	public List<Renderer> fluidRenderers = new List<Renderer>();

	public Material decalDipMaterial;

	public Material decalClearMaterial;

	public List<BoxCollider> fluidHitboxes = new List<BoxCollider>();

	public GenericReagentContainer volumeContainer;

	public HashSet<PhotonView> dippedObjects;

	public UnityEvent drainStart;

	public UnityEvent drainEnd;

	public IEnumerator DrainProcess()
	{
		while (volumeContainer.volume > 0f)
		{
			volumeContainer.Spill(fillRate * Time.fixedDeltaTime);
			yield return new WaitForFixedUpdate();
		}
		drainEnd.Invoke();
	}

	public void TriggerDrain()
	{
		drainStart.Invoke();
		StartCoroutine(DrainProcess());
	}

	private void Awake()
	{
		dippedObjects = new HashSet<PhotonView>();
	}

	public void Start()
	{
		volumeContainer.OnChange.AddListener(OnReagentContainerChanged);
		OnReagentContainerChanged(volumeContainer.GetContents(), GenericReagentContainer.InjectType.Metabolize);
	}

	public void OnDestroy()
	{
		volumeContainer.OnChange.RemoveListener(OnReagentContainerChanged);
	}

	public void Update()
	{
		dippedObjects.RemoveWhere((PhotonView o) => o == null);
		foreach (PhotonView view in dippedObjects)
		{
			if (view == base.photonView)
			{
				continue;
			}
			DipDecal(view);
			if (base.photonView.IsMine)
			{
				GenericReagentContainer container = view.GetComponentInChildren<GenericReagentContainer>();
				if (container != null && GenericReagentContainer.IsMixable(container.type, GenericReagentContainer.InjectType.Flood))
				{
					float spillVolume = Mathf.Min(container.maxVolume - container.volume, fillRate * Time.deltaTime);
					ReagentContents spill = volumeContainer.Spill(spillVolume);
					container.photonView.RPC("AddMixRPC", RpcTarget.All, spill, volumeContainer.photonView.ViewID);
					volumeContainer.photonView.RPC("Spill", RpcTarget.Others, spillVolume);
				}
			}
		}
		dippedObjects.Clear();
	}

	public void DipDecal(PhotonView view)
	{
		if (view.gameObject.layer == LayerMask.NameToLayer("World") || volumeContainer.volume <= 0f)
		{
			return;
		}
		foreach (BoxCollider b in fluidHitboxes)
		{
			Vector3 boxCorner = b.size / 2f;
			Vector3 boxFrontWorld = b.transform.TransformPoint(b.center + new Vector3(0f, 0f, boxCorner.z));
			Vector3 boxRightWorld = b.transform.TransformPoint(b.center + new Vector3(boxCorner.x, 0f, 0f));
			Vector3 boxUpWorld = b.transform.TransformPoint(b.center + new Vector3(0f, boxCorner.y, 0f));
			Vector3 boxCenterWorld = b.transform.TransformPoint(b.center);
			Vector2 rectangle = new Vector2((boxRightWorld - boxCenterWorld).magnitude, (boxUpWorld - boxCenterWorld).magnitude) * 2f;
			float depth = (boxFrontWorld - boxCenterWorld).magnitude * 2f;
			Vector3 pos = boxFrontWorld;
			Vector3 norm = (boxCenterWorld - boxFrontWorld).normalized;
			Color c = volumeContainer.GetColor();
			c.a = 1f;
			if (volumeContainer.IsCleaningAgent())
			{
				staticRenderers.Clear();
				view.transform.GetComponentsInChildrenNoAlloc(staticTempRenderers, staticRenderers);
				foreach (Renderer r2 in staticRenderers)
				{
					PaintDecal.RenderDecal(r2, decalClearMaterial, pos, Quaternion.FromToRotation(Vector3.forward, norm), rectangle, depth);
				}
				continue;
			}
			decalDipMaterial.color = c;
			staticRenderers.Clear();
			view.transform.GetComponentsInChildrenNoAlloc(staticTempRenderers, staticRenderers);
			foreach (Renderer r in staticRenderers)
			{
				PaintDecal.RenderDecal(r, decalDipMaterial, pos, Quaternion.FromToRotation(Vector3.forward, norm), rectangle, depth);
			}
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		PhotonView view = other.GetComponentInParent<PhotonView>();
		if (view != null)
		{
			dippedObjects.Add(view);
		}
	}

	public void OnTriggerStay(Collider other)
	{
		PhotonView view = other.GetComponentInParent<PhotonView>();
		if (view != null)
		{
			dippedObjects.Add(view);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "ico_fluidvolume.png", allowScaling: true);
	}

	public void OnReagentContainerChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType)
	{
		foreach (Renderer r in fluidRenderers)
		{
			Material[] materials = r.materials;
			foreach (Material material in materials)
			{
				material.color = contents.GetColor();
				material.SetFloat("_Position", contents.volume / contents.GetMaxVolume());
			}
		}
		if (fluidScaler != null)
		{
			Transform obj = fluidScaler;
			Vector3 localScale = fluidScaler.localScale;
			float? y = contents.volume / contents.GetMaxVolume();
			obj.localScale = localScale.With(null, y);
		}
	}

	public bool HasAnyFillAmount()
	{
		return !volumeContainer.isEmpty;
	}
}
