using System.Collections.Generic;
using UnityEngine;

public class PlaceableRadioButton : GenericUsable
{
	[SerializeField]
	private Sprite onSprite;

	[SerializeField]
	private Sprite offSprite;

	[SerializeField]
	private Sprite nextTrack;

	public AudioSource aud;

	public List<AudioClip> tracks = new List<AudioClip>();

	private int trackPos;

	public override Sprite GetSprite(Kobold k)
	{
		return nextTrack;
	}

	public override void Use()
	{
		base.Use();
		trackPos = (int)Mathf.Repeat(trackPos + 1, tracks.Count);
		aud.clip = tracks[trackPos];
		aud.Play();
	}
}
