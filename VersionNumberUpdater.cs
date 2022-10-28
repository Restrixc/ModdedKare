using TMPro;
using UnityEngine;

public class VersionNumberUpdater : MonoBehaviour
{
	private void Start()
	{
		GetComponent<TextMeshProUGUI>().text = Application.version.ToString();
	}
}
