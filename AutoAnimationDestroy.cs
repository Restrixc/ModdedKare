using UnityEngine;

public class AutoAnimationDestroy : MonoBehaviour
{
	public float delay = 0f;

	public GameObject destroy;

	private void Start()
	{
		Object.Destroy(destroy, GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + delay);
	}
}
