using TMPro;
using UnityEngine;

public class DonationSliderHelper : MonoBehaviour
{
	public TextMeshProUGUI label;

	public void updateText(float val)
	{
		label.text = val.ToString();
	}
}
