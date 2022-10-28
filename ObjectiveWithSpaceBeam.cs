using System;
using Photon.Pun;
using UnityEngine;

[Serializable]
public class ObjectiveWithSpaceBeam : DragonMailObjective
{
	[SerializeField]
	protected PhotonGameObjectReference spaceBeam;

	[SerializeField]
	protected Transform spaceBeamTarget;

	protected GameObject spaceBeamInstance;

	public override void Register()
	{
		base.Register();
		if (spaceBeamTarget == null)
		{
			Debug.LogWarning("Couldn't find target to spawn space beam.");
		}
		else
		{
			spaceBeamInstance = PhotonNetwork.Instantiate(spaceBeam.photonName, spaceBeamTarget.transform.position, Quaternion.AngleAxis(-90f, Vector3.right), 0);
		}
	}

	public override void Unregister()
	{
		base.Unregister();
		if (spaceBeamInstance != null && spaceBeamInstance.GetPhotonView().IsMine)
		{
			PhotonNetwork.Destroy(spaceBeamInstance);
		}
	}

	public override void OnValidate()
	{
		base.OnValidate();
		spaceBeam.OnValidate();
	}
}
