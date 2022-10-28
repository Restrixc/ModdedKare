using PenetrationTech;
using Photon.Pun;

public class PenetratorPhotonHelper : MonoBehaviourPun
{
	private Penetrator penetrator;

	private void Start()
	{
		penetrator = GetComponent<Penetrator>();
		penetrator.penetrationStart += OnPenetrationStart;
	}

	private void OnDestroy()
	{
		if (penetrator != null)
		{
			penetrator.penetrationStart -= OnPenetrationStart;
		}
	}

	private void OnPenetrationStart(Penetrable p)
	{
		if (!base.photonView.IsMine)
		{
			return;
		}
		PhotonView other = p.GetComponentInParent<PhotonView>();
		Penetrable[] penetrables = other.GetComponentsInChildren<Penetrable>();
		for (int i = 0; i < penetrables.Length; i++)
		{
			if (penetrables[i] == p)
			{
				base.photonView.RPC("PenetrateRPC", RpcTarget.Others, other.ViewID, i);
			}
		}
	}

	[PunRPC]
	private void PenetrateRPC(int viewID, int penetrableID)
	{
		PhotonView other = PhotonNetwork.GetPhotonView(viewID);
		Penetrable[] penetrables = other.GetComponentsInChildren<Penetrable>();
		if (!penetrator.TryGetPenetrable(out var checkPen) || checkPen != penetrables[penetrableID])
		{
			penetrator.Penetrate(penetrables[penetrableID]);
		}
	}
}
