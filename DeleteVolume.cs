using Photon.Pun;
using UnityEngine;

public class DeleteVolume : MonoBehaviour
{
	[SerializeField]
	private Transform bucketRespawnPoint;

	private void OnTriggerEnter(Collider other)
	{
		PhotonView view = other.GetComponentInParent<PhotonView>();
		if (view == null || !view.IsMine)
		{
			return;
		}
		if (view.TryGetComponent<BucketWeapon>(out var bucket))
		{
			bucket.transform.position = bucketRespawnPoint.position;
			bucket.GetComponent<Rigidbody>().velocity = Vector3.zero;
			return;
		}
		if (view.TryGetComponent<Rigidbody>(out var body))
		{
			Debug.Log("Destroying view at " + view.transform.position.ToString() + " going speed " + body.velocity.magnitude);
		}
		PhotonNetwork.Destroy(view);
	}
}
