using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;

public class GenericSpoilable : MonoBehaviourPun, ISpoilable
{
	private void Start()
	{
		SpoilableHandler.AddSpoilable(this);
	}

	private void OnDestroy()
	{
		SpoilableHandler.RemoveSpoilable(this);
	}

	public void OnSpoil()
	{
		if (base.photonView.IsMine)
		{
			PhotonNetwork.Destroy(base.photonView.gameObject);
		}
	}

	
}
