using System.Collections;
using System.Collections.Generic;
using KoboldKare;
using UnityEngine;

public class CateringHandler : MonoBehaviour
{
	public GameEventGeneric morningEvent;

	public GameEventGeneric middayEvent;

	public GameEventGeneric midnightEvent;

	public float cateringChance = 0.15f;

	public bool vanArrived = false;

	public List<MeshRenderer> cateringVan = new List<MeshRenderer>();

	public List<MeshRenderer> cateringVanCity = new List<MeshRenderer>();

	public AudioClip catererArrives;

	public AudioClip catererLeaves;

	private Coroutine catererHurryUp;

	public AudioSource aud;

	private void Start()
	{
		morningEvent.AddListener(evalCatering);
		middayEvent.AddListener(AssignAwards);
		midnightEvent.AddListener(LeaveImmediately);
	}

	private void OnDestroy()
	{
		morningEvent.RemoveListener(evalCatering);
		middayEvent.RemoveListener(AssignAwards);
		midnightEvent.RemoveListener(LeaveImmediately);
	}

	private void evalCatering(object nothing)
	{
		float rnd = Random.Range(0f, 1f);
		if (!(rnd < cateringChance))
		{
			return;
		}
		aud.PlayOneShot(catererArrives);
		foreach (MeshRenderer item2 in cateringVan)
		{
			item2.enabled = true;
			item2.GetComponent<MeshCollider>().enabled = true;
		}
		foreach (MeshRenderer item in cateringVanCity)
		{
			item.enabled = false;
			item.GetComponent<MeshCollider>().enabled = false;
		}
		vanArrived = true;
		Debug.Log("Caterer arrived!");
	}

	private void AssignAwards(object nothing)
	{
		if (vanArrived)
		{
			catererHurryUp = StartCoroutine(HurryUp());
		}
	}

	private void LeaveImmediately(object nothing)
	{
		foreach (MeshRenderer item2 in cateringVan)
		{
			item2.enabled = false;
			item2.GetComponent<MeshCollider>().enabled = false;
		}
		foreach (MeshRenderer item in cateringVanCity)
		{
			item.enabled = true;
			item.GetComponent<MeshCollider>().enabled = true;
		}
		if (catererHurryUp != null)
		{
			StopCoroutine(catererHurryUp);
			catererHurryUp = null;
		}
		Debug.Log("Caterer left; nobody was home!");
	}

	private IEnumerator HurryUp()
	{
		if (!aud.isPlaying)
		{
			aud.PlayOneShot(catererArrives);
		}
		Debug.Log("Caterer is about to leave!");
		yield return new WaitForSeconds(20f);
		Debug.Log("Caterer headed back to the city!");
		foreach (MeshRenderer item2 in cateringVan)
		{
			item2.enabled = false;
			item2.GetComponent<MeshCollider>().enabled = false;
		}
		foreach (MeshRenderer item in cateringVanCity)
		{
			item.enabled = true;
			item.GetComponent<MeshCollider>().enabled = true;
		}
		aud.PlayOneShot(catererLeaves);
	}
}
