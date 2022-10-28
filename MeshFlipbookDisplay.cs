using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshFlipbookDisplay : MonoBehaviour
{
	private MeshFilter filter;

	[SerializeField]
	private MeshFlipbookData meshFlipbookData;

	private void Start()
	{
		filter = GetComponent<MeshFilter>();
	}

	private void Update()
	{
		int frame = Mathf.RoundToInt(Time.time * meshFlipbookData.fps) % meshFlipbookData.meshes.Length;
		filter.mesh = meshFlipbookData.meshes[frame];
	}
}
