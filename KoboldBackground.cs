using Naelstrof.Easing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KoboldBackground : MonoBehaviour
{
	[SerializeField]
	private Image kobold;

	[SerializeField]
	private Image trees;

	[SerializeField]
	private Image mountain;

	private void Update()
	{
		RectTransform rk = kobold.GetComponent<RectTransform>();
		RectTransform tk = trees.GetComponent<RectTransform>();
		RectTransform mk = mountain.GetComponent<RectTransform>();
		rk.localScale = new Vector3(1f, 1f + Mathf.Sin(Time.unscaledTime * 2.2f) * 0.03f, 1f);
		Vector2 mousePos = Mouse.current.position.ReadValue();
		Vector2 offset = mousePos - new Vector2(Screen.width, Screen.height) * 0.5f;
		float distance = Mathf.Clamp01(new Vector2(offset.x * 2f / (float)Screen.width, offset.y / (float)Screen.height).magnitude * 2f);
		rk.anchoredPosition = -offset * (Easing.Quadratic.Out(distance) * 0.1f);
		tk.anchoredPosition = -offset * (Easing.Quadratic.Out(distance) * 0.05f);
		mk.anchoredPosition = -offset * (Easing.Quadratic.Out(distance) * 0.025f);
	}
}
