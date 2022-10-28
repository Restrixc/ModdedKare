using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(GenericReagentContainer))]
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Animator))]
public class BombUsable : GenericUsable, IDamagable
{
	[SerializeField]
	private Sprite bombSprite;

	private bool fired = false;

	private Animator animator;

	[SerializeField]
	private VisualEffect effect;

	private GenericReagentContainer container;

	private void Start()
	{
		container = GetComponent<GenericReagentContainer>();
		animator = GetComponent<Animator>();
	}

	public override Sprite GetSprite(Kobold k)
	{
		return bombSprite;
	}

	[PunRPC]
	public override void Use()
	{
		base.Use();
		Fire();
	}

	private void Fire()
	{
		if (!fired)
		{
			effect.gameObject.SetActive(value: true);
			animator.SetTrigger("Burn");
			if (base.photonView.IsMine)
			{
				ReagentContents contents = new ReagentContents();
				contents.AddMix(ReagentDatabase.GetReagent("Water").GetReagent(40f));
				container.photonView.RPC("AddMixRPC", RpcTarget.All, contents, container.photonView.ViewID);
			}
			fired = true;
		}
	}

	public float GetHealth()
	{
		return 1f;
	}

	[PunRPC]
	public void Damage(float amount)
	{
		if (!fired)
		{
			Fire();
		}
		else if (base.photonView.IsMine)
		{
			PhotonNetwork.Destroy(base.gameObject);
		}
	}

	public void Heal(float amount)
	{
	}

	
}
