using UnityEngine;

public class KoboldPickupable : MonoBehaviour
{
	public Animator KoboldAnimator;

	private float carried = 0f;

	private float pickedUp = 0f;

	private float transSpeed = 1f;

	public void OnGrab()
	{
		pickedUp = 1f;
		transSpeed = 5f;
	}

	public void OnRelease()
	{
		pickedUp = 0f;
		transSpeed = 1f;
	}

	private void Update()
	{
		carried = Mathf.MoveTowards(carried, pickedUp, Time.deltaTime * transSpeed);
		KoboldAnimator.SetFloat("Carried", carried);
	}
}
