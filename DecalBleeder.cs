using SkinnedMeshDecals;
using UnityEngine;

public class DecalBleeder : MonoBehaviour
{
	public Material decalMat;

	public LayerMask _hitLayers;

	private Vector3 _lastPos = Vector3.zero;

	private void OnCollisionEnter(Collision collision)
	{
		if (((1 << collision.collider.gameObject.layer) & (int)_hitLayers) != 0)
		{
			Vector3 normal = Vector3.zero;
			Vector3 point = Vector3.zero;
			ContactPoint[] contacts = collision.contacts;
			for (int i = 0; i < contacts.Length; i++)
			{
				ContactPoint p = contacts[i];
				normal += p.normal;
				point += p.point;
			}
			normal /= (float)collision.contacts.Length;
			point /= (float)collision.contacts.Length;
			decalMat.color = Color.red;
			PaintDecal.RenderDecalForCollision(collision.collider, decalMat, point, normal, Random.Range(0f, 360f), Vector2.one * 3f);
		}
	}
}
