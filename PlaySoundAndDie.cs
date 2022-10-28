using System.Collections;
using UnityEngine;

public class PlaySoundAndDie : MonoBehaviour
{
	public void PlayClip(AudioClip c, float volume = 1f)
	{
		AudioSource a = GetComponent<AudioSource>();
		a.pitch = Random.Range(0.8f, 1.2f);
		a.PlayOneShot(c, volume);
		StartCoroutine(WaitAndDie(c.length + 0.1f));
	}

	private IEnumerator WaitAndDie(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		base.gameObject.SetActive(value: false);
	}
}
