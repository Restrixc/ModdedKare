using UnityEngine;

public class PachinkoBallZone : MonoBehaviour
{
	public Pachinko pachinkoMachine;

	public int zoneID;

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Bullet")
		{
			pachinkoMachine.ReachedBottom(this);
		}
	}
}
