using UnityEngine;

[RequireComponent(typeof(Camera))]
public class EnableCameraDepthBuffer : MonoBehaviour
{
	private void Start()
	{
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
	}
}
