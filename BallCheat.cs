using System.Collections;
using Photon.Pun;
using UnityEngine;

public class BallCheat : MonoBehaviourPun, IPunInstantiateMagicCallback
{
	private Rigidbody rb;

	private Pachinko pachinko;

	public float sensitivity;

	private WaitForFixedUpdate waitForFixedUpdate;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		waitForFixedUpdate = new WaitForFixedUpdate();
	}

	private void Start()
	{
		StartCoroutine(StuckCheckRoutine());
	}

	private void OnCollisionEnter(Collision collisionInfo)
	{
		pachinko.HitPin();
	}

	private IEnumerator StuckCheckRoutine()
	{
		int stuckCount = 0;
		while (stuckCount < 60)
		{
			while (rb.velocity.magnitude > sensitivity)
			{
				yield return waitForFixedUpdate;
			}
			stuckCount++;
			yield return waitForFixedUpdate;
		}
		pachinko.BallStuck();
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		pachinko = PhotonNetwork.GetPhotonView((int)info.photonView.InstantiationData[0]).GetComponent<Pachinko>();
	}
}
