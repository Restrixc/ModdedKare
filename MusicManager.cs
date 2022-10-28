using System.Collections;
using System.Collections.Generic;
using KoboldKare;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	[SerializeField]
	private GameEventGeneric sleepEvent;

	private AudioSource musicSource;

	[SerializeField]
	private List<AudioClip> dayMusic;

	[SerializeField]
	private List<AudioClip> nightMusic;

	private bool waiting = false;

	public IEnumerator FadeOutAndStartOver()
	{
		float fadeoutTime = Time.time + 1f;
		while (Time.time < fadeoutTime)
		{
			musicSource.volume = fadeoutTime - Time.time;
			yield return null;
		}
		musicSource.Stop();
		musicSource.volume = 0.7f;
		waiting = false;
	}

	public void Interrupt()
	{
		StopAllCoroutines();
		waiting = true;
		StartCoroutine(FadeOutAndStartOver());
	}

	public IEnumerator WaitAndPlay(float time)
	{
		yield return new WaitForSecondsRealtime(time);
		AudioClip p = ((!(Random.Range(0f, 1f) > 0.25f)) ? nightMusic[Random.Range(0, nightMusic.Count)] : dayMusic[Random.Range(0, dayMusic.Count)]);
		musicSource.clip = p;
		musicSource.Play();
		waiting = false;
	}

	private void Start()
	{
		musicSource = GetComponent<AudioSource>();
		sleepEvent.AddListener(OnSleep);
	}

	private void OnDestroy()
	{
		sleepEvent.RemoveListener(OnSleep);
	}

	private void OnSleep(object nothing)
	{
		Interrupt();
	}

	private void Update()
	{
		if (!musicSource.isPlaying && !waiting)
		{
			waiting = true;
			StartCoroutine(WaitAndPlay(Random.Range(60, 220)));
		}
	}
}
