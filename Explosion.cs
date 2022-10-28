using System.Collections.Generic;
using Photon.Pun;
using SkinnedMeshDecals;
using UnityEngine;

public class Explosion : MonoBehaviourPun
{
	[SerializeField]
	private LayerMask playerMask;

	[SerializeField]
	private Material scorchDecal;

	private void Start()
	{
		PaintDecal.RenderDecalInBox(Vector3.one * 4f, base.transform.position, scorchDecal, Quaternion.FromToRotation(Vector3.forward, Vector3.down), GameManager.instance.decalHitMask);
		if (!base.photonView.IsMine)
		{
			return;
		}
		List<Kobold> kobolds = new List<Kobold>();
		SoilTile bestTile = null;
		float bestTileDistance = float.MaxValue;
		Collider[] array = Physics.OverlapSphere(base.transform.position, 5f, playerMask, QueryTriggerInteraction.Ignore);
		foreach (Collider c in array)
		{
			scorchDecal.color = Color.black;
			Kobold i = c.GetComponentInParent<Kobold>();
			if (i != null && !kobolds.Contains(i))
			{
				kobolds.Add(i);
				Rigidbody[] ragdollBodies = i.ragdoller.GetRagdollBodies();
				foreach (Rigidbody r in ragdollBodies)
				{
					r.AddExplosionForce(3000f, base.transform.position, 5f);
				}
				i.body.AddExplosionForce(3000f, base.transform.position, 5f);
				i.StartCoroutine(i.ThrowRoutine());
			}
			else
			{
				c.GetComponentInParent<Rigidbody>()?.AddExplosionForce(3000f, base.transform.position, 5f);
			}
			SoilTile tile = c.GetComponentInParent<SoilTile>();
			if (tile != null && tile.GetDebris())
			{
				float distance = Vector3.Distance(base.transform.position, tile.transform.position);
				if (distance < bestTileDistance)
				{
					bestTile = tile;
					bestTileDistance = distance;
				}
			}
			IDamagable damagable = c.GetComponentInParent<IDamagable>();
			if (damagable != null)
			{
				float dist = Vector3.Distance(base.transform.position, c.ClosestPoint(base.transform.position));
				float damage = Mathf.Clamp01((5f - dist) / 5f) * 250f;
				damagable.photonView.RPC("Damage", RpcTarget.All, damage);
			}
		}
		if (bestTile != null)
		{
			bestTile.photonView.RPC("SetDebris", RpcTarget.All, false);
		}
	}
}
