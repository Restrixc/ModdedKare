using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionPopulator : MonoBehaviour
{
	private void Start()
	{
		List<TMP_Dropdown.OptionData> optionData = new List<TMP_Dropdown.OptionData>();
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution r = resolutions[i];
			optionData.Add(new TMP_Dropdown.OptionData(r.width + "x" + r.height + " [" + r.refreshRate + "]"));
		}
		GetComponent<TMP_Dropdown>().AddOptions(optionData);
	}
}
