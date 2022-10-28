using UnityEngine;
using UnityEngine.VFX;

public class DestroyVFXOnFinish : MonoBehaviour
{
	private VisualEffect effect;

	private bool initialized = false;

	private void Start()
	{
		effect = GetComponentInChildren<VisualEffect>();
	}

	private void FixedUpdate()
	{
		if (!initialized)
		{
			initialized = true;
		}
		else if (effect.aliveParticleCount == 0)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
