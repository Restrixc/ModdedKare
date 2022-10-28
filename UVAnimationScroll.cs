using UnityEngine;
using UnityEngine.UI;

public class UVAnimationScroll : MonoBehaviour
{
	public Vector2 scrollSpeed;

	private Vector2 offset;

	private Material material;

	private void Start()
	{
		material = GetComponent<Image>().material;
	}

	private void Update()
	{
		offset = new Vector2(offset.x + Time.time * scrollSpeed.x, offset.y + Time.time * scrollSpeed.y);
		material.mainTextureOffset = offset;
	}
}
