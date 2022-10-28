using Photon.Pun;
using UnityEngine;

public class GenericEdible : GenericUsable
{
	[SerializeField]
	private Sprite eatSymbol;

	[SerializeField]
	private GenericReagentContainer container;

	[SerializeField]
	private bool destroyOnEat = true;

	[SerializeField]
	private AudioPack eatSoundPack;

	public override Sprite GetSprite(Kobold k)
	{
		return eatSymbol;
	}

	public override bool CanUse(Kobold k)
	{
		return container.volume > 0.01f && k.bellyContainer.volume < k.bellyContainer.maxVolume;
	}

	public override void LocalUse(Kobold k)
	{
		base.LocalUse(k);
		float spillAmount = Mathf.Min(10f, k.bellyContainer.maxVolume - k.bellyContainer.volume);
		ReagentContents spill = container.Spill(spillAmount);
		base.photonView.RPC("Spill", RpcTarget.Others, spillAmount);
		k.bellyContainer.photonView.RPC("AddMixRPC", RpcTarget.All, spill, base.photonView.ViewID);
	}

	public override void Use()
	{
		if (destroyOnEat)
		{
			PhotonNetwork.Destroy(base.photonView.gameObject);
		}
		GameManager.instance.SpawnAudioClipInWorld(eatSoundPack, base.transform.position);
	}
}
