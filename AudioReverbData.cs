using System;
using UnityEngine;

public class AudioReverbData : IComparable<AudioReverbData>
{
	public int priority;

	public Collider shape;

	public float fadeDistance;

	public float hfReference;

	public float density;

	public float diffusion;

	public float reverbDelay;

	public float reverb;

	public float reflectDelay;

	public float reflections;

	public float decayHFRatio;

	public float decayTime;

	public float roomHF;

	public float room;

	public float roomLF;

	public float lfReference;

	private AudioReverbData alloc;

	public AudioReverbData()
	{
	}

	public AudioReverbData(AudioReverbData a)
	{
		hfReference = a.hfReference;
		density = a.density;
		diffusion = a.diffusion;
		reverbDelay = a.reverbDelay;
		reverb = a.reverb;
		reflectDelay = a.reflectDelay;
		reflections = a.reflections;
		decayHFRatio = a.decayHFRatio;
		decayTime = a.decayTime;
		roomHF = a.roomHF;
		room = a.room;
		roomLF = a.roomLF;
		lfReference = a.lfReference;
	}

	public AudioReverbData(AudioReverbZone a)
	{
		hfReference = a.HFReference;
		density = a.density;
		diffusion = a.diffusion;
		reverbDelay = a.reverbDelay;
		reverb = a.reverb;
		reflectDelay = a.reflectionsDelay;
		reflections = a.reflections;
		decayHFRatio = a.decayHFRatio;
		decayTime = a.decayTime;
		roomHF = a.roomHF;
		room = a.room;
		roomLF = a.roomLF;
		lfReference = a.LFReference;
	}

	public static AudioReverbData Lerp(AudioReverbData a, AudioReverbData b, float t)
	{
		a.hfReference = Mathf.Lerp(a.hfReference, b.hfReference, t);
		a.density = Mathf.Lerp(a.density, b.density, t);
		a.diffusion = Mathf.Lerp(a.diffusion, b.diffusion, t);
		a.reverbDelay = Mathf.Lerp(a.reverbDelay, b.reverbDelay, t);
		a.reverb = Mathf.Lerp(a.reverb, b.reverb, t);
		a.reflectDelay = Mathf.Lerp(a.reflectDelay, b.reflectDelay, t);
		a.reflections = Mathf.Lerp(a.reflections, b.reflections, t);
		a.decayHFRatio = Mathf.Lerp(a.decayHFRatio, b.decayHFRatio, t);
		a.decayTime = Mathf.Lerp(a.decayTime, b.decayTime, t);
		a.roomHF = Mathf.Lerp(a.roomHF, b.roomHF, t);
		a.room = Mathf.Lerp(a.room, b.room, t);
		a.roomLF = Mathf.Lerp(a.roomLF, b.roomLF, t);
		a.lfReference = Mathf.Lerp(a.lfReference, b.lfReference, t);
		return a;
	}

	public int CompareTo(AudioReverbData other)
	{
		return priority.CompareTo(other.priority);
	}
}
