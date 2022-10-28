using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InitialVelocityDrag : MonoBehaviour
{
	public Vector3 targetPoint;

	public float travelTime = 10f;

	private void Start()
	{
		Rigidbody body = GetComponent<Rigidbody>();
		Vector3 terminalVelocity = GetTerminalVelocity(Physics.gravity, body);
		float i = Physics.gravity.magnitude / (2f * terminalVelocity.magnitude);
		Vector3 windVelocity = Vector3.zero;
		Vector3 vSubInf = terminalVelocity + windVelocity;
		Vector3 initialVelocity = i * (targetPoint - base.transform.position - vSubInf * travelTime);
		initialVelocity += (targetPoint - base.transform.position) / travelTime;
		body.velocity = initialVelocity;
		StartCoroutine(WaitAndThenShow(travelTime));
	}

	public IEnumerator WaitAndThenShow(float duration)
	{
		yield return new WaitForSeconds(duration);
		Debug.DrawLine(base.transform.position, base.transform.position + Vector3.up * 10f, Color.blue, 10f);
		Start();
	}

	public static Vector3 GetTerminalVelocity(Vector3 gravity, Rigidbody body)
	{
		return (gravity / body.drag - Time.fixedDeltaTime * gravity) / body.mass;
	}
}
