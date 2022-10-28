using Photon.Pun;
using UnityEngine;

public class VolumetricSound : MonoBehaviour
{
	private AudioListener listener;

	private AudioSource source;

	private void Start()
	{
		source = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (listener == null || !listener.isActiveAndEnabled)
		{
			if (PhotonNetwork.LocalPlayer.TagObject != null)
			{
				AudioListener[] componentsInChildren = (PhotonNetwork.LocalPlayer.TagObject as Kobold).GetComponentsInChildren<AudioListener>();
				foreach (AudioListener i in componentsInChildren)
				{
					if (i.isActiveAndEnabled)
					{
						listener = i;
						break;
					}
				}
			}
			if (listener == null || !listener.isActiveAndEnabled)
			{
				return;
			}
		}
		float dist = Vector3.Distance(base.transform.position, listener.transform.position);
		source.spatialBlend = Mathf.Clamp01((dist - source.minDistance) / (source.maxDistance - source.minDistance));
	}
}
