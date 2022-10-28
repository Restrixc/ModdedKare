using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class IceSpawnerUsable : GenericUsable
{
	[SerializeField]
	private float cost = 20f;

	[SerializeField]
	private MoneyFloater floater;

	[SerializeField]
	private Sprite buySprite;

	[SerializeField]
	private Transform spawnLocation;

	public PhotonGameObjectReference prefabSpawn;

	private void Start()
	{
		Bounds newBounds = new Bounds(base.transform.position, Vector3.zero);
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer r in componentsInChildren)
		{
			newBounds.Encapsulate(r.bounds);
		}
		floater.SetBounds(newBounds);
		floater.SetText(cost.ToString());
	}

	public override bool CanUse(Kobold k)
	{
		return k.GetComponent<MoneyHolder>().HasMoney(cost);
	}

	public override Sprite GetSprite(Kobold k)
	{
		return buySprite;
	}

	public override void LocalUse(Kobold k)
	{
		k.GetComponent<MoneyHolder>().ChargeMoney(cost);
		base.photonView.RPC("RPCUse", RpcTarget.All);
	}

	[PunRPC]
	public override void Use()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.InstantiateRoomObject(prefabSpawn.photonName, spawnLocation.position, spawnLocation.rotation, 0);
		}
	}

	private void OnValidate()
	{
		prefabSpawn.OnValidate();
	}
}
