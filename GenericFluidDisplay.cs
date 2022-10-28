using UnityEngine;

public class GenericFluidDisplay : MonoBehaviour
{
	public GenericReagentContainer container;

	public Renderer targetRenderer;

	public Transform targetTransform;

	public Vector3 scaleDirection = Vector3.up;

	public void Start()
	{
		scaleDirection = new Vector3(Mathf.Abs(scaleDirection.x), Mathf.Abs(scaleDirection.y), Mathf.Abs(scaleDirection.z));
		container.OnChange.AddListener(OnChanged);
		OnChanged(container.GetContents(), GenericReagentContainer.InjectType.Vacuum);
	}

	public void OnDestroy()
	{
		if (container != null)
		{
			container.OnChange.RemoveListener(OnChanged);
		}
	}

	public void OnChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType)
	{
		Material[] materials = targetRenderer.materials;
		foreach (Material i in materials)
		{
			i.color = contents.GetColor();
		}
		targetTransform.localScale = Vector3.one - scaleDirection + scaleDirection * (container.volume / container.maxVolume);
	}
}
