using System.IO;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MoneyPile : GenericUsable, IPunInstantiateMagicCallback, IOnPhotonViewControllerChange, IPhotonViewCallback
{
	private float internalWorth;

	private Kobold tryingToEquip;

	[SerializeField]
	private GameObject[] displays;

	[SerializeField]
	private AnimationCurve moneyMap;

	[SerializeField]
	private Sprite useSprite;

	private float worth
	{
		get
		{
			return internalWorth;
		}
		set
		{
			internalWorth = value;
			int targetIndex = (int)Mathf.Log(value, 5f);
			targetIndex = Mathf.Clamp(targetIndex, 0, displays.Length - 1);
			for (int i = 0; i < displays.Length; i++)
			{
				displays[i].SetActive(i == targetIndex);
			}
		}
	}

	public override Sprite GetSprite(Kobold k)
	{
		return useSprite;
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (info.photonView.InstantiationData != null && info.photonView.InstantiationData.Length != 0)
		{
			worth = (float)info.photonView.InstantiationData[0];
		}
	}

	public override void LocalUse(Kobold k)
	{
		if (k.photonView.IsMine && !base.photonView.IsMine && tryingToEquip == null)
		{
			tryingToEquip = k;
			base.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
		}
		if (k.photonView.IsMine && base.photonView.IsMine)
		{
			k.GetComponent<MoneyHolder>().AddMoney(worth);
			PhotonNetwork.Destroy(base.photonView.gameObject);
		}
	}

	public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(worth);
		}
		else
		{
			worth = (float)stream.ReceiveNext();
		}
	}

	public override void Save(BinaryWriter writer)
	{
		base.Save(writer);
		writer.Write(worth);
	}

	public override void Load(BinaryReader reader)
	{
		base.Load(reader);
		worth = reader.ReadSingle();
	}

	public void OnControllerChange(Player newController, Player previousController)
	{
		if (tryingToEquip.photonView.IsMine && newController == PhotonNetwork.LocalPlayer)
		{
			tryingToEquip.GetComponent<MoneyHolder>().AddMoney(worth);
			PhotonNetwork.Destroy(base.photonView.gameObject);
		}
	}
}
