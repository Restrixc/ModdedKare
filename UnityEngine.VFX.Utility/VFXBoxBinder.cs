using UnityEngine.Serialization;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Box Binder")]
[VFXBinder("AABB/Box")]
internal class VFXBoxBinder : VFXBinderBase
{
	[SerializeField]
	[VFXPropertyBinding(new string[] { "UnityEditor.VFX.Box" })]
	[FormerlySerializedAs("m_Parameter")]
	protected ExposedProperty m_Property = "Box";

	public BoxCollider boxCollider;

	private ExposedProperty BoxCenter;

	private ExposedProperty BoxSize;

	public string Property
	{
		get
		{
			return (string)m_Property;
		}
		set
		{
			m_Property = value;
			UpdateSubProperties();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateSubProperties();
	}

	private void OnValidate()
	{
		UpdateSubProperties();
	}

	private void UpdateSubProperties()
	{
		BoxCenter = m_Property + "_center";
		BoxSize = m_Property + "_size";
	}

	public override bool IsValid(VisualEffect component)
	{
		return boxCollider != null && component.HasVector3(BoxCenter) && component.HasVector3(BoxSize);
	}

	public override void UpdateBinding(VisualEffect component)
	{
		component.SetVector3(BoxCenter, boxCollider.transform.TransformPoint(boxCollider.center));
		component.SetVector3(BoxSize, boxCollider.size);
	}

	public override string ToString()
	{
		return string.Format("Box : '{0}' -> {1}", m_Property, (boxCollider == null) ? "(null)" : boxCollider.ToString());
	}
}
