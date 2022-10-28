using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StealFocus : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(WaitABit());
	}

	private IEnumerator WaitABit()
	{
		yield return new WaitForSeconds(0.1f);
		GetComponent<Selectable>().Select();
	}
}
