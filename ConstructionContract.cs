using System.IO;
using Photon.Pun;
using UnityEngine;

public class ConstructionContract : GenericUsable
{
	public delegate void ConstructionContractPurchaseAction(ConstructionContract contract);

	[SerializeField]
	private Sprite displaySprite;

	[SerializeField]
	private float cost;

	[SerializeField]
	private MoneyFloater floater;

	[SerializeField]
	private AudioPack purchaseSound;

	[SerializeField]
	private int starRequirement = 1;

	private bool bought;

	public static event ConstructionContractPurchaseAction purchasedEvent;

	private void Start()
	{
		Bounds bound = new Bounds(base.transform.position, Vector3.one);
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer r in componentsInChildren)
		{
			bound.Encapsulate(r.bounds);
		}
		floater.SetBounds(bound);
		floater.SetText(cost.ToString());
	}

	public override Sprite GetSprite(Kobold k)
	{
		return displaySprite;
	}

	public override bool CanUse(Kobold k)
	{
		return (k.GetComponent<MoneyHolder>().HasMoney(cost) && !bought && ObjectiveManager.GetStars() > starRequirement) || (ObjectiveManager.GetStars() == starRequirement && ObjectiveManager.GetCurrentObjective() != null);
	}

	protected virtual void SetState(bool purchased)
	{
		bought = purchased;
		foreach (Transform t in base.transform)
		{
			if (!(base.transform == t))
			{
				t.gameObject.SetActive(!purchased);
			}
		}
		Renderer[] components = GetComponents<Renderer>();
		foreach (Renderer r in components)
		{
			r.enabled = !purchased;
		}
	}

	public override void LocalUse(Kobold k)
	{
		if (k.GetComponent<MoneyHolder>().ChargeMoney(cost))
		{
			base.LocalUse(k);
		}
	}

	[PunRPC]
	public override void Use()
	{
		base.Use();
		SetState(purchased: true);
		GameManager.instance.SpawnAudioClipInWorld(purchaseSound, base.transform.position);
		ConstructionContract.purchasedEvent?.Invoke(this);
	}

	public override void Save(BinaryWriter writer)
	{
		writer.Write(bought);
	}

	public override void Load(BinaryReader reader)
	{
		bought = reader.ReadBoolean();
		SetState(bought);
	}

	public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(bought);
		}
		else
		{
			SetState((bool)stream.ReceiveNext());
		}
	}
}
