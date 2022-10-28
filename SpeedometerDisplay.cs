using TMPro;
using UnityEngine;

public class SpeedometerDisplay : MonoBehaviour
{
	public TMP_Text textTarget;

	public TMP_Text otherTextTarget;

	public Rigidbody body;

	public Gradient colorGradient;

	private string[] cachedText;

	private int maxSize = 32;

	private void Start()
	{
		cachedText = new string[maxSize];
		for (int i = 0; i < maxSize; i++)
		{
			string cachedString = "";
			for (int o = 0; o < i; o++)
			{
				cachedString += "|";
			}
			cachedString += "]";
			cachedText[i] = cachedString;
		}
	}

	private void Update()
	{
		Vector3 velocity = body.velocity;
		float? y = 0f;
		float v = velocity.With(null, y).magnitude;
		textTarget.color = colorGradient.Evaluate(v / (float)maxSize);
		textTarget.text = cachedText[Mathf.RoundToInt(Mathf.Clamp(v, 0f, maxSize - 1))];
		otherTextTarget.text = v.ToString("0.00");
		otherTextTarget.color = textTarget.color;
	}
}
