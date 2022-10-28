using System.Collections;
using UnityEngine;

public class ConstantRotate : MonoBehaviour
{
	public float rotateAmount;

	public float rotateSteps;

	public bool rotate = true;

	public void Start()
	{
		StartCoroutine(rotation());
	}

	public IEnumerator rotation()
	{
		while (rotate)
		{
			yield return new WaitForSeconds(rotateSteps);
			base.transform.Rotate(0f, 0f, base.transform.rotation.z + rotateAmount);
		}
	}
}
