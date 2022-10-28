using UnityEngine;

public class PachinkoExplodeOnTouch : MonoBehaviour
{
	private void OnCollisionStay(Collision collisionInfo)
	{
		if (collisionInfo.gameObject.CompareTag("Bullet"))
		{
			collisionInfo.rigidbody.AddExplosionForce(1000f, base.transform.position, 5f, 10f, ForceMode.Impulse);
		}
	}
}
