using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

[Serializable]
public class ExplosionReaction : ReagentReaction
{
	private class ExplosionBehaviour : MonoBehaviourPun
	{
		public AudioPack sizzle;

		public GameObject explosion;

		public GenericReagentContainer container;

		private void Start()
		{
			GameManager.instance.SpawnAudioClipInWorld(sizzle, base.transform.position);
			StartCoroutine(ExplosionRoutine());
		}

		private IEnumerator ExplosionRoutine()
		{
			yield return new WaitForSeconds(3f);
			if (base.photonView.IsMine)
			{
				PhotonNetwork.Instantiate(explosion.name, base.transform.position, Quaternion.identity, 0);
				container.photonView.RPC("Spill", RpcTarget.All, container.volume);
			}
		}
	}

	[SerializeField]
	private AudioPack sizzle;

	[SerializeField]
	private GameObject explosion;

	public override void React(GenericReagentContainer container)
	{
		base.React(container);
		if (!container.TryGetComponent<ExplosionBehaviour>(out var behaviour))
		{
			behaviour = container.gameObject.AddComponent<ExplosionBehaviour>();
			behaviour.sizzle = sizzle;
			behaviour.explosion = explosion;
			behaviour.container = container;
		}
	}
}
