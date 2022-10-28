using UnityEngine;

public class FaceCamera : MonoBehaviour
{
	private Camera cachedCamera;

	private Camera cam
	{
		get
		{
			if (cachedCamera == null || !cachedCamera.isActiveAndEnabled)
			{
				cachedCamera = Camera.current;
			}
			if (cachedCamera == null || !cachedCamera.isActiveAndEnabled)
			{
				cachedCamera = Camera.main;
			}
			return cachedCamera;
		}
	}

	private void LateUpdate()
	{
		if (!(cam == null))
		{
			base.transform.rotation = Quaternion.LookRotation((base.transform.position - cam.transform.position).normalized);
		}
	}
}
