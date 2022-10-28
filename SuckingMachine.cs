using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SuckingMachine : UsableMachine
{
	[SerializeField]
	private SphereCollider suckZone;

	private HashSet<Rigidbody> trackedRigidbodies;

	private bool sucking;

	private WaitForFixedUpdate waitForFixedUpdate;

	protected virtual void Awake()
	{
		trackedRigidbodies = new HashSet<Rigidbody>();
		waitForFixedUpdate = new WaitForFixedUpdate();
	}

	private Vector3 GetSuckLocation()
	{
		return suckZone.transform.TransformPoint(suckZone.center);
	}

	private float GetSuckRadius()
	{
		return suckZone.transform.lossyScale.x * suckZone.radius;
	}

	[PunRPC]
	protected virtual IEnumerator OnSwallowed(int viewID)
	{
		PhotonView view = PhotonNetwork.GetPhotonView(viewID);
		yield return new WaitForSeconds(0.1f);
		if (!(view == null))
		{
			PhotonNetwork.Destroy(view.gameObject);
		}
	}

	protected virtual bool ShouldStopTracking(Rigidbody body)
	{
		if (body == null)
		{
			return true;
		}
		float distance = Vector3.Distance(body.ClosestPointOnBounds(GetSuckLocation()), GetSuckLocation());
		if (distance > GetSuckRadius() + 1f)
		{
			return true;
		}
		if (distance < 0.1f)
		{
			PhotonView view = body.gameObject.GetComponentInParent<PhotonView>();
			if (view != null && view.IsMine)
			{
				base.photonView.RPC("OnSwallowed", RpcTarget.All, view.ViewID);
			}
			return true;
		}
		return false;
	}

	private IEnumerator SuckAndSwallow()
	{
		sucking = true;
		while (base.isActiveAndEnabled && trackedRigidbodies.Count > 0)
		{
			trackedRigidbodies.RemoveWhere(ShouldStopTracking);
			foreach (Rigidbody body in trackedRigidbodies)
			{
				body.velocity = Vector3.MoveTowards(body.velocity, Vector3.zero, body.velocity.magnitude * Time.deltaTime * 10f);
				body.AddForce((GetSuckLocation() - body.transform.TransformPoint(body.centerOfMass)) * 30f, ForceMode.Acceleration);
			}
			yield return waitForFixedUpdate;
		}
		sucking = false;
	}

	protected virtual void OnTriggerEnter(Collider other)
	{
		Kobold targetKobold = other.GetComponentInParent<Kobold>();
		if (targetKobold != null)
		{
			Player[] playerList = PhotonNetwork.PlayerList;
			foreach (Player player in playerList)
			{
				if ((Kobold)player.TagObject == targetKobold)
				{
					return;
				}
			}
			if (!targetKobold.grabbed && targetKobold.GetComponent<Ragdoller>().ragdolled)
			{
				LocalUse(targetKobold);
			}
			return;
		}
		Rigidbody body = other.GetComponentInParent<Rigidbody>();
		if (body != null && body.gameObject.GetComponent<MoneyPile>() == null)
		{
			trackedRigidbodies.Add(body);
			if (!sucking)
			{
				StartCoroutine(SuckAndSwallow());
			}
		}
	}
}
