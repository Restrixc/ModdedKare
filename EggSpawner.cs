using System.Collections.Generic;
using PenetrationTech;
using Photon.Pun;
using UnityEngine;

public class EggSpawner : MonoBehaviour
{
	private class PenetratorCoupler
	{
		public Penetrator penetrator;

		public Penetrable penetrable;

		public Rigidbody body;

		public float pushAmount;
	}

	public Penetrable targetPenetrable;

	[Range(0f, 1f)]
	public float spawnAlongLength = 0.5f;

	[Range(-1f, 1f)]
	public float pushDirection = -1f;

	public PhotonGameObjectReference penetratorPrefab;

	private List<PenetratorCoupler> penetrators;

	private void Awake()
	{
		penetrators = new List<PenetratorCoupler>();
	}

	public void Update()
	{
		for (int i = 0; i < penetrators.Count; i++)
		{
			PenetratorCoupler coupler = penetrators[i];
			if (coupler.pushAmount < coupler.penetrator.GetWorldLength())
			{
				CatmullSpline path = coupler.penetrable.GetSplinePath();
				Vector3 position = path.GetPositionFromT(0f);
				Vector3 tangent = path.GetVelocityFromT(0f).normalized;
				coupler.pushAmount += Time.deltaTime * 0.4f;
				coupler.body.transform.position = position - tangent * coupler.pushAmount;
			}
			else
			{
				coupler.body.isKinematic = false;
				penetrators.RemoveAt(i);
			}
		}
	}

	public Penetrator SpawnEgg(float eggVolume)
	{
		CatmullSpline path = targetPenetrable.GetSplinePath();
		Penetrator d = PhotonNetwork.Instantiate(penetratorPrefab.photonName, path.GetPositionFromT(0f), Quaternion.LookRotation(path.GetVelocityFromT(0f).normalized, Vector3.up), 0).GetComponentInChildren<Penetrator>();
		if (d == null)
		{
			return null;
		}
		if (false)
        {
			Rigidbody body = d.GetComponentInChildren<Rigidbody>();
			d.GetComponent<GenericReagentContainer>().OverrideReagent(ReagentDatabase.GetReagent("ScrambledEgg"), eggVolume);
			body.isKinematic = true;
			d.Penetrate(targetPenetrable);
			penetrators.Add(new PenetratorCoupler
			{
				penetrable = targetPenetrable,
				penetrator = d,
				body = body,
				pushAmount = 0f
			});
        }
        else
        {
			Debug.Log("Trooll");
        }
		return d;
	}

	public void OnValidate()
	{
	}
}
