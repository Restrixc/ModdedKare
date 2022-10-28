using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SphereCollider))]
public class KoboldSeeker : MonoBehaviour
{
	public List<Kobold> nearbyKobolds = new List<Kobold>();

	public Kobold curTarget;

	public NavMeshAgent agent;

	public Vector3 home;

	[Range(0.05f, 0.5f)]
	public float sensorCheck;

	[Range(5f, 400f)]
	public float sensorRange;

	[Range(0f, 30f)]
	public int sensorFailedChecks;

	[Range(0f, 30f)]
	public int maxSensorFailedChecks;

	public LayerMask physMask;

	private SphereCollider sphereCollider;

	private void Start()
	{
		home = base.transform.position;
		sphereCollider = GetComponent<SphereCollider>();
		sphereCollider.radius = sensorRange;
		StartCoroutine(DoubleCheck());
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!nearbyKobolds.Contains(other.GetComponent<Kobold>()))
		{
			nearbyKobolds.Add(other.GetComponent<Kobold>());
			if (curTarget == null)
			{
				SeekNewTarget();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (nearbyKobolds.Contains(other.GetComponent<Kobold>()))
		{
			nearbyKobolds.Remove(other.GetComponent<Kobold>());
			if (curTarget == other.GetComponent<Kobold>())
			{
				curTarget = null;
				SeekNewTarget();
			}
		}
	}

	private void Update()
	{
		if (curTarget != null)
		{
			Color drawColor = Color.white;
			if (!Physics.Raycast(base.transform.position, curTarget.transform.position - base.transform.position, out var hit, sensorRange, physMask))
			{
				return;
			}
			if (hit.collider.gameObject.tag == "Player")
			{
				drawColor = Color.red;
				agent.SetDestination(curTarget.transform.position);
			}
			else if (curTarget != null)
			{
				if (sensorFailedChecks >= maxSensorFailedChecks)
				{
					curTarget = null;
					sensorFailedChecks = 0;
					agent.SetDestination(home);
				}
				if (sensorFailedChecks < maxSensorFailedChecks)
				{
					sensorFailedChecks++;
				}
			}
		}
		else
		{
			agent.SetDestination(home);
		}
	}

	private void SeekNewTarget(int listPos = 0)
	{
		Debug.Log("Seeking new target");
		if (nearbyKobolds.Count != 0)
		{
			sensorFailedChecks = 0;
			curTarget = nearbyKobolds[listPos];
			agent.SetDestination(curTarget.transform.position);
		}
		else
		{
			curTarget = null;
		}
	}

	private IEnumerator DoubleCheck()
	{
		while (true)
		{
			yield return new WaitForSeconds(sensorCheck);
			if (!(curTarget == null))
			{
				continue;
			}
			int i;
			RaycastHit hit;
			for (i = 0; i < nearbyKobolds.Count; i++)
			{
				if (nearbyKobolds[i] == null)
				{
					nearbyKobolds.Remove(nearbyKobolds[i]);
					break;
				}
				if (Physics.Raycast(base.transform.position, nearbyKobolds[i].transform.position - base.transform.position, out hit, sensorRange, physMask) && hit.collider.gameObject.tag == "Player")
				{
					SeekNewTarget(nearbyKobolds.FindIndex((Kobold x) => x == nearbyKobolds[i]));
					break;
				}
			}
			hit = default(RaycastHit);
		}
	}
}
