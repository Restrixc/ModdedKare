using Photon.Pun;
using UnityEngine;

public interface IGrabbable
{
	Transform transform { get; }

	GameObject gameObject { get; }

	PhotonView photonView { get; }

	bool CanGrab(Kobold kobold);

	[PunRPC]
	void OnGrabRPC(int koboldID);

	[PunRPC]
	void OnReleaseRPC(int koboldID, Vector3 velocity);

	Transform GrabTransform();
}
