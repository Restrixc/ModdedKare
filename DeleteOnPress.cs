using UnityEngine;

public class DeleteOnPress : MonoBehaviour
{
	public GameObject what;

	public void Execute()
	{
		Object.Destroy(what);
	}
}
