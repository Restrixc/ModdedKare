using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ReagentScanner : GenericWeapon, IValuedGood, IGrabbable
{
	private class RaycastHitComparer : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit x, RaycastHit y)
		{
			return x.distance.CompareTo(y.distance);
		}
	}

	public bool firing = false;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Transform center;

	public GameObject canvas;

	public GameObject scannerDisplay;

	public GameObject idleDisplay;

	public GameObject scanBeam;

	public GameObject nothingFoundDisplay;

	public GameObject scannerUIPrefab;

	public Transform laserEmitterLocation;

	public UnityEvent OnSuccess;

	public UnityEvent OnFailure;

	public float scanDelay = 0.09f;

	private static RaycastHit[] hits = new RaycastHit[32];

	private static RaycastHitComparer comparer = new RaycastHitComparer();

	public IEnumerator RenderScreen(ReagentContents reagents)
	{
		for (int j = 0; j < scannerDisplay.transform.childCount; j++)
		{
			UnityEngine.Object.Destroy(scannerDisplay.transform.GetChild(j).gameObject);
		}
		yield return new WaitForSeconds(scanDelay);
		scanBeam.SetActive(value: false);
		if (reagents.volume <= 0f)
		{
			OnFailure.Invoke();
			nothingFoundDisplay.SetActive(value: true);
		}
		else
		{
			nothingFoundDisplay.SetActive(value: false);
		}
		OnSuccess.Invoke();
		float maxVolume = reagents.volume;
		foreach (ScriptableReagent reagent in ReagentDatabase.GetReagents())
		{
			float rvolume = reagents.GetVolumeOf(reagent);
			if (!(rvolume <= 0.05f))
			{
				float width = rvolume / maxVolume;
				GameObject g = UnityEngine.Object.Instantiate(scannerUIPrefab);
				float maxParentWidth = g.GetComponent<RectTransform>().sizeDelta.x;
				TMP_Text t = g.transform.Find("Label").GetComponent<TMP_Text>();
				Image i = g.transform.Find("Level").GetComponent<Image>();
				t.text = reagent.GetLocalizedName().GetLocalizedString() + ": " + rvolume.ToString("F2");
				i.color = Color.Lerp(reagent.GetColor(), Color.black, 0.75f);
				t.color = Color.white;
				i.GetComponent<RectTransform>().sizeDelta = new Vector2(maxParentWidth * width, i.GetComponent<RectTransform>().sizeDelta.y);
				g.transform.SetParent(scannerDisplay.transform, worldPositionStays: false);
				yield return new WaitForSeconds(scanDelay);
			}
		}
	}

	[PunRPC]
	protected override void OnFireRPC(int playerViewID)
	{
		base.OnFireRPC(playerViewID);
		if (firing)
		{
			return;
		}
		firing = true;
		scanBeam.SetActive(value: true);
		idleDisplay.SetActive(value: false);
		int hitCount = Physics.SphereCastNonAlloc(laserEmitterLocation.position - laserEmitterLocation.forward * 0.25f, 0.75f, laserEmitterLocation.forward, hits, 10f, GameManager.instance.waterSprayHitMask, QueryTriggerInteraction.Ignore);
		if (hitCount <= 0)
		{
			ReagentContents noReagents2 = new ReagentContents();
			StopAllCoroutines();
			StartCoroutine(RenderScreen(noReagents2));
			return;
		}
		Array.Sort(hits, 0, hitCount, comparer);
		GenericReagentContainer[] containers = null;
		for (int i = 0; i < hitCount; i++)
		{
			RaycastHit hit = hits[i];
			if (Vector3.Dot(hit.normal, laserEmitterLocation.forward) > 0f)
			{
				continue;
			}
			PhotonView rootView = hit.collider.GetComponentInParent<PhotonView>();
			if (!(rootView != null))
			{
				continue;
			}
			GenericReagentContainer[] containersCheck = rootView.GetComponentsInChildren<GenericReagentContainer>();
			if (containersCheck.Length != 0)
			{
				float vol = 0f;
				GenericReagentContainer[] array = containersCheck;
				foreach (GenericReagentContainer cont in array)
				{
					vol += cont.volume;
				}
				if (vol > 0.01f)
				{
					containers = containersCheck;
					break;
				}
			}
		}
		if (containers == null || containers.Length == 0)
		{
			ReagentContents noReagents = new ReagentContents();
			StopAllCoroutines();
			StartCoroutine(RenderScreen(noReagents));
			return;
		}
		ReagentContents allReagents = new ReagentContents();
		GenericReagentContainer[] array2 = containers;
		foreach (GenericReagentContainer container in array2)
		{
			allReagents.AddMix(container.Peek());
		}
		StopAllCoroutines();
		StartCoroutine(RenderScreen(allReagents));
	}

	[PunRPC]
	protected override void OnEndFireRPC(int viewID)
	{
		firing = false;
	}

	public Vector3 GetWeaponPositionOffset(Transform grabber)
	{
		return grabber.up * 0.1f + grabber.right * 0.5f - grabber.forward * 0.25f;
	}

	public bool ShouldSave()
	{
		return true;
	}

	public float GetWorth()
	{
		return 15f;
	}

	public bool CanGrab(Kobold kobold)
	{
		return true;
	}

	[PunRPC]
	public void OnGrabRPC(int koboldID)
	{
		animator.SetBool("Open", value: true);
	}

	[PunRPC]
	public void OnReleaseRPC(int koboldID, Vector3 velocity)
	{
		animator.SetBool("Open", value: false);
	}

	public Transform GrabTransform()
	{
		return center;
	}

	
}
