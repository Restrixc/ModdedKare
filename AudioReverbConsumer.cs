using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioReverbConsumer : MonoBehaviour
{
	public LayerMask reverbLayer;

	public AudioMixer target;

	public AudioReverbData defaultSettings;

	public AudioReverbData data;

	public AudioReverbData[] nearbyAudioReverb = new AudioReverbData[4];

	private Camera attachedCamera;

	private AudioListener listener;

	private Collider[] colliders = new Collider[4];

	private void Start()
	{
		attachedCamera = GetComponent<Camera>();
		defaultSettings = new AudioReverbData();
		target.GetFloat("HF Reference", out defaultSettings.hfReference);
		target.GetFloat("Density", out defaultSettings.density);
		target.GetFloat("Diffusion", out defaultSettings.diffusion);
		target.GetFloat("Reverb Delay", out defaultSettings.reverbDelay);
		target.GetFloat("Reflections", out defaultSettings.reflections);
		target.GetFloat("Decay HF Ratio", out defaultSettings.decayHFRatio);
		target.GetFloat("Decay Time", out defaultSettings.decayTime);
		target.GetFloat("Room HF", out defaultSettings.roomHF);
		target.GetFloat("Room", out defaultSettings.room);
		target.GetFloat("Room LF", out defaultSettings.roomLF);
		target.GetFloat("LF Reference", out defaultSettings.lfReference);
		data = new AudioReverbData(defaultSettings);
		listener = GetComponent<AudioListener>();
	}

	private void Update()
	{
		if (!attachedCamera.isActiveAndEnabled || !base.isActiveAndEnabled || listener.isActiveAndEnabled)
		{
			return;
		}
		data.hfReference = defaultSettings.hfReference;
		data.density = defaultSettings.density;
		data.diffusion = defaultSettings.diffusion;
		data.reverbDelay = defaultSettings.reverbDelay;
		data.reflections = defaultSettings.reflections;
		data.decayHFRatio = defaultSettings.decayHFRatio;
		data.decayTime = defaultSettings.decayTime;
		data.roomLF = defaultSettings.roomLF;
		data.room = defaultSettings.room;
		data.roomHF = defaultSettings.roomHF;
		data.lfReference = defaultSettings.lfReference;
		int hits = Physics.OverlapSphereNonAlloc(base.transform.position, 10f, colliders, reverbLayer, QueryTriggerInteraction.Collide);
		for (int j = 0; j < hits; j++)
		{
			Collider c = colliders[j];
			if (c == null)
			{
				break;
			}
			AudioReverbArea d = c.GetComponentInParent<AudioReverbArea>();
			if (d != null)
			{
				nearbyAudioReverb[j] = d.data;
			}
		}
		Array.Sort(nearbyAudioReverb, 0, hits);
		for (int i = 0; i < hits; i++)
		{
			AudioReverbData d2 = nearbyAudioReverb[i];
			Vector3 closestPoint = d2.shape.ClosestPoint(base.transform.position);
			float dist = Vector3.Distance(closestPoint, base.transform.position);
			AudioReverbData.Lerp(data, d2, Mathf.Clamp01((d2.fadeDistance - dist) / d2.fadeDistance));
		}
		target.SetFloat("HF Reference", data.hfReference);
		target.SetFloat("Density", data.density);
		target.SetFloat("Diffusion", data.diffusion);
		target.SetFloat("Reverb Delay", data.reverbDelay);
		target.SetFloat("Reverb", data.reverb);
		target.SetFloat("Reflect Delay", data.reflectDelay);
		target.SetFloat("Reflections", data.reflections);
		target.SetFloat("Decay HF Ratio", data.decayHFRatio);
		target.SetFloat("Decay Time", data.decayTime);
		target.SetFloat("Room HF", data.roomHF);
		target.SetFloat("Room", data.room);
		target.SetFloat("Room LF", data.roomLF);
		target.SetFloat("LF Reference", data.lfReference);
	}
}
