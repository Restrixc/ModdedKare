using System.Collections;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class GenericPurchasable : GenericUsable, IPunObservable, ISavable
{
	public delegate void PurchasableChangedAction(ScriptablePurchasable newPurchasable);

	[SerializeField]
	private Sprite displaySprite;

	[SerializeField]
	private ScriptablePurchasable purchasable;

	[SerializeField]
	private AudioPack purchaseSoundPack;

	private GameObject display;

	private AudioSource source;

	[SerializeField]
	private MoneyFloater floater;

	public PurchasableChangedAction purchasableChanged;

	private bool inStock => display.activeInHierarchy;

	public ScriptablePurchasable GetPurchasable()
	{
		return purchasable;
	}

	public virtual void Start()
	{
		source = base.gameObject.AddComponent<AudioSource>();
		source.spatialBlend = 1f;
		source.rolloffMode = AudioRolloffMode.Custom;
		source.minDistance = 0f;
		source.maxDistance = 25f;
		source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, GameManager.instance.volumeCurve);
		source.outputAudioMixerGroup = GameManager.instance.soundEffectGroup;
		SwapTo(purchasable, forceRefresh: true);
	}

	public override Sprite GetSprite(Kobold k)
	{
		return displaySprite;
	}

	protected void SwapTo(ScriptablePurchasable newPurchasable, bool forceRefresh = false)
	{
		if (!(purchasable == newPurchasable) || forceRefresh)
		{
			if (display != null)
			{
				Object.Destroy(display);
			}
			purchasable = newPurchasable;
			display = Object.Instantiate(purchasable.display, base.transform);
			Bounds centerBounds = ScriptablePurchasable.DisableAllButGraphics(display);
			floater.SetBounds(centerBounds);
			display.SetActive(inStock);
			floater.SetText(purchasable.cost.ToString());
			purchasableChanged?.Invoke(purchasable);
		}
	}

	public virtual void OnDestroy()
	{
	}

	public virtual void OnRestock(object nothing)
	{
		if (!display.activeInHierarchy)
		{
			display.SetActive(value: true);
			floater.gameObject.SetActive(value: true);
		}
	}

	public override void LocalUse(Kobold k)
	{
		base.photonView.RPC("RPCUse", RpcTarget.All);
		k.GetComponent<MoneyHolder>().ChargeMoney(purchasable.cost);
	}

	public override bool CanUse(Kobold k)
	{
		return display.activeInHierarchy && (k == null || k.GetComponent<MoneyHolder>().HasMoney(purchasable.cost));
	}

	[PunRPC]
	public override void Use()
	{
		purchaseSoundPack.Play(source);
		floater.gameObject.SetActive(value: false);
		display.SetActive(value: false);
		if (PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.InstantiateRoomObject(purchasable.spawnPrefab.photonName, base.transform.position, Quaternion.identity, 0, new object[1] { new KoboldGenes().Randomize() });
			StartCoroutine(Restock());
		}
	}

	public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(inStock);
			stream.SendNext(PurchasableDatabase.GetID(purchasable));
		}
		else
		{
			display.SetActive((bool)stream.ReceiveNext());
			short currentPurchasable = (short)stream.ReceiveNext();
			SwapTo(PurchasableDatabase.GetPurchasable(currentPurchasable));
		}
	}

	public override void Save(BinaryWriter writer)
	{
		base.Save(writer);
		writer.Write(inStock);
		writer.Write(PurchasableDatabase.GetID(purchasable));
	}

	public override void Load(BinaryReader reader)
	{
		base.Load(reader);
		display.SetActive(reader.ReadBoolean());
		short currentPurchasable = reader.ReadInt16();
		SwapTo(PurchasableDatabase.GetPurchasable(currentPurchasable));
	}

	private IEnumerator Restock()
	{
		yield return new WaitForSeconds(30f);
		OnRestock(null);
	}
}
