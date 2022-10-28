using UnityEngine;

public class RandomizeRotation : MonoBehaviour
{
	private void Start()
	{
		base.transform.rotation *= Quaternion.AngleAxis(Random.Range(0, 360), base.transform.up);
	}
}
