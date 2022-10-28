using System.Collections;
using UnityEngine;

public class BillboardObject : MonoBehaviour
{
	private Coroutine routine;

	private int nextUpdate;

	private const float fadeDistance = 10f;

	private void OnEnable()
	{
		routine = StartCoroutine(UpdateRoutine());
	}

	private void OnDisable()
	{
		if (routine != null)
		{
			StopCoroutine(routine);
		}
	}

	private IEnumerator UpdateRoutine()
	{
		while (base.isActiveAndEnabled)
		{
			for (int i = 0; i < nextUpdate; i++)
			{
				yield return null;
			}
			Camera check = Camera.main;
			if (check == null)
			{
				nextUpdate = 64;
				continue;
			}
			float distance = base.transform.DistanceTo(check.transform);
			Vector3 diff = check.transform.position - base.transform.position;
			base.transform.rotation = Quaternion.LookRotation(-diff.normalized, Vector3.up);
			nextUpdate = ((distance < 11f) ? 1 : 64);
		}
	}
}
