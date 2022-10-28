using UnityEngine;

public class ActionStation : MonoBehaviour
{
	public bool completedAction = false;

	private bool use;

	public AnimationClip clip;

	public float duration = -1f;

	public bool inUse
	{
		get
		{
			return use;
		}
		set
		{
			use = value;
			if (use)
			{
				Collider[] components = GetComponents<Collider>();
				foreach (Collider c2 in components)
				{
					c2.enabled = false;
				}
			}
			else
			{
				Collider[] components2 = GetComponents<Collider>();
				foreach (Collider c in components2)
				{
					c.enabled = true;
				}
			}
		}
	}

	public void Start()
	{
		if (duration == -1f)
		{
			duration = clip.length;
		}
	}
}
