using System.Reflection;
using UnityEngine;

public class AdultCheckShower : MonoBehaviour
{
	public ScriptableFloat shouldShow;

	

	private void Start()
	{
		if (shouldShow.value != 0f)
		{
			base.gameObject.SetActive(value: false);
		}
		shouldShow.set(0f);
	}
}
