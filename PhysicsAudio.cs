using System.Collections;
using Naelstrof.Easing;
using UnityEngine;

public class PhysicsAudio : MonoBehaviour
{
	public float maxVolume = 1f;

	private AudioSource scrapeSoundOutput;

	private float soundDelay = 0.2f;

	private float lastSoundTime = 0f;

	private Rigidbody body;

	public IEnumerator WaitOneFrameThenPause()
	{
		yield return null;
		scrapeSoundOutput.volume = 0f;
		scrapeSoundOutput.Pause();
		scrapeSoundOutput.enabled = false;
	}

	private void Start()
	{
		body = GetComponent<Rigidbody>();
		scrapeSoundOutput = base.gameObject.AddComponent<AudioSource>();
		scrapeSoundOutput.spatialBlend = 1f;
		scrapeSoundOutput.rolloffMode = AudioRolloffMode.Custom;
		scrapeSoundOutput.minDistance = 0f;
		scrapeSoundOutput.maxDistance = 25f;
		scrapeSoundOutput.SetCustomCurve(AudioSourceCurveType.CustomRolloff, GameManager.instance.volumeCurve);
		scrapeSoundOutput.outputAudioMixerGroup = GameManager.instance.soundEffectGroup;
		scrapeSoundOutput.enabled = false;
	}

	private void PlaySoundForCollider(Rigidbody thisBody, Rigidbody otherBody, Collider thisCollider, Collider otherCollider, Vector3 contact, Vector3 relativeVelocity, Vector3 impulse, bool self)
	{
		PhysicsAudioGroup group = PhysicsMaterialDatabase.GetPhysicsAudioGroup(thisCollider.sharedMaterial);
		if (!(group == null))
		{
			float mag = relativeVelocity.magnitude;
			if (((bool)thisBody && thisBody.isKinematic) || ((bool)otherBody && otherBody.isKinematic))
			{
				mag = impulse.magnitude;
			}
			AudioClip clip = group.GetImpactClip(otherCollider, mag);
			if (clip != null)
			{
				GameManager.instance.SpawnAudioClipInWorld(clip, contact, Mathf.Min(maxVolume, group.GetImpactVolume(otherCollider, mag)));
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if ((bool)body && body.isKinematic)
		{
			return;
		}
		ContactPoint cp = collision.GetContact(0);
		PhysicsAudioGroup group = PhysicsMaterialDatabase.GetPhysicsAudioGroup(cp.thisCollider.sharedMaterial);
		if (group == null)
		{
			return;
		}
		AudioClip scrapeclip = group.GetScrapeClip(cp.otherCollider);
		if (scrapeclip != null && scrapeSoundOutput != null)
		{
			scrapeSoundOutput.clip = scrapeclip;
			scrapeSoundOutput.loop = true;
		}
		if (lastSoundTime == 0f || !(Time.timeSinceLevelLoad - lastSoundTime < soundDelay))
		{
			lastSoundTime = Time.timeSinceLevelLoad;
			PlaySoundForCollider(body, collision.rigidbody, cp.thisCollider, cp.otherCollider, cp.point, collision.relativeVelocity, collision.impulse, self: true);
			if (cp.otherCollider.GetComponentInParent<PhysicsAudio>() == null)
			{
				PlaySoundForCollider(body, collision.rigidbody, cp.otherCollider, cp.thisCollider, cp.point, collision.relativeVelocity, collision.impulse, self: false);
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (body != null && body.isKinematic)
		{
			scrapeSoundOutput.volume = 0f;
			return;
		}
		if (!scrapeSoundOutput.isPlaying && scrapeSoundOutput.isActiveAndEnabled && (!collision.rigidbody || !collision.rigidbody.isKinematic))
		{
			if (!scrapeSoundOutput.enabled)
			{
				scrapeSoundOutput.enabled = true;
			}
			scrapeSoundOutput.Play();
		}
		PhysicsAudioGroup group = PhysicsMaterialDatabase.GetPhysicsAudioGroup(collision.GetContact(0).thisCollider.sharedMaterial);
		if (!(group == null))
		{
			scrapeSoundOutput.volume = Easing.Cubic.In(Mathf.Clamp01(collision.relativeVelocity.magnitude - group.minScrapeSpeed));
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (base.isActiveAndEnabled)
		{
			StopCoroutine("WaitOneFrameThenPause");
			StartCoroutine("WaitOneFrameThenPause");
		}
		else if (scrapeSoundOutput != null)
		{
			scrapeSoundOutput.volume = 0f;
			scrapeSoundOutput.Pause();
			scrapeSoundOutput.enabled = false;
		}
	}
}
