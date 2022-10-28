using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomAmbientSound : MonoBehaviour
{
	public List<AudioClip> sounds = new List<AudioClip>();

	public float delayStart;

	public float delayBetween;

	public float delayUpToRandom;

	public float randomDelayStartAdd;

	public AudioSource audioSource;

	public bool play;

	public bool startPlaying;

	private Coroutine audioplayer;

	private void Start()
	{
		if (startPlaying)
		{
			StartPlayingSounds();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
	}

	private void OnTriggerExit(Collider other)
	{
	}

	public void StartPlayingSounds()
	{
		play = true;
		audioplayer = StartCoroutine(ShuffleAndPlaySounds());
	}

	private IEnumerator ShuffleAndPlaySounds()
	{
		while (play)
		{
			yield return new WaitForSeconds(delayStart + Random.Range(0f, randomDelayStartAdd));
			audioSource.PlayOneShot(sounds[Random.Range(0, sounds.Count - 1)]);
			yield return new WaitForSeconds(delayBetween + Random.Range(0f, delayUpToRandom));
		}
	}
}
