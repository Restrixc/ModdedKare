using Photon.Pun;
using UnityEngine;

public class DragonMailInteractable : GenericUsable, IPunObservable
{
	public AudioSource src;

	public Canvas tgt;

	public DragonMailHandler dmHandler;

	public Sprite displaySprite;

	public override Sprite GetSprite(Kobold k)
	{
		return displaySprite;
	}

	public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	public override void Use()
	{
		if (base.photonView.IsMine)
		{
			base.Use();
			DragonMailHandler.inst.Toggle();
		}
	}
}
