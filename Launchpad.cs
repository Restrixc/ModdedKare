using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Launchpad : UsableMachine
{
	public Vector3 localTarget;

	public float flightTime = 5f;

	public float playerFlightTime = 4.5f;

	private WaitForFixedUpdate waitForFixedUpdate;

	public UnityEvent OnFire;

	[HideInInspector]
	public Vector3 playerGravityMod = new Vector3(0f, -4f, 0f);

	private float fireDelay = 1f;

	private float lastFireTime;

	public override bool CanUse(Kobold k)
	{
		return false;
	}

	private void Awake()
	{
		waitForFixedUpdate = new WaitForFixedUpdate();
	}

	private IEnumerator HighQualityCollision(Rigidbody body)
	{
		if (body.collisionDetectionMode == CollisionDetectionMode.Discrete)
		{
			body.collisionDetectionMode = CollisionDetectionMode.Continuous;
			while (body.velocity.magnitude > 1f)
			{
				yield return waitForFixedUpdate;
			}
			body.collisionDetectionMode = CollisionDetectionMode.Discrete;
		}
	}

	private IEnumerator DisableControllerForTime(KoboldCharacterController controller)
	{
		controller.enabled = false;
		yield return new WaitForSeconds(0.5f);
		controller.enabled = true;
	}

	public override void SetConstructed(bool isConstructed)
	{
		base.SetConstructed(isConstructed);
		Collider[] components = GetComponents<Collider>();
		foreach (Collider coll in components)
		{
			coll.enabled = isConstructed;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!constructed)
		{
			return;
		}
		Rigidbody[] rigidbodies = other.GetAllComponents<Rigidbody>();
		if (rigidbodies.Length == 0)
		{
			return;
		}
		KoboldCharacterController controller = other.GetComponentInParent<KoboldCharacterController>();
		float gravity = Physics.gravity.y;
		float alteredFlightTime = flightTime;
		if (controller != null && !controller.body.isKinematic)
		{
			gravity = Physics.gravity.y + controller.gravityMod.y * controller.transform.lossyScale.x;
			alteredFlightTime = playerFlightTime;
			StartCoroutine(DisableControllerForTime(controller));
		}
		else
		{
			Rigidbody[] array = rigidbodies;
			foreach (Rigidbody body in array)
			{
				StartCoroutine(HighQualityCollision(body));
			}
		}
		float initialYVelocity = (base.transform.TransformVector(localTarget).y - 0.5f * gravity * (alteredFlightTime * alteredFlightTime)) / alteredFlightTime;
		Vector3 original = base.transform.TransformVector(localTarget);
		float? y = 0f;
		float xDistance = original.With(null, y).magnitude;
		float initialXVelocity = xDistance / alteredFlightTime;
		Vector3 original2 = base.transform.TransformVector(localTarget);
		y = 0f;
		Vector3 xForceDir = original2.With(null, y).normalized;
		Vector3 yForceDir = Vector3.up;
		Vector3 initialVelocity = xForceDir * initialXVelocity + yForceDir * initialYVelocity;
		Rigidbody[] array2 = rigidbodies;
		foreach (Rigidbody r in array2)
		{
			r.velocity = initialVelocity;
		}
		if (lastFireTime + fireDelay < Time.time)
		{
			OnFire.Invoke();
			lastFireTime = Time.time;
		}
	}

	private void OnDrawGizmos()
	{
		Vector3 original = base.transform.TransformVector(localTarget);
		float? y = 0f;
		float xDistance = original.With(null, y).magnitude;
		float initialXVelocity = xDistance / flightTime;
		float initialYVelocity = (base.transform.TransformVector(localTarget).y - 0.5f * Physics.gravity.y * (flightTime * flightTime)) / flightTime;
		float initialpXVelocity = xDistance / playerFlightTime;
		float initialpYVelocity = (base.transform.TransformVector(localTarget).y - 0.5f * (Physics.gravity.y + playerGravityMod.y) * (playerFlightTime * playerFlightTime)) / playerFlightTime;
		Vector3 lastPos = base.transform.position;
		Vector3 lastpPos = base.transform.position;
		Vector3 original2 = base.transform.TransformVector(localTarget);
		y = 0f;
		Vector3 xForceDir = original2.With(null, y).normalized;
		Vector3 yForceDir = Vector3.up;
		Gizmos.color = Color.white;
		for (float t2 = 0f; t2 < flightTime; t2 += 0.5f)
		{
			Vector3 sample = base.transform.position + xForceDir * initialXVelocity * t2 + yForceDir * (initialYVelocity * t2 + 0.5f * Physics.gravity.y * (t2 * t2));
			Gizmos.DrawLine(lastPos, sample);
			lastPos = sample;
		}
		Gizmos.DrawLine(lastPos, base.transform.TransformPoint(localTarget));
		Gizmos.color = Color.green;
		for (float t = 0f; t < playerFlightTime; t += 0.5f)
		{
			Vector3 psample = base.transform.position + xForceDir * initialpXVelocity * t + yForceDir * (initialpYVelocity * t + 0.5f * (Physics.gravity.y + playerGravityMod.y) * (t * t));
			Gizmos.DrawLine(lastpPos, psample);
			lastpPos = psample;
		}
		Gizmos.DrawLine(lastpPos, base.transform.TransformPoint(localTarget));
	}
}
