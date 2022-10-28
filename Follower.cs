using UnityEngine;
using UnityScriptableSettings;

public class Follower : MonoBehaviour
{
	public Transform target;

	public float distance = 0.12f;

	private Kobold kobold;

	[SerializeField]
	private SettingInt motionSicknessReducer;

	private Vector3 startingPosition;

	private bool ragdoll;

	private void Awake()
	{
		kobold = GetComponentInParent<Kobold>();
		startingPosition = base.transform.localPosition;
		base.transform.localPosition = base.transform.parent.InverseTransformPoint(target.position);
		motionSicknessReducer.changed += OnMotionSicknessReducerChanged;
		OnMotionSicknessReducerChanged(motionSicknessReducer.GetValue());
		kobold.ragdoller.RagdollEvent += RagdollEvent;
	}

	private void OnDestroy()
	{
		if (kobold != null)
		{
			kobold.ragdoller.RagdollEvent -= RagdollEvent;
		}
	}

	private void OnMotionSicknessReducerChanged(int value)
	{
		base.enabled = value == 0;
		base.transform.localPosition = startingPosition;
	}

	private void LateUpdate()
	{
		base.transform.position -= base.transform.up * distance;
		Vector3 a = base.transform.localPosition;
		Vector3 b = base.transform.parent.InverseTransformPoint(target.position);
		if (ragdoll)
		{
			base.transform.localPosition = b;
		}
		else
		{
			base.transform.localPosition = Vector3.MoveTowards(a, b, Time.deltaTime * 5f);
		}
		base.transform.position += base.transform.up * distance;
	}

	public void RagdollEvent(bool ragdolled)
	{
		ragdoll = ragdolled;
		base.transform.localPosition = base.transform.parent.InverseTransformPoint(target.position);
		if (ragdolled && !base.enabled)
		{
			base.enabled = true;
		}
		else if (!ragdolled && motionSicknessReducer.GetValue() == 1)
		{
			base.transform.localPosition = startingPosition;
			base.enabled = false;
		}
	}
}
