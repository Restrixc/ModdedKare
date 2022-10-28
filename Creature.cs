using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

public class Creature : MonoBehaviourPun, IGrabbable, IDamagable, IPunObservable, IPunInstantiateMagicCallback
{
	[SerializeField]
	private PhotonGameObjectReference spawnOnDeath;

	[SerializeField]
	private VisualEffect splashPrefab;

	[SerializeField]
	private float health = 1f;

	[SerializeField]
	private float speed = 4f;

	[SerializeField]
	private CreaturePath targetPath;

	private float distanceTravelled = 0f;

	private float distanceTravelledVel;

	private float networkedDistanceTravelled = 0f;

	[SerializeField]
	private AudioPack gibSound;

	private void OnValidate()
	{
		spawnOnDeath.OnValidate();
	}

	public bool CanGrab(Kobold kobold)
	{
		return true;
	}

	[PunRPC]
	public void OnGrabRPC(int koboldID)
	{
		Die();
	}

	[PunRPC]
	public void OnReleaseRPC(int koboldID, Vector3 velocity)
	{
	}

	public Transform GrabTransform()
	{
		return base.transform;
	}

	public void Update()
	{
		if (targetPath == null)
		{
			return;
		}
		if (!base.photonView.IsMine)
		{
			if (Mathf.Abs(distanceTravelled - networkedDistanceTravelled) > 5f)
			{
				distanceTravelled = networkedDistanceTravelled;
			}
			distanceTravelled = Mathf.SmoothDamp(distanceTravelled, networkedDistanceTravelled, ref distanceTravelledVel, 1f);
		}
		else
		{
			distanceTravelled += Time.deltaTime * speed;
			if (distanceTravelled > targetPath.GetSpline().arcLength)
			{
				distanceTravelled -= targetPath.GetSpline().arcLength;
			}
		}
		base.transform.position = targetPath.GetSpline().GetPositionFromDistance(distanceTravelled);
		base.transform.forward = targetPath.GetSpline().GetVelocityFromDistance(distanceTravelled).normalized;
	}

	public float GetHealth()
	{
		return health;
	}

	public void Damage(float amount)
	{
		if (!(health < 0f) && base.photonView.IsMine)
		{
			health -= amount;
			if (health < 0f)
			{
				base.photonView.RPC("Die", RpcTarget.All);
			}
		}
	}

	[PunRPC]
	private void Die()
	{
		GameObject effect = Object.Instantiate(splashPrefab.gameObject, base.transform.position, Quaternion.identity);
		effect.GetComponent<VisualEffect>().SetVector4("Color", spawnOnDeath.gameObject.GetComponent<Fruit>().startingReagent.reagent.GetColor());
		GameManager.instance.SpawnAudioClipInWorld(gibSound, base.transform.position);
		Object.Destroy(effect, 5f);
		if (base.photonView.IsMine)
		{
			PhotonNetwork.Instantiate(spawnOnDeath.photonName, base.transform.position, base.transform.rotation, 0);
			PhotonNetwork.Destroy(base.gameObject);
		}
	}

	public void Heal(float amount)
	{
		health += amount;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(health);
			stream.SendNext(distanceTravelled);
		}
		else
		{
			health = (float)stream.ReceiveNext();
			networkedDistanceTravelled = (float)stream.ReceiveNext();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.rigidbody != null && collision.impulse.magnitude > 1f)
		{
			Damage(collision.impulse.magnitude);
		}
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (info.photonView.InstantiationData != null && info.photonView.InstantiationData.Length != 0 && info.photonView.InstantiationData[0] is int)
		{
			targetPath = PhotonNetwork.GetPhotonView((int)info.photonView.InstantiationData[0]).GetComponentInChildren<CreaturePath>();
		}
	}


}
