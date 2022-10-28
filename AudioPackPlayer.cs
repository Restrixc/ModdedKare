using UnityEngine;

public class AudioPackPlayer : MonoBehaviour
{
	[SerializeField]
	private AudioPack pack;

	private AudioSource source;

	private void OnEnable()
	{
		if (source == null)
		{
			source = base.gameObject.AddComponent<AudioSource>();
			source.playOnAwake = false;
			source.maxDistance = 10f;
			source.minDistance = 0.2f;
			source.rolloffMode = AudioRolloffMode.Linear;
			source.spatialBlend = 1f;
			source.loop = false;
		}
		pack.Play(source);
	}
}
