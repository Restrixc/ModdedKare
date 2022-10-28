using System.IO;
using Photon.Pun;
using UnityEngine;

public class GenericUsable : MonoBehaviourPun, ISavable, IPunObservable
{
	public virtual Sprite GetSprite(Kobold k)
	{
		return null;
	}

	public virtual bool CanUse(Kobold k)
	{
		return true;
	}

	public virtual void LocalUse(Kobold k)
	{
		base.photonView.RPC("RPCUse", RpcTarget.All);
	}

	public virtual void Use()
	{
	}

	[PunRPC]
	public void RPCUse()
	{
		Use();
	}

	public virtual void Save(BinaryWriter writer)
	{
	}

	public virtual void Load(BinaryReader reader)
	{
	}

	public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}
}
