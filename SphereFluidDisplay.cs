using UnityEngine;

public class SphereFluidDisplay : MonoBehaviour
{
	public GenericReagentContainer container;

	public Rigidbody body;

	[Range(0f, 100f)]
	public float spring = 50f;

	[Range(0f, 10f)]
	public float damping = 0f;

	public Renderer fluidRenderer;

	private Vector3 vel;

	private Vector3 pos;

	private void Start()
	{
		vel = Vector3.zero;
		pos = Vector3.up;
		container.OnChange.AddListener(OnChanged);
		OnChanged(container.GetContents(), GenericReagentContainer.InjectType.Inject);
	}

	private void OnDestroy()
	{
		container.OnChange.RemoveListener(OnChanged);
	}

	private void FixedUpdate()
	{
		Vector3 normal = body.velocity - Physics.gravity;
		Vector3 wantedNormal = fluidRenderer.transform.InverseTransformDirection(Vector3.Normalize(normal));
		vel += ((wantedNormal - pos) * spring - vel * damping) * Time.fixedDeltaTime;
		pos = Vector3.Normalize(pos + vel * Time.fixedDeltaTime);
		fluidRenderer.material.SetVector("_PlaneNormal", pos);
	}

	private void OnChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType)
	{
		fluidRenderer.material.SetColor("_Color", contents.GetColor());
		fluidRenderer.material.SetFloat("_Position", contents.volume / contents.GetMaxVolume());
	}
}
