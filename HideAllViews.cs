using System.Collections.Generic;
using UnityEngine;

public class HideAllViews : MonoBehaviour
{
	public List<GameObject> views = new List<GameObject>();

	public void HideThem()
	{
		foreach (GameObject view in views)
		{
			view.SetActive(value: false);
		}
	}
}
