using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
	public Transform firstperson;

	private Vector3 offset;

	public LayerMask hitmask;

	private RaycastHit[] hits = new RaycastHit[3];

	private void Start()
	{
		offset = base.transform.localPosition;
	}

	private void Update()
	{
		Vector3 dir = (base.transform.parent.TransformPoint(offset) - firstperson.position).normalized;
		float dist = Vector3.Distance(base.transform.parent.TransformPoint(offset), firstperson.position);
		Vector3 targetPoint = offset;
		if (Physics.Raycast(firstperson.position - dir * 0.1f, dir, out var hit, dist + 0.1f, hitmask, QueryTriggerInteraction.Ignore))
		{
			targetPoint = base.transform.parent.InverseTransformPoint(hit.point) + dir * 0.1f;
		}
		float positionLerpTime = 2f;
		float positionLerpPct = 1f - Mathf.Exp(Mathf.Log(0.00999999f) / positionLerpTime * Time.deltaTime);
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, targetPoint, positionLerpPct);
	}
}
