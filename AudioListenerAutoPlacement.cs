using UnityEngine;

public class AudioListenerAutoPlacement : MonoBehaviour
{
	private Camera cam;

	private void LateUpdate()
	{
		if (cam == null || !cam.isActiveAndEnabled)
		{
			cam = Camera.main;
		}
		if (cam == null || !cam.isActiveAndEnabled)
		{
			cam = Camera.current;
		}
		if (!(cam == null) && cam.isActiveAndEnabled)
		{
			base.transform.position = cam.transform.position;
			base.transform.rotation = cam.transform.rotation;
		}
	}
}
