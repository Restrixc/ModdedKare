using UnityEngine;

public class SpaceBeam : MonoBehaviour
{
	private Camera targetCamera;

	[SerializeField]
	[Range(0f, 1f)]
	private float scaleFactor = 0.1f;

	[SerializeField]
	private Renderer targetRenderer;

	private MaterialPropertyBlock propertyBlock;

	private static readonly int Visibility = Shader.PropertyToID("_Visibility");

	private void Start()
	{
		base.name = "SpaceBeamDontSave";
		propertyBlock = new MaterialPropertyBlock();
	}

	private void Update()
	{
		if (targetCamera == null || !targetCamera.enabled)
		{
			targetCamera = Camera.main;
		}
		if (targetCamera == null || !targetCamera.enabled)
		{
			targetCamera = Camera.current;
		}
		if (!(targetCamera == null) && targetCamera.enabled)
		{
			float distance = Vector3.Distance(base.transform.position, targetCamera.transform.position);
			base.transform.localScale = Vector3.one * Mathf.Max(distance * scaleFactor * scaleFactor, 1f);
			targetRenderer.GetPropertyBlock(propertyBlock);
			propertyBlock.SetFloat(Visibility, Mathf.Clamp01(distance * scaleFactor * scaleFactor));
			targetRenderer.SetPropertyBlock(propertyBlock);
		}
	}
}
