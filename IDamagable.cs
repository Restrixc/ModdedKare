using Photon.Pun;

public interface IDamagable
{
	PhotonView photonView { get; }

	float GetHealth();

	[PunRPC]
	void Damage(float amount);

	void Heal(float amount);
}
