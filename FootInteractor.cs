using System.Collections;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;

public class FootInteractor : MonoBehaviour, IAdvancedInteractable
{
	private Kobold kobold;

	private static WaitForSeconds waitForSeconds = new WaitForSeconds(3f);

	public void OnInteract(Kobold k)
	{
		StopAllCoroutines();
		if (!kobold.ragdoller.ragdolled)
		{
			kobold.photonView.RPC("PushRagdoll", RpcTarget.All);
		}
	}

	public void Start()
	{
		kobold = GetComponentInParent<Kobold>();
	}

	public void OnEndInteract()
	{
		StartCoroutine(WaitThenStand());
	}

	public void InteractTo(Vector3 worldPosition, Quaternion worldRotation)
	{
	}

	public bool ShowHand()
	{
		return true;
	}

	public bool PhysicsGrabbable()
	{
		return true;
	}

	private IEnumerator WaitThenStand()
	{
		yield return waitForSeconds;
		kobold.photonView.RPC("PopRagdoll", RpcTarget.All);
	}

	
}
