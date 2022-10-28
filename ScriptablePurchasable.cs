using UnityEngine;


public class ScriptablePurchasable : ScriptableObject
{
	public GameObject display;

	public PhotonGameObjectReference spawnPrefab;

	public float cost;

	private void OnValidate()
	{
		spawnPrefab.OnValidate();
	}

	public static Bounds DisableAllButGraphics(GameObject target)
	{
		Bounds centerBounds = new Bounds(target.transform.position, Vector3.zero);
		Component[] componentsInChildren = target.GetComponentsInChildren<Component>();
		foreach (Component c in componentsInChildren)
		{
			if (c is Renderer)
			{
				centerBounds.Encapsulate((c as Renderer).bounds);
			}
			else if (!(c is MeshFilter) && !(c is LODGroup))
			{
				if (c is Behaviour)
				{
					(c as Behaviour).enabled = false;
				}
				if (c is Rigidbody)
				{
					(c as Rigidbody).isKinematic = true;
				}
			}
		}
		return centerBounds;
	}
}
