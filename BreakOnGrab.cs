using System.IO;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(Rigidbody))]
public class BreakOnGrab : MonoBehaviourPun, IPunObservable, ISavable, IGrabbable
{
	private bool grabbed = false;

	private AudioSource source;

	[SerializeField]
	private GameObject disableOnGrab;

	private Rigidbody body;

	private void Start()
	{
		source = GetComponent<AudioSource>();
		body = GetComponent<Rigidbody>();
		PlayAreaEnforcer.AddTrackedObject(base.photonView);
	}

	private void OnDestroy()
	{
		PlayAreaEnforcer.RemoveTrackedObject(base.photonView);
	}

	private void SetState(bool newGrabbed)
	{
		if (grabbed != newGrabbed)
		{
			grabbed = newGrabbed;
			disableOnGrab.SetActive(!grabbed);
			body.isKinematic = !grabbed;
			if (grabbed)
			{
				source.Play();
			}
		}
	}

	public bool CanGrab(Kobold kobold)
	{
		return true;
	}

	[PunRPC]
	public void OnGrabRPC(int koboldID)
	{
		if (base.photonView.IsMine)
		{
			SetState(newGrabbed: true);
		}
	}

	[PunRPC]
	public void OnReleaseRPC(int koboldID, Vector3 velocity)
	{
	}

	public Transform GrabTransform()
	{
		return base.transform;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(grabbed);
		}
		else
		{
			SetState((bool)stream.ReceiveNext());
		}
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(grabbed);
	}

	public void Load(BinaryReader reader)
	{
		SetState(reader.ReadBoolean());
	}

	
}
